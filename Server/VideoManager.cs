using PcapDotNet.Packets;
using SRTShareLib;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.RequestsFactory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CConsole = SRTShareLib.CColorManager;  // Colored Console

namespace Server
{
    internal class VideoManager
    {
        private static int screenIndex;
        private static Screen[] screens;

        private readonly SClient client;
        private bool connected;

#if DEBUG
        private static ulong dataSent = 0;  // count data sent packets (included chunks)
#endif

        internal VideoManager(SClient client)
        {
            screenIndex = 0;
            screens = Screen.AllScreens;

            this.client = client;
            connected = true;
        }

        /// <summary>
        /// The function starts the thread responsible for screenshare sending
        /// </summary>
        internal void StartVideo()
        {
            Thread keyListenerThread = new Thread(KeysListener);
            Thread videoStarter = new Thread(new ParameterizedThreadStart(VideoInit));  // create thread of keep-alive checker

            keyListenerThread.Start();
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
            int count = 0;

            while (connected)
            {
                Bitmap bmp = TakeScreenShot();
                MemoryStream mStream = GetJpegStream(bmp);
                List<byte> stream = mStream.ToArray().ToList();

                DataRequest dataRequest = new DataRequest(
                                OSIManager.BuildBaseLayers(NetworkManager.MacAddress, client.MacAddress.ToString(), NetworkManager.LocalIp, client.IPAddress.ToString(), ConfigManager.PORT, client.Port));

                List<Packet> data_packets = dataRequest.SplitToPackets(stream, time_stamp: 0, u_dest_socket_id, (int)client.MTU);

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

            // get the selected screen
            Screen selectedScreen = Screen.AllScreens[screenIndex];

            int x = selectedScreen.Bounds.X;
            int y = selectedScreen.Bounds.Y;

            width = selectedScreen.Bounds.Size.Width;
            height = selectedScreen.Bounds.Size.Height;

            Bitmap bmp = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(x, y, 0, 0, bmp.Size);
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

        private static void KeysListener()
        {
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey();
                if (keyInfo.Key == ConsoleKey.LeftArrow)
                {
                    if (screenIndex > 0)
                    {
                        screenIndex--;
                        CConsole.WriteLine($"[Server] Screen {screenIndex + 1} is shared", MessageType.txtInfo);
                    }
                    else
                    {
                        CConsole.WriteLine($"[Server] You can only move between ({1} - {screens.Length}) screens", MessageType.txtWarning);
                    }
                }
                else if (keyInfo.Key == ConsoleKey.RightArrow)
                {
                    if (screenIndex + 1 < screens.Length)
                    {
                        screenIndex++;
                        CConsole.WriteLine($"[Server] Screen {screenIndex + 1} is shared.", MessageType.txtInfo);
                    }
                    else
                    {
                        CConsole.WriteLine($"[Server] You can only move between ({1} - {screens.Length}) screens", MessageType.txtWarning);
                    }
                }
            }
        }
    }
}
