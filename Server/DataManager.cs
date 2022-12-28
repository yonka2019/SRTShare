using PcapDotNet.Packets;
using SRTLibrary;
using SRTLibrary.SRTManager.RequestsFactory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using SRTRequest = SRTLibrary.SRTManager.RequestsFactory;

namespace Server
{

    internal class DataManager
    {
        private static class Win32Native
        {
            public const int DESKTOPVERTRES = 0x75;
            public const int DESKTOPHORZRES = 0x76;

            [DllImport("gdi32.dll")]
            public static extern int GetDeviceCaps(IntPtr hDC, int index);
        }

        private readonly SClient client;
        private bool connected;

        internal DataManager(SClient client)
        {
            this.client = client;
            connected = true;
        }

        /// <summary>
        /// The function starts the thread responsible for screenshare sending
        /// </summary>
        internal void StartVideo()
        {
            Thread videoStarter = new Thread(new ParameterizedThreadStart(VideoInit));  // create thread of keep-alive checker

            videoStarter.Start(client.SocketId);
        }
        
        /// <summary>
        /// Stops the video via the condition variable
        /// </summary>
        internal void StopVideo()
        {
            connected = false;
        }

        private void VideoInit(object dest_socket_id)
        {
            uint u_dest_socket_id = (uint)dest_socket_id;

            while (connected)
            {
                Bitmap bmp = TakeScreenShot();
                MemoryStream mStream = GetJpegStream(bmp);
                List<byte> stream = mStream.ToArray().ToList();

                DataRequest dataRequest = new DataRequest(
                                (PacketManager.BuildBaseLayers(PacketManager.macAddress, client.MacAddress.ToString(), PacketManager.localIp, client.IPAddress.ToString(), PacketManager.SERVER_PORT, client.Port)));

                List<Packet> data_packets = dataRequest.SplitToPackets(stream, time_stamp: 0, u_dest_socket_id, (int)client.MTU);

                foreach (Packet packet in data_packets)
                    PacketManager.SendPacket(packet);
            }
        }

        private static Bitmap TakeScreenShot()
        {
            int width, height;

            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero))
            {
                IntPtr hDC = g.GetHdc();
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

        private static MemoryStream GetJpegStream(Bitmap bmp)
        {
            MemoryStream stream = new MemoryStream();
            Encoder myEncoder = Encoder.Quality;

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
            myEncoderParameters.Param[0] = myEncoderParameter;

            bmp.Save(stream, jpgEncoder, myEncoderParameters);

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
