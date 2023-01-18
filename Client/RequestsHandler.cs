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
            if (handshake_request.SYN_COOKIE == ProtocolManager.GenerateCookie(PacketManager.PublicIp, MainView.myPort))
            {
                HandshakeRequest handshake_response = new HandshakeRequest(PacketManager.BuildBaseLayers(PacketManager.MacAddress, MainView.server_mac, PacketManager.LocalIp, ConfigManager.IP, MainView.myPort, ConfigManager.PORT));

                // client -> server (conclusion)

                Packet handshake_packet = handshake_response.Conclusion(init_psn: 0, p_ip: GetAdaptedPeerIp(), clientSide: true, MainView.client_socket_id, handshake_request.SOCKET_ID, cookie: handshake_request.SYN_COOKIE); // ***need to change peer id***
                PacketManager.SendPacket(handshake_packet);

            }
            else
            {
                // Exit the prgram and send a shutdwon request
                ShutDownRequest shutdown_response = new ShutDownRequest(PacketManager.BuildBaseLayers(PacketManager.MacAddress, MainView.server_mac, PacketManager.LocalIp, ConfigManager.IP, MainView.myPort, ConfigManager.PORT));
                Packet shutdown_packet = shutdown_response.Exit();
                PacketManager.SendPacket(shutdown_packet);

                MessageBox.Show("Bad cookie - Exiting...", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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


            Packet handshake_packet = handshake.Induction(cookie: ProtocolManager.GenerateCookie(PacketManager.PublicIp, myPort), init_psn: 0, p_ip: GetAdaptedPeerIp(), clientSide: true, client_socket_id, 0);

            PacketManager.SendPacket(handshake_packet);
        }

        internal static void HandleData(Data.SRTHeader data_request, Cyotek.Windows.Forms.ImageBox pictureBoxDisplayIn)
        {
            ImageDisplay.ProduceImage(data_request, pictureBoxDisplayIn);
        }

        /// <summary>
        /// If the connection is external (the server outside client's subnet) so use the public ip as peer ip (peer ip is the packet sender IP according SRT docs)
        /// </summary>
        /// <returns>Adapted IpV4 Address according the connection type</returns>
        internal static IpV4Address GetAdaptedPeerIp()
        {
            if (MainView.externalConnection)  // if external connection use public ip as peer ip (sender ip)
                return new IpV4Address(PacketManager.PublicIp);
            else
                return new IpV4Address(PacketManager.LocalIp);
        }
    }
}
