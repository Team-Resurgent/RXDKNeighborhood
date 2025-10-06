using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;

namespace RXDKNeighborhood.Extensions
{
    public static class BitmapExtension
    {
        public static Bitmap ToBitmap(this Uri assetUri)
        {
            var rootNamespace = typeof(App).Namespace;
            var finalUri = assetUri.IsAbsoluteUri ? assetUri : new Uri($"avares://{rootNamespace}/Assets/{assetUri.OriginalString.TrimStart('/')}");
            using var stream = AssetLoader.Open(finalUri);
            return new Bitmap(stream);
        }
    }
}
