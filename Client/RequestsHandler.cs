using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using SRTShareLib;
using SRTShareLib.PcapManager;
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
            if (handshake_request.SYN_COOKIE == ProtocolManager.GenerateCookie(MainView.GetAdaptedPeerIp(), MainView.myPort))
            {
                HandshakeRequest handshake_response = new HandshakeRequest(OSIManager.BuildBaseLayers(NetworkManager.MacAddress, MainView.serverMac, NetworkManager.LocalIp, ConfigManager.IP, MainView.myPort, ConfigManager.PORT));

                // client -> server (conclusion)

                IpV4Address peer_ip = new IpV4Address(MainView.GetAdaptedPeerIp());
                Packet handshake_packet = handshake_response.Conclusion(init_psn: MainView.INITIAL_PSN, p_ip: peer_ip, clientSide: true, MainView.client_sid, handshake_request.SOCKET_ID, handshake_request.ENCRYPTION_FIELD, handshake_request.SYN_COOKIE);
                PacketManager.SendPacket(handshake_packet);

            }
            else
            {
                // Exit the prgram and send a shutdwon request
                ShutdownRequest shutdown_response = new ShutdownRequest(OSIManager.BuildBaseLayers(NetworkManager.MacAddress, MainView.serverMac, NetworkManager.LocalIp, ConfigManager.IP, MainView.myPort, ConfigManager.PORT));
                Packet shutdown_packet = shutdown_response.Shutdown(MainView.server_sid);
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

            IpV4Address peer_ip = new IpV4Address(MainView.GetAdaptedPeerIp());
            Packet handshake_packet = handshake.Induction(cookie: ProtocolManager.GenerateCookie(MainView.GetAdaptedPeerIp(), myPort), init_psn: MainView.INITIAL_PSN, p_ip: peer_ip, clientSide: true, client_socket_id, 0, (ushort)MainView.ENCRYPTION);

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
