using PcapDotNet.Packets;
using SRTShareLib;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.RequestsFactory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CConsole = SRTShareLib.CColorManager;  // Colored Console
using Data = SRTShareLib.SRTManager.ProtocolFields.Data;

namespace Client
{
    internal class ImageDisplay
    {
        private static ushort lastDataPosition;
        private static readonly object _lock = new object();

        private static List<Data.SRTHeader> dataPackets = new List<Data.SRTHeader>();
        internal static byte CurrentVideoQuality = 50;

        private static DateTime lastQualityModify;
        private const int minimum_SecondsElapsedToModify = 3;  // don't allow the algorithm to AUTO modify the quality if there is was a quality change

        private static byte[] FullData
        {
            get
            {
                List<byte> fullData = new List<byte>();

                foreach (Data.SRTHeader srtHeader in dataPackets)  // add each data into [List<byte> fullData]
                {
                    fullData.AddRange(srtHeader.DATA);
                }
                // convert to byte array and return
                return fullData.ToArray();
            }
        }

        internal static void ProduceImage(Data.SRTHeader data_request, Cyotek.Windows.Forms.ImageBox imageBoxDisplayIn)
        {
            // in case if chunk had received while other chunk is building (in this method), the new chunk will create new task and
            // will intervene the proccess, so to avoid multi access tries, lock the global resource (allChunks) until the task will finish
            lock (_lock)
            {
                if (data_request.PACKET_POSITION_FLAG == (ushort)Data.PositionFlags.FIRST)
                {
                    if (lastDataPosition == (ushort)Data.PositionFlags.MIDDLE)  // LAST lost, image received [FIRST MID MID MID ---- FIRST]
                    {
                        ShowImage(false, imageBoxDisplayIn);
                        dataPackets.Clear();
                    }
                    dataPackets.Add(data_request);
                }

                else if (data_request.PACKET_POSITION_FLAG == (ushort)Data.PositionFlags.LAST)  // full image received (but maybe middle packets get lost)
                {
                    dataPackets.Add(data_request);
                    ShowImage(true, imageBoxDisplayIn);
                    dataPackets.Clear();
                }
                else
                {
                    dataPackets.Add(data_request);
                }

                lastDataPosition = data_request.PACKET_POSITION_FLAG;
            }
        }

        private static void ShowImage(bool lastChunkReceived, Cyotek.Windows.Forms.ImageBox imageBoxDisplayIn)
        {
#if DEBUG
            if (!lastChunkReceived)
                Debug.WriteLine("[IMAGE] ERROR: LAST chunk missing (SHOWING IMAGE)\n");
#endif
            uint[] missedPackets = MissingPackets();

            Console.WriteLine("SHOULD BE: " + (Math.Ceiling(dataPackets.Last().MESSAGE_NUMBER * (MainView.DATA_LOSS_PERCENT_REQUIRED / 100.0))));
            Console.WriteLine("MISSED: " + (missedPackets.Length));

            TimeSpan timeElapsed = DateTime.Now - lastQualityModify;

            // dataPackets.Last().MESSAGE_NUMBER - the last seq number which is the max
            if ((Math.Ceiling(dataPackets.Last().MESSAGE_NUMBER * (MainView.DATA_LOSS_PERCENT_REQUIRED / 100.0)) <= missedPackets.Length)  // check if necessary

                && MainView.AutoQualityControl  // check if option enabled

                && timeElapsed.TotalSeconds > minimum_SecondsElapsedToModify)  // check if min required time elapsed
            {
                if (CurrentVideoQuality - MainView.DATA_DECREASE_QUALITY_BY > 0)
                {
                    CurrentVideoQuality -= MainView.DATA_DECREASE_QUALITY_BY;  // down quality and send quality update request 

                    QualityUpdateRequest qualityUpdate_request = new QualityUpdateRequest(OSIManager.BuildBaseLayers(NetworkManager.MacAddress, MainView.serverMac, NetworkManager.LocalIp, ConfigManager.IP, MainView.myPort, ConfigManager.PORT));
                    Packet qualityUpdate_packet = qualityUpdate_request.UpdateQuality(MainView.server_sid, CurrentVideoQuality);
                    PacketManager.SendPacket(qualityUpdate_packet);

                    CConsole.WriteLine($"[Auto Quality Control] Quality updated: {CurrentVideoQuality}\n" , MessageType.txtWarning);

                    ToolStripMenuItem qualityButton = MainView.QualityButtons[RoundToNearestTen(CurrentVideoQuality)];

                    if (qualityButton.Checked)  // if already selected - do not do anything
                        return;

                    foreach (ToolStripMenuItem item in MainView.QualityButtons.Values)
                    {
                        item.Checked = false;
                    }
                    qualityButton.Checked = true;

                    lastQualityModify = DateTime.Now;
                }
            }

            using (MemoryStream ms = new MemoryStream(FullData))
            {
                try
                {
                    imageBoxDisplayIn.Image = System.Drawing.Image.FromStream(ms);
                }
                catch
                {
                    Debug.WriteLine("[IMAGE] ERROR: Can't build image\n");
                }
            }
        }
        private static byte RoundToNearestTen(byte num)
        {
            if (num % 10 >= 5)
            {
                return (byte)((num / 10 + 1) * 10);
            }
            else
            {
                return (byte)(num / 10 * 10);
            }
        }

        private static uint[] MissingPackets()
        {
            dataPackets = dataPackets.OrderBy(dp => dp.MESSAGE_NUMBER).ToList();

            List<uint> missingList = new List<uint>();
            for (int i = 0; i < dataPackets.Count - 1; i++)
            {
                uint diff = dataPackets[i + 1].MESSAGE_NUMBER - dataPackets[i].MESSAGE_NUMBER;
                if (diff > 1)
                {
                    for (uint j = 1; j < diff; j++)
                    {
                        missingList.Add(dataPackets[i].MESSAGE_NUMBER + j);
                    }
                }
            }

            return missingList.ToArray();
        }
    }
}
