using RXDKXBDM.Commands;
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

        public static async Task<bool> DownloadFolderAsync(string sourcefolder, string destfolder, Action<long, long> progress)
        {
            return await Task.Run(() =>
            {
                return true;
            });
        }

        public static async Task<bool> DownloadFileAsync(string sourcefile, string destfile, Action<long, long> progress)
        {
            return await Task.Run(() =>
            {
                using (var fileStream = new FileStream(destfile, FileMode.Create))
                using (var downloadStream = new DownloadStream(fileStream, progress))
                {
                    var response = Download.SendAsync(Globals.GlobalConnection, sourcefile, downloadStream).Result;
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
