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
    internal static class ImageDisplay
    {
        internal static Cyotek.Windows.Forms.ImageBox ImageBoxDisplayIn { private get; set; }
        private static ushort lastDataPosition;
        private static readonly object _lock = new object();

        private static Dictionary<uint, List<Data.SRTHeader>> dataBuffer = new Dictionary<uint, List<Data.SRTHeader>>(); 
        private static List<uint> lostSequences = new List<uint>();

        internal static long CurrentVideoQuality = ProtocolManager.DEFAULT_QUALITY;

        private static DateTime lastQualityModify;
        private const int MINIMUM_SECONDS_ELPASED_TO_MODIFY = 3;  // don't allow the algorithm to AUTO modify the quality if there is was a quality change

        private static byte[] GetFullData(uint packetSequenceNumber)
        {
            List<byte> fullData = new List<byte>();

            foreach (Data.SRTHeader srtHeader in dataBuffer[packetSequenceNumber])  // add each data into [List<byte> fullData]
            {
                fullData.AddRange(srtHeader.DATA);
            }
            // convert to byte array and return
            return fullData.ToArray();
        }

        internal static void ProduceImage(Data.SRTHeader data_request)
        {
            // in case if chunk had received while other chunk is building (in this method), the new chunk will create new task and
            // will intervene the proccess, so to avoid multi access tries, lock the global resource (allChunks) until the task will finish
            lock (_lock)
            {
                if (data_request.PACKET_POSITION_FLAG == (ushort)Data.PositionFlags.FIRST)
                {
                    //if (lastDataPosition == (ushort)Data.PositionFlags.MIDDLE)  // LAST lost, image received [FIRST MID MID MID ---- FIRST]
                    //{
                    //    ShowImage(data_request.SEQUENCE_NUMBER, false);
                    //}

                    if (!dataBuffer.ContainsKey(data_request.SEQUENCE_NUMBER))
                        dataBuffer[data_request.SEQUENCE_NUMBER] = new List<Data.SRTHeader>();

                    dataBuffer[data_request.SEQUENCE_NUMBER].Add(data_request); // adding new chunk to its related sequence number
                }

                else if (data_request.PACKET_POSITION_FLAG == (ushort)Data.PositionFlags.LAST)  // full image received (but maybe middle packets get lost)
                {
                    dataBuffer[data_request.SEQUENCE_NUMBER].Add(data_request); // adding new chunk to its related sequence number

                    ShowImage(data_request.SEQUENCE_NUMBER, true);

                }

                else
                {
                    dataBuffer[data_request.SEQUENCE_NUMBER].Add(data_request); // adding new chunk to its related sequence number
                }

                lastDataPosition = data_request.PACKET_POSITION_FLAG;
            }
        }

        private static List<uint> missingMessageNumbersFromMissingPackets(List<Data.SRTHeader> my_list)
        {
            List<uint> my_list_messsages = new List<uint>();

            foreach(var item in my_list)
            {
                my_list_messsages.Add(item.MESSAGE_NUMBER);
            }

            return my_list_messsages;
        }

        private static void ShowImage(uint packetSequenceNumber, bool lastChunkReceived)
        {
            Console.WriteLine("last chunk: " + lastChunkReceived);
            uint[] lostChunks_MessageNumber = MissingPackets(packetSequenceNumber);

            // if there are missing packets -> send a nak packet with missing packets
            if (lostChunks_MessageNumber.Length > 0)
            {
                dataBuffer[packetSequenceNumber].Clear(); // last chunk of sequence number -> clear it
                lostSequences.Add(packetSequenceNumber);
                List <uint> CurrentVideoQuality = missingMessageNumbersFromMissingPackets(dataBuffer[packetSequenceNumber]);
                SendMissingPackets(packetSequenceNumber, lostChunks_MessageNumber);
                return;
            }

           // else // if all the packets of the current image were received -> send an ack packet with the image's sequence number
             //   SendImageConfirm(packetSequenceNumber);



            if (!lastChunkReceived)
                Debug.WriteLine("[IMAGE BUILDER] ERROR: LAST chunk missing (SHOWING IMAGE)\n");

            double packetsShouldLost = Math.Ceiling(dataBuffer[packetSequenceNumber].Last().MESSAGE_NUMBER * (MainView.DATA_LOSS_PERCENT_REQUIRED / 100.0));
            TimeSpan timeElapsed = DateTime.Now - lastQualityModify;

            // dataPackets.Last().MESSAGE_NUMBER - the last seq number which is the max
            if ((packetsShouldLost <= lostChunks_MessageNumber.Length)  // check if necessary

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
                        $"[-] LOST: {lostChunks_MessageNumber.Length}\n" +
                        $"[-] MIN-TO-LOST: {packetsShouldLost}");
                    CConsole.WriteLine($"[Auto Quality Control] Quality updated: {CurrentVideoQuality}\n", MessageType.txtWarning);

                    ToolStripMenuItem qualityButton = MainView.QualityButtons[CurrentVideoQuality.RoundToNearestTen()];

                    if (qualityButton.Checked)  // if already selected - do not do anything
                        return;

                    foreach (ToolStripMenuItem item in MainView.QualityButtons.Values)  // set all buttons to unchecked
                    {
                        if (item.Owner.InvokeRequired && item.Owner.IsHandleCreated)
                        {
                            item.Owner.Invoke((MethodInvoker)delegate
                            {
                                item.Checked = false;
                            });
                        }
                        else
                            item.Checked = false;
                    }

                    if (qualityButton.Owner.InvokeRequired && qualityButton.Owner.IsHandleCreated)
                    {
                        qualityButton.Owner.Invoke((MethodInvoker)delegate
                        {
                            qualityButton.Checked = true;  // check the new setted quality
                        });
                    }
                    else
                        qualityButton.Checked = true;

                    lastQualityModify = DateTime.Now;
                }
            }

            using (MemoryStream ms = new MemoryStream(GetFullData(packetSequenceNumber)))
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

        private static uint[] MissingPackets(uint packetSequenceNumber)
        {
            if(!dataBuffer.ContainsKey(packetSequenceNumber))
            {
                Console.WriteLine("hello");
            }
            dataBuffer[packetSequenceNumber] = dataBuffer[packetSequenceNumber].OrderBy(dp => dp.MESSAGE_NUMBER).ToList();

            List<uint> missingList = new List<uint>();
            for (int i = 0; i < dataBuffer[packetSequenceNumber].Count - 1; i++)
            {
                uint diff = dataBuffer[packetSequenceNumber][i + 1].MESSAGE_NUMBER - dataBuffer[packetSequenceNumber][i].MESSAGE_NUMBER;
                if (diff > 1)
                {
                    for (uint j = 1; j < diff; j++)
                    {
                        missingList.Add(dataBuffer[packetSequenceNumber][i].MESSAGE_NUMBER + j);
                    }
                }
            }

            return  missingList.ToArray();
        }

        private static void SendMissingPackets(uint corrupted_sequence_number, uint[] missedSequenceNumbers)
        {
            NAKRequest nak_request = new NAKRequest
                                (OSIManager.BuildBaseLayers(NetworkManager.MacAddress, MainView.server_mac, NetworkManager.LocalIp, ConfigManager.IP, MainView.my_client_port, ConfigManager.PORT));

            Packet nak_packet = nak_request.SendMissingPackets(corrupted_sequence_number, missedSequenceNumbers.ToList(), MainView.server_sid, MainView.my_client_sid);
            PacketManager.SendPacket(nak_packet);
        }

        private static void SendImageConfirm(uint ackSequenceNumber)
        {
            ACKRequest ack_request = new ACKRequest
                                (OSIManager.BuildBaseLayers(NetworkManager.MacAddress, MainView.server_mac, NetworkManager.LocalIp, ConfigManager.IP, MainView.my_client_port, ConfigManager.PORT));

            Packet ack_packet = ack_request.NotifyReceived(ackSequenceNumber, MainView.server_sid, MainView.my_client_sid);
            PacketManager.SendPacket(ack_packet);
        }
    }
}
