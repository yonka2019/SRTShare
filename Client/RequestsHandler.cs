using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using SRTShareLib;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
using SRTShareLib.SRTManager.RequestsFactory;
using System;
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
            if (handshake_request.SYN_COOKIE == ProtocolManager.GenerateCookie(MainView.GetAdaptedIP()))
            {
                HandshakeRequest handshake_response = new HandshakeRequest(OSIManager.BuildBaseLayers(NetworkManager.MacAddress, MainView.server_mac, NetworkManager.LocalIp, ConfigManager.IP, MainView.my_client_port, ConfigManager.PORT));

                // client -> server (conclusion)

                IpV4Address peer_ip = new IpV4Address(MainView.GetAdaptedIP());

                byte[] myPublicKey;
                if (MainView.ENCRYPTION != EncryptionType.None)
                    myPublicKey = DiffieHellman.MyPublicKey;
                else
                    myPublicKey = new byte[DiffieHellman.PUBLIC_KEY_SIZE];

                Packet handshake_packet = handshake_response.Conclusion(init_psn: MainView.INITIAL_PSN, p_ip: peer_ip, clientSide: true, MainView.client_sid, handshake_request.SOCKET_ID, handshake_request.ENCRYPTION_TYPE, myPublicKey, handshake_request.SYN_COOKIE);
                PacketManager.SendPacket(handshake_packet);

            }
            else
            {
                // Exit the prgram and send a shutdwon request
                ShutdownRequest shutdown_request = new ShutdownRequest(OSIManager.BuildBaseLayers(NetworkManager.MacAddress, MainView.server_mac, NetworkManager.LocalIp, ConfigManager.IP, MainView.my_client_port, ConfigManager.PORT));
                Packet shutdown_packet = shutdown_request.Shutdown(MainView.server_sid);
                PacketManager.SendPacket(shutdown_packet);

                MessageBox.Show("Bad cookie - Stopping", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            Packet handshake_packet = handshake.Induction(cookie: ProtocolManager.GenerateCookie(MainView.GetAdaptedIP()), init_psn: MainView.INITIAL_PSN, p_ip: peer_ip, clientSide: true, client_socket_id, 0, (ushort)MainView.ENCRYPTION, new byte[DiffieHellman.PUBLIC_KEY_SIZE]);

            PacketManager.SendPacket(handshake_packet);
        }

        internal static void HandleData(Data.SRTHeader data_request, Cyotek.Windows.Forms.ImageBox pictureBoxDisplayIn)
        {
            ImageDisplay.ProduceImage(data_request, pictureBoxDisplayIn);
        }

        /// <summary>
        /// Handle server shutdown (only ctrl+c event)
        /// </summary>
        internal static void HandleShutDown()
        {
            ServerAliveChecker.Disable();

            CConsole.WriteLine("[Shutdown] Server stopped", MessageType.txtError);
            MessageBox.Show("Server have been stopped", "Server Stopped", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }
    }
}
