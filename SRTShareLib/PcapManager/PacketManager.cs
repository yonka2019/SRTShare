using PcapDotNet.Core;
using PcapDotNet.Packets;
using CConsole = SRTShareLib.CColorManager;

namespace SRTShareLib.PcapManager
{
    public static class PacketManager
    {
        /// <summary>
        /// The function sends the given packet
        /// </summary>
        /// <param name="packetToSend">The packet to send</param>
        public static void SendPacket(Packet packetToSend)
        {
            using (PacketCommunicator communicator = NetworkManager.Device.Open(100, // name of the device
                                 PacketDeviceOpenAttributes.DataTransferUdpRemote, // udp mode
                                 1000)) // read timeout
            {
                communicator.SendPacket(packetToSend);
            }
        }

        /// <summary>
        /// The fucntion handles the packets recieves by a handle to a function that it gets
        /// </summary>
        /// <param name="count"></param>
        /// <param name="callback">Handle to a function</param>
        public static void ReceivePackets(int count, HandlePacket callback)
        {
            using (PacketCommunicator communicator =
            NetworkManager.Device.Open(65536,                         // portion of the packet to capture
                                                                      // 65536 guarantees that the whole packet will be captured on all the link layers
                    PacketDeviceOpenAttributes.DataTransferUdpRemote,  // udp mode
                    1000))                                  // read timeout
            {
#if DEBUG
                CConsole.WriteLine($"# [LISTENING] {callback.Method.Name}\n", MessageType.txtInfo);
#endif
                communicator.ReceivePackets(0, callback);
            }
        }
    }
}
