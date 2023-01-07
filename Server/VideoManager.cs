using PcapDotNet.Packets;
using PcapDotNet.Packets.Ip;
using SRTLibrary;
using SRTLibrary.SRTManager.RequestsFactory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace Server
{
    internal class VideoManager
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

#if DEBUG
        private static ulong dataSent = 0;
#endif

        internal VideoManager(SClient client)
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
            //bool hi = false;
            //bool hello = false;

            int count = 0;
            while (connected)
            {
                Bitmap bmp = TakeScreenShot();
                MemoryStream mStream = GetJpegStream(bmp);
                List<byte> stream = mStream.ToArray().ToList();

                //if (count == 10)
                //{
                //    using (MemoryStream ms = new MemoryStream(stream.ToArray()))
                //    {
                //        // Read the bytes from the memory stream
                //        byte[] bytes = ms.ToArray();

                //        // Save the image to a file
                //        File.WriteAllBytes("imageStream.png", bytes);
                //    }
                //    //

                //    hi = true;
                //}

                DataRequest dataRequest = new DataRequest(
                                PacketManager.BuildBaseLayers(PacketManager.MacAddress, client.MacAddress.ToString(), PacketManager.LocalIp, client.IPAddress.ToString(), ConfigManager.PORT, client.Port));


                List<Packet> data_packets = dataRequest.SplitToPackets(stream, time_stamp: 0, u_dest_socket_id, (int)client.MTU);

                //if(count == 10)
                //{
                //    List<byte> bData = new List<byte>();

                //    foreach (Packet p in data_packets)
                //    {
                //        SRTLibrary.SRTManager.ProtocolFields.Data.SRTHeader data_request = new SRTLibrary.SRTManager.ProtocolFields.Data.SRTHeader(p.Ethernet.IpV4.Udp.Payload.ToArray());

                //        bData.AddRange(data_request.DATA);
                //    }

                //    using (MemoryStream ms = new MemoryStream(bData.ToArray()))
                //    {
                //        // Read the bytes from the memory stream
                //        byte[] bytes = ms.ToArray();

                //        // Save the image to a file
                //        File.WriteAllBytes("image.png", bytes);
                //    }
                //    //

                //    //

                //    hello = true;
                //}


                foreach (Packet packet in data_packets)
                {
                    PacketManager.SendPacket(packet);
#if DEBUG
                    Console.Title = $"Data sent {++dataSent}";
#endif
                }

                count++;
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
