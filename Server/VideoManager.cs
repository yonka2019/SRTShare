using PcapDotNet.Packets;
using SRTShareLib;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
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
        private readonly SClient client;
        private bool connected;
        private uint retransmitRequested;

        internal Dictionary<uint, List<byte>> ImagesBuffer = new Dictionary<uint, List<byte>>();
        public readonly BaseEncryption ClientEncryption;
        public bool VideoStage { get; private set; }

#if DEBUG
        private static ulong dataSent = 0;  // count data sent packets (included chunks)
#endif

        internal long CurrentQuality { private get; set; }

        private static uint current_sequence_number;

        internal VideoManager(SClient client, BaseEncryption baseEncryption, uint intial_sequence_number)
        {
            current_sequence_number = intial_sequence_number;  // start from init seq number (MUST BE NOT 0)
            retransmitRequested = 0;

            this.client = client;
            connected = true;

            ClientEncryption = baseEncryption;
            CurrentQuality = ProtocolManager.DEFAULT_QUALITY;  // default quality value
        }

        /// <summary>
        /// The function starts the thread responsible for screenshare sending
        /// </summary>
        internal void StartVideo()
        {
            Thread videoStarter = new Thread(new ThreadStart(VideoInit));  // create thread of keep-alive checker
            videoStarter.Start();
            VideoStage = true;

            CConsole.WriteLine($"[Server] [{client.IPAddress}] Video is being shared\n", MessageType.txtInfo);
        }

        /// <summary>
        /// Stops the video via the condition variable
        /// </summary>
        internal void StopVideo()
        {
            connected = false;
            VideoStage = false;
        }

        /// <summary>
        /// if NAK packet received which means that the server should retransmit the corrupted image
        /// this function retransmittes the requested image by his sequence number
        /// </summary>
        /// <param name="sequenceNumberToRetransmit">image (sequence number) which should be retransmitted</param>
        internal void ResendImage(uint sequenceNumberToRetransmit)
        {
            retransmitRequested = sequenceNumberToRetransmit;
        }

        /// <summary>
        /// function which is confirms that the client received the whole image successfully and he can be cleaned from server's buffer
        /// </summary>
        /// <param name="packetSequenceNumber">image (sequence number) which should be confirm</param>
        internal void ConfirmImage(uint packetSequenceNumber)
        {
            if (ImagesBuffer.ContainsKey(packetSequenceNumber))  // maybe image already confirmed
            {
                Console.WriteLine("cleared: " + packetSequenceNumber);
                ImagesBuffer[packetSequenceNumber].Clear();
                ImagesBuffer.Remove(packetSequenceNumber);
            }
        }

        private void VideoInit()
        {
            while (connected)
            {
                if (retransmitRequested != 0)  // if retransmit requested, retransmit - and continue
                {
                    SplitAndSend(ImagesBuffer[retransmitRequested]);
                    retransmitRequested = 0;  // reset
                }

                Bitmap bmp = TakeScreenShot();
                MemoryStream mStream = GetJpegStream(bmp);
                List<byte> stream = mStream.ToArray().ToList();
                ImagesBuffer[current_sequence_number] = stream;  // save image to buffer
                Console.WriteLine("saved: " + current_sequence_number);

                SplitAndSend(stream);
                current_sequence_number++;
            }
        }

        private void SplitAndSend(List<byte> image)
        {
            DataRequest dataRequest = new DataRequest(
                               OSIManager.BuildBaseLayers(NetworkManager.MacAddress, client.MacAddress.ToString(), NetworkManager.LocalIp, client.IPAddress.ToString(), ConfigManager.PORT, client.Port));

            // (.MTU - 100; explanation) : To avoid errors with sending, because this field used to set fixed size of splitted data packet,
            // while the real mtu that the interface provides refers the whole size of the packet which get sent,
            // and with the whole srt packet and all layers in will much more
            List<Packet> data_packets = dataRequest.SplitToPackets(image, ref current_sequence_number, time_stamp: 0, client.SocketId, (int)client.MTU - 100, ClientEncryption);

            foreach (Packet packet in data_packets)
            {
                PacketManager.SendPacket(packet);
#if DEBUG
                Console.Title = $"Data sent {++dataSent}";
#endif
            }
        }

        // 'app.manifest' file modified for auto-scale screenshot - https://stackoverflow.com/questions/47015893/windows-screenshot-with-scaling
        /// <summary>
        /// Screenshots current selected screen (supporting scale (125%, 150%..)
        /// </summary>
        /// <returns>Bitmap of the screenshot</returns>
        private Bitmap TakeScreenShot()
        {
            int width, height;

            // get the selected screen
            Screen selectedScreen = Screen.AllScreens[Program.SharedScreenIndex];

            int x = selectedScreen.Bounds.X;
            int y = selectedScreen.Bounds.Y;

            width = selectedScreen.Bounds.Width;
            height = selectedScreen.Bounds.Height;

            Bitmap bmp = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(x, y, 0, 0, bmp.Size);
                return bmp;
            }
        }

        /// <summary>
        /// Converts bitmap into jpg to avoid load
        /// </summary>
        /// <param name="bmp">bitmap image to convert</param>
        /// <returns>memory stream which contains the jpg image</returns>
        private MemoryStream GetJpegStream(Bitmap bmp)
        {
            MemoryStream stream = new MemoryStream();
            Encoder myEncoder = Encoder.Quality;

            ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            EncoderParameters myEncoderParameters = new EncoderParameters(1);

            EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, CurrentQuality);  // set qualiy 0 -> 100
            myEncoderParameters.Param[0] = myEncoderParameter;

            bmp.Save(stream, jpgEncoder, myEncoderParameters);

            return stream;
        }

        /// <summary>
        /// gets the image encoder according the image format
        /// </summary>
        /// <param name="format">image format to get his encoder</param>
        /// <returns>codec of the given image format</returns>
        private ImageCodecInfo GetEncoder(ImageFormat format)
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
