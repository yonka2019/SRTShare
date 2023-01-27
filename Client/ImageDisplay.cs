using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Data = SRTShareLib.SRTManager.ProtocolFields.Data;

namespace Client
{
    internal class ImageDisplay
    {
        private static ushort lastDataPosition;
        private static readonly List<byte> allChunks = new List<byte>();

        // check if need to be async

        internal static void ProduceImage(Data.SRTHeader data_request, Cyotek.Windows.Forms.ImageBox imageBoxDisplayIn)
        {
            if (data_request.PACKET_POSITION_FLAG == (ushort)Data.PositionFlags.FIRST)
            {
                if (lastDataPosition == (ushort)Data.PositionFlags.MIDDLE)
                {
                    ShowImage(false, imageBoxDisplayIn);
                    allChunks.Clear();
                }
                allChunks.AddRange(data_request.DATA);
            }

            else if (data_request.PACKET_POSITION_FLAG == (ushort)Data.PositionFlags.LAST)
            {
                allChunks.AddRange(data_request.DATA);
                ShowImage(true, imageBoxDisplayIn);
                allChunks.Clear();
            }
            else
            {
                allChunks.AddRange(data_request.DATA);
            }

            lastDataPosition = data_request.PACKET_POSITION_FLAG;
        }

        private static void ShowImage(bool allChunksReceived, Cyotek.Windows.Forms.ImageBox imageBoxDisplayIn)
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
                    imageBoxDisplayIn.Image = System.Drawing.Image.FromStream(ms);
                }
                catch
                {
                    Debug.WriteLine("[IMAGE] ERROR: Can't build image at all\n--------------------\n");
                }
            }
        }
    }
}
