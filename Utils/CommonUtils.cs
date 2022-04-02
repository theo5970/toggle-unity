using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Toggle.Utils
{
    public static class CommonUtils
    {
        private static StringBuilder tempBuilder = new StringBuilder();

        public static StringBuilder GetStringBuilder()
        {
            tempBuilder.Clear();
            return tempBuilder;
        }

        public static string FormatTime(int totalSeconds)
        {
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds / 60) % 60;
            int seconds = totalSeconds % 60;

            tempBuilder.Clear();

            if (hours > 0)
            {
                if (hours < 10)
                {
                    tempBuilder.Append('0');
                }

                tempBuilder.Append(hours);
                tempBuilder.Append(':');
            }

            if (minutes < 10)
            {
                tempBuilder.Append('0');
            }

            tempBuilder.Append(minutes);
            tempBuilder.Append(':');

            if (seconds < 10)
            {
                tempBuilder.Append('0');
            }

            tempBuilder.Append(seconds);

            return tempBuilder.ToString();
        }

        public static double GetCurrentTimeStamp()
        {
            DateTime date = DateTime.Now;
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        public static DateTime TimeStampToDateTime(double timeStamp)
        {
            DateTime time = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            time = time.AddSeconds(timeStamp).ToLocalTime();

            return time;
        }
        public static int MonthDifference(this DateTime lValue, DateTime rValue)
        {
            return (lValue.Month - rValue.Month) + 12 * (lValue.Year - rValue.Year);
        }

        public static string CompressOrdersToBase64(List<Vector2Int> orders)
        {
            byte[] binaryBytes;
            using (MemoryStream binaryMemoryStream = new MemoryStream())
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(binaryMemoryStream))
                {
                    binaryWriter.Write(orders.Count);
                    for (int i = 0; i < orders.Count; i++)
                    {
                        binaryWriter.Write(orders[i].x);
                        binaryWriter.Write(orders[i].y);
                    }
                }

                binaryBytes = binaryMemoryStream.ToArray();
            }

            byte[] compressedBytes;
            using (MemoryStream compressMemoryStream = new MemoryStream())
            {
                using (DeflateStream deflateStream = new DeflateStream(compressMemoryStream, CompressionMode.Compress))
                {
                    deflateStream.Write(binaryBytes, 0, binaryBytes.Length);
                }

                compressedBytes = compressMemoryStream.ToArray();
            }

            return Convert.ToBase64String(compressedBytes);
        }


    }
}