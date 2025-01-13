using RXDKXBDM;
using RXDKXBDM.Commands;
using RXDKXBDM.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Windows.Storage.Pickers;

namespace RXDKNeighborhood
{
    public struct ContentsProgress
    {
        public long TotalSize { get; set; }

        public long FilesCount { get; set; }

        public long FolderCount { get; set; }
    }

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

        public static async Task<DriveItem[]> GetFolderComtents(string sourcefolder, CancellationToken cancellationToken, Action<ContentsProgress>? progress = null)
        {
            return await Task.Run(() =>
            {
                var scanFolders = new List<string>
                {
                    sourcefolder
                };

                var totalSize = (long)0;
                var fileCount = (long)0;
                var folderCount = (long)0;
                var recursiveItems = new List<DriveItem>();

                while (scanFolders.Count > 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return [];
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
                    if (!RXDKXBDM.Utils.IsSuccess(response.ResponseCode))
                    {
                        return false;
                    }
                }
                return true;
            });
        }

        private static void AoolyPixelDataToImage(Image<Rgb24> image, uint pitch, uint format, byte[] data)
        {
            const uint D3DFMT_LIN_A8R8G8B8 = 0x00000012;
            const uint D3DFMT_LIN_X8R8G8B8 = 0x0000001E; 
            const uint D3DFMT_LIN_R5G6B5 = 0x00000011; 
            const uint D3DFMT_LIN_X1R5G5B5 = 0x0000001C; 

            var dataOffset = 0;
     
            if (format == D3DFMT_LIN_A8R8G8B8 || format == D3DFMT_LIN_X8R8G8B8)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    dataOffset = (int)(y * pitch);
                    for (var x = 0; x < image.Width; x++)
                    {
                        var b = data[dataOffset + 0];
                        var g = data[dataOffset + 1];
                        var r = data[dataOffset + 2];
                        image[x, y] = new Rgb24(r, g, b);
                        dataOffset += 4;
                    }
                }
            }
            else if (format == D3DFMT_LIN_R5G6B5)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    dataOffset = (int)(y * pitch);
                    for (var x = 0; x < image.Width; x++)
                    {
                        var temp = (ushort)(data[dataOffset + 0] << 8 | data[dataOffset + 1]);
                        var tempR = (temp >> 11) & 0x1F;
                        var tempG = (temp >> 5) & 0x3F;
                        var tempB = temp & 0x1F;
                        var r = (byte)((tempR << 3) | (tempR >> 2));
                        var g = (byte)((tempG << 2) | (tempG >> 4));
                        var b = (byte)((tempB << 3) | (tempB >> 2));
                        image[x, y] = new Rgb24(r, g, b);
                        dataOffset += 2;
                    }
                }
            }
            else if (format == D3DFMT_LIN_X1R5G5B5)
            {
                for (var y = 0; y < image.Height; y++)
                {
                    dataOffset = (int)(y * pitch);
                    for (var x = 0; x < image.Width; x++)
                    {
                        var temp = (ushort)(data[dataOffset + 0] << 8 | data[dataOffset + 1]);
                        var tempR = (temp >> 10) & 0x1F;
                        var tempG = (temp >> 5) & 0x1F;
                        var tempB = temp & 0x1F;
                        var r = (byte)((tempR << 3) | (tempR >> 2));
                        var g = (byte)((tempG << 3) | (tempG >> 2));
                        var b = (byte)((tempB << 3) | (tempB >> 2));
                        image[x, y] = new Rgb24(r, g, b);
                        dataOffset += 2;
                    }
                }
            }
        }

        public static async Task<bool> DownloadScreenshotAsync(string destfile, CancellationToken cancellationToken)
        {
            return await Task<bool>.Run(() =>
            {
                using (var memoryStream = new MemoryStream())
                using (var downloadStream = new DownloadStream(memoryStream))
                {
                    var response = RXDKXBDM.Commands.Screenshot.SendAsync(Globals.GlobalConnection, cancellationToken, downloadStream).Result;
                    if (!RXDKXBDM.Utils.IsSuccess(response.ResponseCode))
                    {
                        return false;
                    }

                    var screenshot = response.ResponseValue;
                    var bytesPerPixel = screenshot.Pitch / screenshot.Width;
                    using var image = new Image<Rgb24>((int)screenshot.Width, (int)screenshot.Height);
                    AoolyPixelDataToImage(image, screenshot.Pitch, screenshot.Forrmat, screenshot.Data);

                    try
                    {
                        image.Save(destfile);
                    }
                    catch 
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

        public static async Task<string?> ImageFilePicker(Window window, string name)
        {
            var filetypes = new[] { ".bmp", ".gif", ".jpg", ".png", ".tif", ".tga", ".webp" };

            var savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.Downloads,
                SuggestedFileName = name,
                DefaultFileExtension = ".png"
            };
            savePicker.FileTypeChoices.Add("Image File Types", filetypes);

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
