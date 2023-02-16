﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Data = SRTShareLib.SRTManager.ProtocolFields.Data;

namespace Client
{
    internal class ImageDisplay
    {
        private static ushort lastDataPosition;
        private static readonly object _lock = new object();

        private static List<Data.SRTHeader> dataPackets = new List<Data.SRTHeader>();

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

            var a = MissingPackets();
            Console.WriteLine(string.Join(",", a));

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
