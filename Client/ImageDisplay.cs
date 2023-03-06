﻿using PcapDotNet.Packets;
using SRTShareLib;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.RequestsFactory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Data = SRTShareLib.SRTManager.ProtocolFields.Data;

namespace Client
{
    internal class ImageDisplay
    {
        internal static Cyotek.Windows.Forms.ImageBox ImageBoxDisplayIn { private get; set; }

        private static ushort lastDataPosition;
        private static readonly object _lock = new object();

        private static List<Data.SRTHeader> dataPackets = new List<Data.SRTHeader>();
        internal static long CurrentVideoQuality = ProtocolManager.DEFAULT_QUALITY;

        private static DateTime lastQualityModify;
        private const int MINIMUM_SECONDS_ELPASED_TO_MODIFY = 3;  // don't allow the algorithm to AUTO modify the quality if there is was a quality change

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

        internal static void ProduceImage(Data.SRTHeader data_request)
        {
            // in case if chunk had received while other chunk is building (in this method), the new chunk will create new task and
            // will intervene the proccess, so to avoid multi access tries, lock the global resource (allChunks) until the task will finish
            lock (_lock)
            {
                if (data_request.PACKET_POSITION_FLAG == (ushort)Data.PositionFlags.FIRST)
                {
                    if (lastDataPosition == (ushort)Data.PositionFlags.MIDDLE)  // LAST lost, image received [FIRST MID MID MID ---- FIRST]
                    {
                        ShowImage(false);
                        dataPackets.Clear();
                    }
                    dataPackets.Add(data_request);
                }
                else if (data_request.PACKET_POSITION_FLAG == (ushort)Data.PositionFlags.LAST)  // full image received (but maybe middle packets get lost)
                {
                    dataPackets.Add(data_request);
                    ShowImage(true);
                    dataPackets.Clear();
                }
                else
                {
                    dataPackets.Add(data_request);
                }

                lastDataPosition = data_request.PACKET_POSITION_FLAG;
            }
        }

        private static void ShowImage(bool lastChunkReceived)
        {
            uint[] lostChunks = GetMissingPackets();

            if (RetransmissionRequired(lostChunks))
                return;

            if (!lastChunkReceived)
                Debug.WriteLine("[IMAGE BUILDER] ERROR: LAST chunk missing (SHOWING IMAGE)\n");

            LowerQualityNecessity(lostChunks);


            using (MemoryStream ms = new MemoryStream(FullData))
            {
                try
                {
                    ImageBoxDisplayIn.Image = System.Drawing.Image.FromStream(ms);
                }
                catch
                {
                    Debug.WriteLine("[IMAGE BUILDER] ERROR: Can't build image at all\n");
                }
            }
        }

        /// <summary>
        /// Checks if retransmission needed due packet lost
        /// </summary>
        /// <param name="lostChunks">lost chunks</param>
        private static bool RetransmissionRequired()
        {
            // if there are missing chnuks -> send a NAK request in order to ask the server to retransmit the image
            // (the whole message numbers of those sequence number)
            if ((lostChunks.Length > 0) && MainView.RETRANSMISSION_MODE)  // retranmission required
            {
                Console.WriteLine("need to retr: " + dataPackets[0].SEQUENCE_NUMBER);

                RequestsHandler.RequestForRetransmit(dataPackets[0].SEQUENCE_NUMBER);
                dataPackets.Clear();
                return true;
            }

            else  // if all the packets of the current image were received -> send an ack packet with the image's sequence number to clean servers saved images bufer
            {
                RequestsHandler.SendImageConfirm(dataPackets[0].SEQUENCE_NUMBER);
                return false;
            }
        }

        /// <summary>
        /// Checks if auto quality control should lower the quality due high packet lost
        /// </summary>
        /// <param name="lostChunks">lost chunks</param>
        private static void LowerQualityNecessity(uint[] lostChunks)
        {
            // dataPackets.Last().MESSAGE_NUMBER - the last message number which is the max
            double packetsShouldLost = Math.Ceiling(dataPackets.Last().MESSAGE_NUMBER * (MainView.DATA_LOSS_PERCENT_REQUIRED / 100.0));
            TimeSpan timeElapsed = DateTime.Now - lastQualityModify;

            if ((packetsShouldLost <= lostChunks.Length)  // check if necessary

                && MainView.AutoQualityControl  // check if option enabled

                && timeElapsed.TotalSeconds > MINIMUM_SECONDS_ELPASED_TO_MODIFY)  // check if min required time elapsed
            {
                if (CurrentVideoQuality - MainView.DATA_DECREASE_QUALITY_BY > 0)
                {
                    CurrentVideoQuality -= MainView.DATA_DECREASE_QUALITY_BY;  // down quality and send quality update request 

                    QualityUpdateRequest qualityUpdate_request = new QualityUpdateRequest(OSIManager.BuildBaseLayers(NetworkManager.MacAddress, MainView.server_mac, NetworkManager.LocalIp, ConfigManager.IP, MainView.my_client_port, ConfigManager.PORT));
                    Packet qualityUpdate_packet = qualityUpdate_request.UpdateQuality(MainView.server_sid, MainView.my_client_sid, CurrentVideoQuality);
                    PacketManager.SendPacket(qualityUpdate_packet);

                    Debug.WriteLine($"[QUALITY-CONTROL] Quality reduced to {CurrentVideoQuality}\n" +
                        $"[-] LOST: {lostChunks.Length}\n" +
                        $"[-] MIN-TO-LOST: {packetsShouldLost}");

                    ToolStripMenuItem qualityButton = MainView.QualityButtons[CurrentVideoQuality.RoundToNearestTen()];
                    qualityButton.PerformClick();  // simulate click

                    lastQualityModify = DateTime.Now;
                }
            }
        }

        private static uint[] GetMissingPackets()
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
