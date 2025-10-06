using RXDKXBDM;
using RXDKXBDM.Commands;
using RXDKXBDM.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RXDKNeighborhood.Helpers
{
    public struct ContentsProgress
    {
        public ulong TotalSize { get; set; }

        public ulong FilesCount { get; set; }

        public ulong FolderCount { get; set; }
    }

    public static class FolderHelper
    {

        public static async Task<FileSystemItem[]> GetFolderComtents(Connection connection, FileSystemItem sourceItem, CancellationToken cancellationToken, Action<ContentsProgress>? progress = null)
        {
            return await Task.Run(() =>
            {
                var scanFolders = new List<string>
                {
                    Path.Combine(sourceItem.Path, sourceItem.Name)
                };

                var totalSize = (ulong)0;
                var fileCount = (ulong)0;
                var folderCount = (ulong)0;
                var recursiveItems = new List<FileSystemItem>
                {
                    sourceItem
                };

                while (scanFolders.Count > 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return [];
                    }

                    var dirToList = scanFolders[0] + "\\";
                    scanFolders.RemoveAt(0);

                    var response = DirList.SendAsync(connection, dirToList).Result;
                    if (Utils.IsSuccess(response.ResponseCode) == false)
                    {
                        return [];
                    }
                    for (var i = 0; i < response.ResponseValue.Length; i++)
                    {
                        var item = response.ResponseValue[i];
                        if (item.IsDirectory)
                        {
                            scanFolders.Add(Path.Combine(item.Path, item.Name));
                        }
                        recursiveItems.Add(item);
                        totalSize += item.Size;
                        if (item.IsDirectory)
                        {
                            folderCount++;
                        }
                        else
                        {
                            fileCount++;
                        }
                        progress?.Invoke(new ContentsProgress { TotalSize = totalSize, FilesCount = fileCount, FolderCount = folderCount });
                    }
                }
                return recursiveItems.ToArray();
            });
        }
    }
}
