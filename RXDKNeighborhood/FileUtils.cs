using RXDKNeighborhood.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;

namespace RXDKNeighborhood
{
    public static class FileUtils
    {
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
