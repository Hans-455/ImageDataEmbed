using System;
using System.Collections;
using System.Drawing;

namespace ImageDataEmbed.Code.Image
{
    public class PngDataEmbed
    {
        private Bitmap _bitmap;
        private Bitmap _writeImage;
        public int BitStorage { get { return (_bitmap.Width * _bitmap.Height * 4) - 32; } }
        public PngDataEmbed(string file)
        {
            _bitmap = new Bitmap(file);
            _writeImage = new Bitmap(_bitmap.Width, _bitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            for(int x = 0; x < _bitmap.Width; x++)
            {
                for (int y = 0; y < _bitmap.Height; y++)
                {
                    _writeImage.SetPixel(x, y, _bitmap.GetPixel(x, y));
                }
            }
        }

        public void EmbedData(byte[] data)
        {
            BitArray dataBits = new BitArray(data);

            int writeBits = dataBits.Length;

            if (writeBits > BitStorage)
            {
                throw new Exception("Not enough space in image");
            }

            int x = 0;
            int y = 0;
            int c = 0;

            BitArray sizeBits = new BitArray(BitConverter.GetBytes(writeBits));
            WriteDataToImage(ref x, ref y, ref c, sizeBits);
            WriteDataToImage(ref x, ref y, ref c, dataBits);
        }

        public byte[] ExtractData()
        {
            BitArray sizeBits = new BitArray(32);
            int x = 0;
            int y = 0;
            int c = 0;

            ExtractBits(sizeBits, ref x, ref y, ref c, 32);

            int[] array = new int[1];
            sizeBits.CopyTo(array, 0);
            int size = array[0];

            BitArray dataBits = new BitArray(size);
            ExtractBits(dataBits, ref x, ref y, ref c, size);

            byte[] result = new byte[size / 8];
            dataBits.CopyTo(result, 0);

            return result;
        }

        private void ExtractBits(BitArray sizeBits, ref int x, ref int y, ref int c, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                byte currentByte = GetChannelValue(c, _bitmap.GetPixel(x, y));

                if (currentByte % 2 == 1)
                {
                    sizeBits.Set(i, true);
                }
                else
                {
                    sizeBits.Set(i, false);
                }

                x++;
                if (x >= _bitmap.Width)
                {
                    x = 0;
                    y++;
                }
                if (y >= _bitmap.Height)
                {
                    y = 0;
                    c++;
                }
            }
        }

        public void SaveImage(string file)
        {
            _writeImage.Save(file);
        }

        private void WriteDataToImage(ref int x, ref int y, ref int c, BitArray sizeBits)
        {
            for (int i = 0; i < sizeBits.Count; ++i)
            {
                Color current = _bitmap.GetPixel(x, y);
                byte currentValue = GetChannelValue(c, current);
                byte replaceByte = currentValue;

                if (sizeBits[i])
                {
                    if (currentValue % 2 == 0)
                    {
                        replaceByte = TweakByte(currentValue);
                    }
                }
                else
                {
                    if (currentValue % 2 == 1)
                    {
                        replaceByte = TweakByte(currentValue);
                    }
                }

                Color newColor = GetNewColorValue(c, replaceByte, current);
                _writeImage.SetPixel(x, y, newColor);

                x++;
                if (x >= _bitmap.Width)
                {
                    x = 0;
                    y++;
                }
                if (y >= _bitmap.Height)
                {
                    y = 0;
                    c++;
                }
            }
        }

        private byte TweakByte(byte b)
        {
            byte newB = b;
            if(newB > 0)
            {
                newB--;
            }else
            {
                newB++;
            }
            return newB;
        }
        private byte GetChannelValue(int channel, Color color)
        {
            switch(channel)
            {
                case 0: return color.R;
                case 1: return color.G;
                case 2: return color.B;
                case 3: return color.A;
                default:throw new IndexOutOfRangeException();
            }
        }
        private Color GetNewColorValue(int channel, byte channelVal, Color existingColor)
        {
            switch(channel)
            {
                case 0: return Color.FromArgb(existingColor.A, channelVal, existingColor.G, existingColor.B);
                case 1: return Color.FromArgb(existingColor.A, existingColor.R, channelVal, existingColor.B);
                case 2: return Color.FromArgb(existingColor.A, existingColor.R, existingColor.G, channelVal);
                case 3: return Color.FromArgb(channelVal, existingColor.R, existingColor.G, existingColor.B);
                default: throw new IndexOutOfRangeException();
            }
        }
    }
}
