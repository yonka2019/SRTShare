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
        private static readonly object _lock = new object();

        internal static void ProduceImage(Data.SRTHeader data_request, Cyotek.Windows.Forms.ImageBox imageBoxDisplayIn)
        {
            // in case if chunk had received while other chunk is building (in this method), the new chunk will create new task and
            // will intervene the proccess, so to avoid multi access tries, lock the global resource (allChunks) until the task will finish
            lock (_lock)
            {
                if (data_request.PACKET_POSITION_FLAG == (ushort)Data.PositionFlags.FIRST)
                {
                    if (lastDataPosition == (ushort)Data.PositionFlags.MIDDLE)  // last lost, image received
                    {
                        ShowImage(false, imageBoxDisplayIn);
                        allChunks.Clear();
                    }
                    allChunks.AddRange(data_request.DATA);
                }

                else if (data_request.PACKET_POSITION_FLAG == (ushort)Data.PositionFlags.LAST)  // full image received (but maybe middle packets get lost)
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
        }

        private static void ShowImage(bool allChunksReceived, Cyotek.Windows.Forms.ImageBox imageBoxDisplayIn)
        {
#if DEBUG
            if (allChunksReceived)
                Debug.WriteLine("[IMAGE] SUCCESS: Image fully built (but maybe middle packets get lost)\n--------------------\n");
            else
                Debug.WriteLine("[IMAGE] ERROR: LAST chunk missing (SHOWING IMAGE)\n--------------------\n");
#endif

            using (MemoryStream ms = new MemoryStream(allChunks.ToArray()))
            {
                try
                {
                    imageBoxDisplayIn.Image = System.Drawing.Image.FromStream(ms);
                }
                catch
                {
                    Debug.WriteLine("[IMAGE] ERROR: Can't build image\n--------------------\n");
                }
            }
        }
    }
}
