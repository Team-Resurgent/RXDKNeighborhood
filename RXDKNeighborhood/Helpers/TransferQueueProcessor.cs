using Avalonia.Threading;
using RXDKNeighborhood.Models;
using RXDKXBDM;
using RXDKXBDM.Commands;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RXDKNeighborhood.Helpers
{
    public class TransferQueueProcessor
    {
        private readonly CancellationTokenSource _cts = new();

        public ObservableCollection<TransferDetail> TransferDetails { get; } = [];

        public event Action? TransferCompleted;

        public void Start()
        {
            Task.Run(() => ProcessQueueAsync(_cts.Token));
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        private static async Task<bool> DownloadFileAsync(Connection connection, string sourcefile, string destfile, CancellationToken cancellationToken, Action<long, long> progress)
        {
            return await Task.Run(() =>
            {
                using (var fileStream = new FileStream(destfile, FileMode.Create))
                using (var downloadStream = new DownloadStream(fileStream, progress))
                {
                    var response = Download.SendAsync(connection, sourcefile, cancellationToken, downloadStream).Result;
                    if (!Utils.IsSuccess(response.ResponseCode))
                    {
                        return false;
                    }
                }
                return true;
            });
        }

        private static async Task<bool> UploadFileAsync(Connection connection, string sourcefile, long size, string destfile, CancellationToken cancellationToken, Action<long, long> progress)
        {
            return await Task.Run(() =>
            {
                using (var fileStream = new FileStream(sourcefile, FileMode.Open))
                using (var uploadStream = new UploadStream(fileStream, progress) { ExpectedSize = size })
                {
                    var response = SendFile.SendAsync(connection, destfile, size, cancellationToken, uploadStream).Result;
                    if (!Utils.IsSuccess(response.ResponseCode))
                    {
                        return false;
                    }
                }
                return true;
            });
        }

        private async Task ProcessQueueAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var itemsToProcess = TransferDetails.Where(x => !x.Failed).ToArray();
                    if (itemsToProcess.Length == 0)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    foreach (var item in itemsToProcess)
                    {
                        if (item.IsDirectory)
                        {
                            bool createdDirectory = false;
                            try
                            {
                                if (item.TransferType == TransferType.Download)
                                {
                                    using var connection = new Connection();
                                    if (await connection.OpenAsync(item.IpAddress) == true)
                                    {
                                        var dirListResponse = await DirList.SendAsync(connection, item.SourcePath);
                                        if (Utils.IsSuccess(dirListResponse.ResponseCode) != false && dirListResponse.ResponseValue != null)
                                        {
                                            Directory.CreateDirectory(item.DestPath);
                                            foreach (var entry in dirListResponse.ResponseValue)
                                            {
                                                var transferDetail = new TransferDetail
                                                {
                                                    IpAddress = item.IpAddress,
                                                    TransferType = item.TransferType,
                                                    Failed = false,
                                                    Progress = "Pending",
                                                    SourcePath = Path.Combine(entry.Path, entry.Name),
                                                    DestPath = Path.Combine(item.DestPath, entry.Name),
                                                    IsDirectory = entry.IsDirectory,
                                                    FileSize = entry.Size,
                                                };
                                                Dispatcher.UIThread.Invoke(() =>
                                                {
                                                    TransferDetails.Add(transferDetail);
                                                });
                                            }
                                        }
                                    }
                                    createdDirectory = true;
                                }
                                else if (item.TransferType == TransferType.Upload)
                                {
                                    var entries = Directory.EnumerateFileSystemEntries(item.SourcePath);
                                    foreach (var entry in entries)
                                    {
                                        var isDirectory = File.GetAttributes(entry).HasFlag(FileAttributes.Directory);
                                        var transferDetail = new TransferDetail
                                        {
                                            IpAddress = item.IpAddress,
                                            TransferType = item.TransferType,
                                            Failed = false,
                                            Progress = "Pending",
                                            SourcePath = entry,
                                            DestPath = Path.Combine(item.DestPath, Path.GetFileName(entry)),
                                            IsDirectory = isDirectory,
                                            FileSize = isDirectory ? 0 : (ulong)new FileInfo(entry).Length
                                        };
                                        Dispatcher.UIThread.Invoke(() =>
                                        {
                                            TransferDetails.Add(transferDetail);
                                        });
                                    }

                                    using var connection = new Connection();
                                    if (await connection.OpenAsync(item.IpAddress) == true)
                                    {
                                        var response = await MkDir.SendAsync(connection, item.DestPath);
                                        createdDirectory = Utils.IsSuccess(response.ResponseCode);
                                    }
                                }
                            }
                            catch
                            {
                            }

                            Dispatcher.UIThread.Invoke(() =>
                            {
                                if (createdDirectory)
                                {
                                    TransferDetails.Remove(item);
                                }
                                else
                                {
                                    item.Failed = true;
                                    item.Progress = "Failed";
                                }
                            });
                        }
                        else
                        {
                            bool createdFile = false;
                            try
                            {
                                if (item.TransferType == TransferType.Download)
                                {
                                    using var connection = new Connection();
                                    if (await connection.OpenAsync(item.IpAddress) == true)
                                    {
                                        await DownloadFileAsync(connection, item.SourcePath, item.DestPath, item.CancellationTokenSource.Token, (position, size) =>
                                        {
                                            var percent = size > 0 ? (position * 100.0 / size) : 0;
                                            Dispatcher.UIThread.Invoke(() =>
                                            {
                                                item.Progress = $"{percent:0.00}%";
                                            });
                                        });
                                        createdFile = true;
                                    }
                                }
                                else if (item.TransferType == TransferType.Upload)
                                {
                                    using var connection = new Connection();
                                    if (await connection.OpenAsync(item.IpAddress) == true)
                                    {
                                        await UploadFileAsync(connection, item.SourcePath, (long)item.FileSize, item.DestPath, item.CancellationTokenSource.Token, (position, size) =>
                                        {
                                            var percent = size > 0 ? (position * 100.0 / size) : 0;
                                            Dispatcher.UIThread.Invoke(() =>
                                            {
                                                item.Progress = $"{percent:0.00}%";
                                            });
                                        });
                                        createdFile = true;
                                    }
                                }
                            }
                            catch
                            {
                            }

                            Dispatcher.UIThread.Invoke(() =>
                            {
                                if (createdFile)
                                {
                                    TransferDetails.Remove(item);
                                }
                                else
                                {
                                    item.Failed = true;
                                    item.Progress = "Failed";
                                }
                            });
                        }
                    }
                
                    if (!TransferDetails.Where(x => !x.Failed).Any())
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            TransferCompleted?.Invoke();
                        });
                    }
                }

            }
            catch
            {
            }
        }

    }
}
