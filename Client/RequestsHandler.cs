using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using SRTLibrary;
using SRTLibrary.SRTManager.RequestsFactory;
using System.Windows.Forms;
using Control = SRTLibrary.SRTManager.ProtocolFields.Control;
using Data = SRTLibrary.SRTManager.ProtocolFields.Data;

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
            if (handshake_request.SYN_COOKIE == ProtocolManager.GenerateCookie(PacketManager.LocalIp, MainView.myPort))
            {
                HandshakeRequest handshake_response = new HandshakeRequest(PacketManager.BuildBaseLayers(PacketManager.MacAddress, MainView.server_mac, PacketManager.LocalIp, ConfigManager.IP, MainView.myPort, ConfigManager.PORT));

                // client -> server (conclusion)
                IpV4Address peer_ip = new IpV4Address(PacketManager.LocalIp);

                Packet handshake_packet = handshake_response.Conclusion(init_psn: 0, p_ip: peer_ip, clientSide: true, MainView.client_socket_id, handshake_request.SOCKET_ID, cookie: handshake_request.SYN_COOKIE); // ***need to change peer id***
                PacketManager.SendPacket(handshake_packet);
            }
            else
            {
                // Exit the prgram and send a shutdwon request
                ShutDownRequest shutdown_response = new ShutDownRequest(PacketManager.BuildBaseLayers(PacketManager.MacAddress, MainView.server_mac, PacketManager.LocalIp, ConfigManager.IP, MainView.myPort, ConfigManager.PORT));
                Packet shutdown_packet = shutdown_response.Exit();
                PacketManager.SendPacket(shutdown_packet);

                MessageBox.Show("Wrong cookie - Exiting...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// The function handles what happens after getting an arp message from the server
        /// </summary>
        /// <param name="server_mac">Server's mac</param>
        /// <param name="myPort">Client's port</param>
        /// <param name="client_socket_id">Client's socket id</param>
        internal static void HandleArp(string server_mac, ushort myPort, uint client_socket_id)
        {
            HandshakeRequest handshake = new HandshakeRequest
                    (PacketManager.BuildBaseLayers(PacketManager.MacAddress, server_mac, PacketManager.LocalIp, ConfigManager.IP, myPort, ConfigManager.PORT));

            IpV4Address peer_ip = new IpV4Address(PacketManager.LocalIp);
            Packet handshake_packet = handshake.Induction(cookie: ProtocolManager.GenerateCookie(PacketManager.LocalIp, myPort), init_psn: 0, p_ip: peer_ip, clientSide: true, client_socket_id, 0);

            PacketManager.SendPacket(handshake_packet);
        }

        internal static void HandleData(Data.SRTHeader data_request, PictureBox pictureBoxDisplayIn)
        {
            ImageDisplay.ProduceImage(data_request, pictureBoxDisplayIn);
        }
    }
}
