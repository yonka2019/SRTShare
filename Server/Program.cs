using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace ServerTVieverConsole
{
    static class Win32Native
    {
        public const int DESKTOPVERTRES = 0x75;
        public const int DESKTOPHORZRES = 0x76;

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hDC, int index);
    }
    class Program
    {
        static Bitmap TakeScreenShot()
        {
            int width, height;
            using (var g = Graphics.FromHwnd(IntPtr.Zero))
            {
                var hDC = g.GetHdc();
                width = Win32Native.GetDeviceCaps(hDC, Win32Native.DESKTOPHORZRES);
                height = Win32Native.GetDeviceCaps(hDC, Win32Native.DESKTOPVERTRES);
                g.ReleaseHdc(hDC);
            }

            Bitmap bmp = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
                return bmp;
            }
        }

        static void Main(string[] args)
        {
            var server = new TcpListener(
          IPAddress.Any,
          40001
         );

            server.Start();
            while (true)
            {
                var client = server.AcceptTcpClient();

                Console.WriteLine("[INFO] New client connected to the stream");
                Task.Run(() =>
                {
                    while (true)
                    {
                        try
                        {
                            var stream = client.GetStream();
                            var bmp = TakeScreenShot();
                            var formatter = new BinaryFormatter();

                            Stream picture_stream = GetJpegStream(bmp);
                            formatter.Serialize(stream, picture_stream);
                            picture_stream.Close();
                        }

                        catch (Exception ex)
                        {
                            Console.WriteLine("[INFO] A client left the stream");
                            client.Close();
                            break;
                        }



                    }
                });
            }
        }

        public static Stream GetJpegStream(Bitmap bmp)
        {
            Stream stream = new MemoryStream();

            System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            bmp.Save(stream, jpgEncoder, myEncoderParameters);

            //bmp.Save(stream, ImageFormat.Jpeg);

            return stream;
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }

}
