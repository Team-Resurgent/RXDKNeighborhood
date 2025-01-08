﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RXDKXBDM
{
    public static class Utils
    {
        public static IDictionary<string, string> MultilineResponseToDictionary(string response)
        {
            var result = new Dictionary<string, string>();
            var lines = response.Split("\r\n");
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line == ".")
                {
                    break;
                }
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
       
        public static IDictionary<string, string>[] MultilineResponseToDictionaryArray(string response)
        {
            var result = new List<Dictionary<string, string>>();
            var lines = response.Split("\r\n");
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line == ".")
                {
                    break;
                }
                result.Add(StringToDictionary(line));
            }
            return result.ToArray();
        }

        public static Dictionary<string, string> StringToDictionary(string line)
        {
            var parts = line.Split(" ").ToArray();
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

        public static ulong GetDictionaryLongFromKeys(IDictionary<string, string> keyValues, string hiKey, string loKey)
        {
            var hiValue = GetDictionaryString(keyValues, hiKey);
            var loValue = GetDictionaryString(keyValues, loKey);
            var hi = Convert.ToUInt32(hiValue, 16);
            var lo = Convert.ToUInt32(loValue, 16);
            var result = ((ulong)hi << 32) | (ulong)lo;
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


    }
}
