using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Data = SRTLibrary.SRTManager.ProtocolFields.Data;

namespace Client
{
    internal class ImageDisplay
    {
        private static ushort lastDataPosition;
        private static readonly List<byte> allChunks = new List<byte>();

        internal static void ProduceImage(Data.SRTHeader data_request, PictureBox pictureBoxDisplayIn)
        {
            if (data_request.PACKET_POSITION_FLAG == (ushort)Data.PositionFlags.FIRST)
            {
                if (lastDataPosition == (ushort)Data.PositionFlags.MIDDLE)
                {
                    ShowImage(false, pictureBoxDisplayIn);
                    allChunks.Clear();
                }
                allChunks.AddRange(data_request.DATA);
            }

            else if (data_request.PACKET_POSITION_FLAG == (ushort)Data.PositionFlags.LAST)
            {
                allChunks.AddRange(data_request.DATA);
                ShowImage(true, pictureBoxDisplayIn);
                allChunks.Clear();
            }
            else
            {
                allChunks.AddRange(data_request.DATA);
            }

            lastDataPosition = data_request.PACKET_POSITION_FLAG;
        }

        private static void ShowImage(bool allChunksReceived, PictureBox pictureBoxDisplayIn)
        {
#if DEBUG
            if (allChunksReceived)
                Debug.WriteLine("[IMAGE] SUCCESS: Image fully built\n--------------------\n");
            else
                Debug.WriteLine("[IMAGE] ERROR: Chunks missing (SHOWING IMAGE)\n--------------------\n");
#endif

            using (MemoryStream ms = new MemoryStream(allChunks.ToArray()))
            {
                try
                {
                    pictureBoxDisplayIn.Image = new Bitmap(Image.FromStream(ms));
                }
                catch
                {
                    Debug.WriteLine("[IMAGE] ERROR: Can't build image at all");
                }
            }
        }
    }
}
