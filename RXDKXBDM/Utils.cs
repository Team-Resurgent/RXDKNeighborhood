using RXDKXBDM.Commands;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace RXDKXBDM
{
    public static class Utils
    {
        public static string[] SplitBySpaceIgnoringQuotes(string input)
        {
            var result = new List<string>();
            var currentPart = string.Empty;
            bool insideQuotes = false;

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];
                if (c == '"') 
                {
                    insideQuotes = !insideQuotes;
                    continue; 
                }
                if (c == ' ' && !insideQuotes) 
                {
                    if (!string.IsNullOrEmpty(currentPart))
                    {
                        result.Add(currentPart);
                        currentPart = string.Empty; 
                    }
                }
                else
                {
                    currentPart += c; 
                }
            }

            if (!string.IsNullOrEmpty(currentPart))
            {
                result.Add(currentPart);
            }

            return result.ToArray();
        }

        public static bool IsSuccess(ResponseCode responseCode)
        {
            return (int)responseCode >= 200 && (int)responseCode <= 299;
        }

        public static IDictionary<string, string> BodyToDictionary(string[] body)
        {
            var result = new Dictionary<string, string>();
            for (int i = 0; i < body.Length; i++)
            {
                var line = body[i];
                var temp = StringToDictionary(line);
                var keys = temp.Keys.ToArray();
                for (var j = 0; j < keys.Length; j++)
                {
                    var key = keys[j];
                    result.Add(key, temp[key]);
                }
            }
            return result;
        }
       
        public static IDictionary<string, string>[] BodyToDictionaryArray(string[] body)
        {
            var result = new List<Dictionary<string, string>>();
            for (int i = 0; i < body.Length; i++)
            {
                var line = body[i];
                result.Add(StringToDictionary(line));
            }
            return result.ToArray();
        }

        public static Dictionary<string, string> StringToDictionary(string line)
        {
            var parts = SplitBySpaceIgnoringQuotes(line);
            var result = new Dictionary<string, string>();
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i] ?? string.Empty;
                var keyValues = part.Split('=');
                var key = keyValues[0];
                if (keyValues.Length == 2)
                {
                    var value = keyValues[1];
                    if (value.StartsWith("\""))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }
                    result[key] = value;
                }
                else
                {
                    result[key] = string.Empty;
                }
                
            }
            return result;
        }

        public static string GetDictionaryString(IDictionary<string, string> keyValues, string key)
        {
            var result = keyValues.TryGetValue(key, out string? value) ? value : "";
            return result;
        }

        public static uint GetDictionaryIntFromKey(IDictionary<string, string> keyValues, string key)
        {
            var value = GetDictionaryString(keyValues, key);
            if (value.EndsWith(","))
            {
                value = value.Substring(0, value.Length - 1);
            }
            if (string.IsNullOrEmpty(value) || uint.TryParse(value.Substring(2), System.Globalization.NumberStyles.HexNumber, null, out var result) == false)
            {
                return 0;
            }
            return result;
        }

        public static ulong GetDictionaryLongFromKeys(IDictionary<string, string> keyValues, string hiKey, string loKey)
        {
            var hiValue = GetDictionaryIntFromKey(keyValues, hiKey);
            var loValue = GetDictionaryIntFromKey(keyValues, loKey);
            var result = ((ulong)hiValue << 32) | loValue;
            return result;
        }

        public static IDictionary<string, string> DateTimeToDictionary(DateTime dateTime)
        {
            DateTime fileTimeStart = new DateTime(1601, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var fileTimeTicks = (ulong)(dateTime.ToUniversalTime() - fileTimeStart).Ticks;
            uint hiValue = (uint)(fileTimeTicks >> 32);
            uint loValue = (uint)(fileTimeTicks & 0xFFFFFFFF);
            var result = new Dictionary<string, string>
            {
                { "hi", "0x" + hiValue.ToString("x8") },
                { "lo", "0x" + loValue.ToString("x8") }
            };
            return result;
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

        public static async Task<bool> DownloadScreenshotAsync(Connection connection, string destfile, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                using (var memoryStream = new MemoryStream())
                using (var downloadStream = new DownloadStream(memoryStream))
                {
                    var response = Screenshot.SendAsync(connection, cancellationToken, downloadStream).Result;
                    if (!IsSuccess(response.ResponseCode))
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
    }
}
