using RXDKXBDM.Commands;
using RXDKXBDM.Models;
using System.Speech.Recognition;
using Windows.Globalization;
using Windows.Storage.Pickers;

namespace RXDKNeighborhood
{
    public static class Utils
    {
        public static string FormatBytes(long bytes)
        {
            const long KB = 1024;
            const long MB = KB * 1024;
            const long GB = MB * 1024;

            if (bytes >= GB)
                return $"{(double)bytes / GB:0.##} GB";
            if (bytes >= MB)
                return $"{(double)bytes / MB:0.##} MB";
            if (bytes >= KB)
                return $"{(double)bytes / KB:0.##} KB";

            return $"{bytes} bytes";
        }

        public static async Task<DriveItem[]> GetFolderComtents(string sourcefolder, CancellationToken cancellationToken, Action<long> size)
        {
            return await Task<DriveItem[]>.Run(() =>
            {
                var scanFolders = new List<string>
                {
                    sourcefolder
                };

                var totalSize = (long)0;
                var recursiveItems = new List<DriveItem>();

                while (scanFolders.Count > 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return recursiveItems.ToArray();
                    }

                    var dirToList = scanFolders[0] + "\\";
                    scanFolders.RemoveAt(0);

                    var response = DirList.SendAsync(Globals.GlobalConnection, dirToList).Result;
                    if (RXDKXBDM.Utils.IsSuccess(response.ResponseCode) == false)
                    {
                        return [];
                    }
                    for (var i = 0; i < response.ResponseValue.Length; i++)
                    {
                        var item = response.ResponseValue[i];
                        if (item.IsDirectory)
                        {
                            scanFolders.Add(item.CombinePath());
                        }
                        recursiveItems.Add(item);
                        totalSize += item.Size;
                        size.Invoke(totalSize);
                        System.Diagnostics.Debug.Print($"{recursiveItems.Count}");
                    }
                }
                return recursiveItems.ToArray();
            });
        }

        public static async Task<bool> DownloadFolderAsync(string sourcefolder, string destfolder, CancellationToken cancellationToken, Action<long, long> progress)
        {
            return await Task<bool>.Run(() =>
            {
                var scanFolders = new List<string>();
                scanFolders.Add(sourcefolder);

                var totalSize = (long)0;
                var recursiveItems = new List<DriveItem>();

                while (scanFolders.Count > 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return true;
                    }

                    var dirToList = scanFolders[0] + "\\";
                    scanFolders.RemoveAt(0);

                    var response = DirList.SendAsync(Globals.GlobalConnection, dirToList).Result;
                    if (RXDKXBDM.Utils.IsSuccess(response.ResponseCode) == false)
                    {
                        return false;
                    }

                    for (var i = 0; i < response.ResponseValue.Length; i++)
                    {
                        var item = response.ResponseValue[i];
                        if (item.IsDirectory)
                        {
                            scanFolders.Add(item.CombinePath());
                        }
                        recursiveItems.Add(item);
                        totalSize += item.Size;
                    }
                }
                return true;
            });
        }

        public static async Task<bool> DownloadFileAsync(string sourcefile, string destfile, CancellationToken cancellationToken, Action<long, long> progress)
        {
            return await Task<bool>.Run(() =>
            {
                using (var fileStream = new FileStream(destfile, FileMode.Create))
                using (var downloadStream = new DownloadStream(fileStream, progress))
                {
                    var response = Download.SendAsync(Globals.GlobalConnection, sourcefile, cancellationToken, downloadStream).Result;
                    if (!RXDKXBDM.Utils.IsSuccess(response.ResponseCode) || downloadStream.ExpectedSize != downloadStream.Length)
                    {
                        return false;
                    }
                }
                return true;
            });
        }

        public static async Task<string?> FilePicker(Window window, string name)
        {
            var extension = System.IO.Path.GetExtension(name);
            if (string.IsNullOrEmpty(extension))
            {
                extension = ".";
            }

            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads,
                SuggestedFileName = name
            };
            savePicker.FileTypeChoices.Add("Suggested File Types", [extension]);

            if (window.Handler.PlatformView is MauiWinUIWindow mauiWinUIWindow)
            {
                var hwnd = mauiWinUIWindow.WindowHandle;
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
            }
            else
            {
                return null;
            }

            var file = await savePicker.PickSaveFileAsync();
            return file?.Path;
        }
    }
}
