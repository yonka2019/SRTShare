﻿using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using SRTShareLib;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
using SRTShareLib.SRTManager.RequestsFactory;
using System;
using System.Diagnostics;
using System.Windows.Forms;

using CConsole = SRTShareLib.CColorManager;
using Control = SRTShareLib.SRTManager.ProtocolFields.Control;
using Data = SRTShareLib.SRTManager.ProtocolFields.Data;

namespace Client
{
    internal class RequestsHandler
    {
        /// <summary>
        /// The function handles what happens after getting an induction message from the server
        /// </summary>
        /// <param name="handshake_request">Handshake object</param>
        internal static void HandleInduction(Control.Handshake handshake_request)
        {
            HandshakeRequest handshake_response = new HandshakeRequest(OSIManager.BuildBaseLayers(NetworkManager.MacAddress, MainView.Server_MAC, NetworkManager.LocalIp, ConfigManager.IP, MainView.MY_PORT, ConfigManager.PORT));

            // client -> server (conclusion)

            IpV4Address peer_ip = new IpV4Address(MainView.GetAdaptedIP());

            byte[] myPublicKey;
            if (MainView.ENCRYPTION != EncryptionType.None)
                myPublicKey = DiffieHellman.MyPublicKey;
            else
                myPublicKey = new byte[DiffieHellman.PUBLIC_KEY_SIZE];

            Packet handshake_packet = handshake_response.Conclusion(init_psn: MainView.INITIAL_PSN, p_ip: peer_ip, clientSide: true, MainView.My_SID, handshake_request.SOURCE_SOCKET_ID, handshake_request.ENCRYPTION_TYPE, myPublicKey, handshake_request.RETRANSMISSION_MODE);
            PacketManager.SendPacket(handshake_packet);
        }

        internal static void HandleConclusion(MainView mainView, Control.Handshake handshake_request)
        {
            // encryption data received - initialize him for future decrypt necessity
            MainView.Server_EncryptionControl = EncryptionFactory.CreateEncryption((EncryptionType)handshake_request.ENCRYPTION_TYPE, handshake_request.ENCRYPTION_PEER_PUBLIC_KEY);

            mainView.Invoke((MethodInvoker)delegate
            {
                mainView.VideoBox.Text = "";
            });

            CConsole.WriteLine("[Handshake completed] Starting video display\n", MessageType.bgSuccess);

            EnableQualityButtons(mainView);
        }

        /// <summary>
        /// The function handles what happens after getting an arp message from the server (BEGGINING SRT CONNECTION - INDUCTION 1)
        /// </summary>
        /// <param name="server_mac">Server's mac</param>
        /// <param name="myPort">Client's port</param>
        /// <param name="client_socket_id">Client's socket id</param>
        internal static void HandleArp(string server_mac, ushort myPort, uint client_socket_id)
        {
            HandshakeRequest handshake = new HandshakeRequest
                    (OSIManager.BuildBaseLayers(NetworkManager.MacAddress, server_mac, NetworkManager.LocalIp, ConfigManager.IP, myPort, ConfigManager.PORT));

            IpV4Address peer_ip = new IpV4Address(MainView.GetAdaptedIP());
            Packet handshake_packet = handshake.Induction(init_psn: MainView.INITIAL_PSN, p_ip: peer_ip, clientSide: true, client_socket_id, 0, (ushort)MainView.ENCRYPTION, new byte[DiffieHellman.PUBLIC_KEY_SIZE], MainView.RETRANSMISSION_MODE);

            PacketManager.SendPacket(handshake_packet);
        }

        internal static void HandleKeepAlive()
        {
            Debug.WriteLine("[KEEP-ALIVE] Received request\n");

            KeepAliveRequest keepAlive_response = new KeepAliveRequest(OSIManager.BuildBaseLayers(NetworkManager.MacAddress, MainView.Server_MAC, NetworkManager.LocalIp, ConfigManager.IP, MainView.MY_PORT, ConfigManager.PORT));
            Packet keepAlive_confirm = keepAlive_response.Alive(MainView.Server_SID, MainView.My_SID);
            PacketManager.SendPacket(keepAlive_confirm);

            Debug.WriteLine("[KEEP-ALIVE] Sending confirm\n");
        }

        internal static void HandleImageData(Data.ImageData data_request)
        {
            DataDebug.VideoReceived++;

            if (data_request.ENCRYPTION_FLAG)
            {
                if (!Enum.IsDefined(typeof(EncryptionType), MainView.ENCRYPTION))
                    throw new Exception($"'{MainView.ENCRYPTION}' This encryption method isn't supported yet");

                data_request.DATA = MainView.Server_EncryptionControl.TryDecrypt(data_request.DATA);
            }
            ImageDisplay.ProduceImage(data_request);
        }

        internal static void HandleAudioData(Data.AudioData data_request)
        {
            DataDebug.AudioReceived++;

            if (data_request.ENCRYPTION_FLAG)
            {
                if (!Enum.IsDefined(typeof(EncryptionType), MainView.ENCRYPTION))
                    throw new Exception($"'{MainView.ENCRYPTION}' This encryption method isn't supported yet");

                data_request.DATA = MainView.Server_EncryptionControl.TryDecrypt(data_request.DATA);
            }
            AudioPlay.ProduceAudio(data_request);
        }

        /// <summary>
        /// Handle server shutdown (only ctrl+c event)
        /// </summary>
        internal static void HandleShutDown()
        {
            ServerAliveChecker.Disable();

            CConsole.WriteLine("[Shutdown] Server stopped", MessageType.txtError);
            MessageBox.Show("Server has been stopped", "Server Stopped", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }

        /// <summary>
        /// send request of transmit corrupted image
        /// </summary>
        /// <param name="corruptedImageSequenceNumber"></param>
        internal static void RequestForRetransmit(uint corruptedImageSequenceNumber)
        {
            NAKRequest nak_request = new NAKRequest
                                (OSIManager.BuildBaseLayers(NetworkManager.MacAddress, MainView.Server_MAC, NetworkManager.LocalIp, ConfigManager.IP, MainView.MY_PORT, ConfigManager.PORT));

            Packet nak_packet = nak_request.RequestRetransmit(corruptedImageSequenceNumber, MainView.Server_SID, MainView.My_SID);
            PacketManager.SendPacket(nak_packet);
        }

        /// <summary>
        /// When image fully received send to server confirm (via ACK) that the whole image received correctly and can be cleaned from server buffer
        /// </summary>
        /// <param name="goodImageSequenceNumber"></param>
        internal static void SendImageConfirm(uint goodImageSequenceNumber)
        {
            ACKRequest ack_request = new ACKRequest
                                (OSIManager.BuildBaseLayers(NetworkManager.MacAddress, MainView.Server_MAC, NetworkManager.LocalIp, ConfigManager.IP, MainView.MY_PORT, ConfigManager.PORT));

            Packet ack_packet = ack_request.ConfirmReceivedImage(goodImageSequenceNumber, MainView.Server_SID, MainView.My_SID);

            // send triple ack confirm (if one of AKCs them lost or corruped) - server will get only the one he receive and ignore the others
            PacketManager.SendPacket(ack_packet);
            PacketManager.SendPacket(ack_packet);
            PacketManager.SendPacket(ack_packet);
        }

        /// <summary>
        /// When video staged achieved, the quality buttons should be enabled 
        /// </summary>
        private static void EnableQualityButtons(MainView mainView)
        {
            foreach (ToolStripMenuItem button in MainView.QualityButtons.Values)
            {
                if (mainView.QualitySetter.InvokeRequired && mainView.QualitySetter.IsHandleCreated)
                {
                    mainView.QualitySetter.Invoke((MethodInvoker)delegate
                    {
                        button.Enabled = true;
                    });
                }
                else
                    button.Enabled = true;

            }
        }
    }
}
