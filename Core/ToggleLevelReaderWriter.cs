using System;
using System.Collections;
using System.IO;
using Toggle.Core.Function;
using UnityEngine;
using Toggle.Utils;

namespace Toggle.Core
{
    public static class ToggleLevelBinaryProperties
    {
        public const byte Width = 0x01;
        public const byte Height = 0x02;
        public const byte SubTypes = 0x03;
        public const byte States = 0x04;
        public const byte MinimumClick = 0x05;
        public const byte Creator = 0x06;
    }

    public static class ToggleLevelReader
    {
        public struct Result
        {
            public int width;
            public int height;
            public FunctionSubType[] buttons;
            public BitArray states;
            public int? minimumClickCount;
            public string creator;
        }

        public static Result FromBase64(string base64Str)
        {
            Result result = new Result();

            byte[] compressedBytes = Convert.FromBase64String(base64Str);
            byte[] decompressedBytes = CompressionUtils.Decompress(compressedBytes);

            using (MemoryStream memoryStream = new MemoryStream(decompressedBytes))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    int version = binaryReader.ReadByte();

                    if (version > ToggleLevel.CurrentVersion)
                    {
                        throw new Exception("Version Error.");
                    }

                    ReadProperties(binaryReader, ref result);
                }
            }

            return result;
        }

        private static void ReadProperties(BinaryReader binaryReader, ref Result result)
        {
            Stream baseStream = binaryReader.BaseStream;

            bool didReadWidth = false;

            long length = baseStream.Length;
            int buttonCount = 0;

            while (baseStream.Position < length)
            {
                byte propertyType = binaryReader.ReadByte();
                switch (propertyType)
                {
                    case ToggleLevelBinaryProperties.Width:
                        didReadWidth = true;
                        result.width = binaryReader.ReadByte();
                        break;
                    case ToggleLevelBinaryProperties.Height:
                        result.height = binaryReader.ReadByte();
                        if (didReadWidth)
                        {
                            result.states = new BitArray(result.width * result.height);
                            buttonCount = result.width * result.height;
                        }

                        break;
                    case ToggleLevelBinaryProperties.SubTypes:
                        if (buttonCount == 0)
                            throw new Exception("Either Width or Height should not be Zero!");

                        result.buttons = new FunctionSubType[buttonCount];
                        for (int i = 0; i < buttonCount; i++)
                        {
                            int intType = binaryReader.ReadByte();
                            result.buttons[i] = ConvertMap.IntToType(intType);
                        }
                        break;
                    case ToggleLevelBinaryProperties.States:
                        if (buttonCount == 0)
                            throw new Exception("Either Width or Height should not be Zero!");

                        int stateByteCount = (buttonCount - 1) / 8 + 1;
                        byte[] stateBytes = binaryReader.ReadBytes(stateByteCount);
                        result.states = new BitArray(stateBytes);
                        break;

                    case ToggleLevelBinaryProperties.MinimumClick:

                        int value = binaryReader.ReadInt32();
                        if (value > 0)
                        {
                            result.minimumClickCount = value;
                        }
                        break;
                    case ToggleLevelBinaryProperties.Creator:
                        result.creator = binaryReader.ReadString();
                        break;
                }
            }
        }


    }


    public class ToggleLevelWriter : IDisposable
    {
        private MemoryStream memoryStream;
        private BinaryWriter binaryWriter;

        public ToggleLevelWriter()
        {
            memoryStream = new MemoryStream();
            binaryWriter = new BinaryWriter(memoryStream);
        }

        public void Write(ToggleLevel level)
        {
            binaryWriter.Write((byte)ToggleLevel.CurrentVersion);

            binaryWriter.Write(ToggleLevelBinaryProperties.Width);
            binaryWriter.Write((byte)level.width);

            binaryWriter.Write(ToggleLevelBinaryProperties.Height);
            binaryWriter.Write((byte)level.height);

            binaryWriter.Write(ToggleLevelBinaryProperties.SubTypes);
            for (int i = 0; i < level.buttons.Length; i++)
            {
                int intType = ConvertMap.TypeToInt(level.buttons[i]);
                binaryWriter.Write((byte)intType);
            }

            binaryWriter.Write(ToggleLevelBinaryProperties.States);
            byte[] stateBytes = level.states.BitArrayToByteArray();
            binaryWriter.Write(stateBytes);
        }

        public void Write(byte propertyDefine, byte value)
        {
            binaryWriter.Write(propertyDefine);
            binaryWriter.Write(value);
        }

        public void Write(byte propertyDefine, int value)
        {
            binaryWriter.Write(propertyDefine);
            binaryWriter.Write(value);
        }

        public void Write(byte propertyDefine, string value)
        {
            binaryWriter.Write(propertyDefine);
            binaryWriter.Write(value);
        }

        public string ToBase64()
        {
            byte[] compressedBytes = CompressionUtils.Compress(memoryStream.ToArray());
            return Convert.ToBase64String(compressedBytes);
        }

        public void Dispose()
        {
            memoryStream?.Dispose();
            binaryWriter?.Dispose();
        }
    }
}