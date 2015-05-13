using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OxfordUtilities
{
    /// <summary>
    /// Most (if not all) of these are taken/adapted from this great series of tutorials: http://www.codeproject.com/Articles/1989/Image-Processing-for-Dummies-with-C-and-GDI-Part
    /// </summary>
    public static class ImageUtilities
    {
        public enum Channel {  Red, Green, Blue };

        /// <summary>
        /// Invert image pulled from URI
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="outputFormat">Defaults to PNG</param>
        /// <returns></returns>
        public static async Task<Stream> InvertAsync(Uri uri, ImageFormat outputFormat = null)
        {
            return await _ProcessImageFromUri(uri, 
                bitmap => _ProcessBitmap(bitmap, 
                    rgb =>
                    {
                        return Tuple.Create(
                            (byte)(255 - rgb.Item1),
                            (byte)(255 - rgb.Item2),
                            (byte)(255 - rgb.Item3));
                    }
                ), 
                outputFormat).ConfigureAwait(false);
        }

        public static async Task<Stream> GreyscaleAsync(Uri uri, ImageFormat outputFormat = null)
        {
            return await _ProcessImageFromUri(uri,
                bitmap => _ProcessBitmap(bitmap,
                    rgb =>
                    {
                        var grey = (byte)(.299 * rgb.Item1 + .587 * rgb.Item2 + .114 * rgb.Item3);
                        return Tuple.Create(grey, grey, grey);
                    }
                ),
                outputFormat).ConfigureAwait(false);
        }

        public static async Task<Stream> GammaAsync(Uri uri, double gamma, ImageFormat outputFormat = null)
        {
            byte[] redGamma = new byte[256];
            byte[] greenGamma = new byte[256];
            byte[] blueGamma = new byte[256];

            for (int i = 0; i < 256; ++i)
            {
                redGamma[i] = (byte)Math.Min(255, (int)((255.0
                    * Math.Pow(i / 255.0, 1.0 / gamma)) + 0.5));
                greenGamma[i] = (byte)Math.Min(255, (int)((255.0
                    * Math.Pow(i / 255.0, 1.0 / gamma)) + 0.5));
                blueGamma[i] = (byte)Math.Min(255, (int)((255.0
                    * Math.Pow(i / 255.0, 1.0 / gamma)) + 0.5));
            }

            return await _ProcessImageFromUri(uri,
                bitmap => _ProcessBitmap(bitmap,
                    rgb =>
                    {
                        return Tuple.Create(redGamma[rgb.Item1], greenGamma[rgb.Item2], blueGamma[rgb.Item3]);
                    }
                ),
                outputFormat).ConfigureAwait(false);
        }

        public static async Task<Stream> SingleChannelAsync(Uri uri, Channel channel = Channel.Red, bool cloneToOtherChannels = true, ImageFormat outputFormat = null)
        {
            var clone = cloneToOtherChannels;
            return await _ProcessImageFromUri(uri,
                bitmap => _ProcessBitmap(bitmap,
                    rgb =>
                    {
                        byte zero = 0;
                        switch (channel)
                        {
                            case Channel.Red:
                                return Tuple.Create(rgb.Item1, clone ? rgb.Item1 : zero, clone ? rgb.Item1 : zero);
                            case Channel.Green:
                                return Tuple.Create(clone ? rgb.Item2 : zero, rgb.Item2, clone ? rgb.Item2 : zero);
                            case Channel.Blue:
                                return Tuple.Create(clone ? rgb.Item3 : zero, clone ? rgb.Item3 : zero, rgb.Item3);
                            default:
                                throw new ArgumentException("That's not true... that's impossible!");
                        }
                    }
                ),
                outputFormat).ConfigureAwait(false);
        }

        private static async Task<Stream> _ProcessImageFromUri(Uri imageUri, Action<Bitmap> processor, ImageFormat outputFormat = null)
        {
            var response = await WebRequest.Create(imageUri).GetResponseAsync().ConfigureAwait(false);
            Bitmap imageData;
            using (var responseStream = response.GetResponseStream())
            {
                imageData = new Bitmap(responseStream);
            }

            processor(imageData);

            var memoryStream = new MemoryStream();
            imageData.Save(memoryStream, outputFormat ?? ImageFormat.Png);
            memoryStream.Position = 0;
            return memoryStream;
        }

        private static void _ProcessBitmap(Bitmap imageData, Func<Tuple<byte, byte, byte>, Tuple<byte, byte, byte>> processRGB)
        {
            // GDI+ still lies to us - the return format is BGR, NOT RGB. 
            BitmapData bmData = imageData.LockBits(new Rectangle(0, 0, imageData.Width, imageData.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            try
            {
                int stride = bmData.Stride;
                System.IntPtr Scan0 = bmData.Scan0;
                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    byte red, green, blue;
                    int nOffset = stride - imageData.Width * 3;
                    for (int y = 0; y < imageData.Height; ++y)
                    {
                        for (int x = 0; x < imageData.Width; ++x)
                        {
                            blue = p[0];
                            green = p[1];
                            red = p[2];

                            var result = processRGB(Tuple.Create(red, green, blue));
                            p[0] = result.Item3;
                            p[1] = result.Item2;
                            p[2] = result.Item1;

                            p += 3;
                        }
                        p += nOffset;
                    }
                }
            }
            finally
            {
                imageData.UnlockBits(bmData);
            }

        }
    }
}
