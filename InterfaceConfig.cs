using System;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;

static public class InterfaceConfig
{
	public static PacketDevice device;

    static public InterfaceConfig()
	{
        IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;
        int deviceIndex = -1;

        if (allDevices.Count == 0)
            return;

        // Print the list
        for (int i = 0; i != allDevices.Count; ++i)
        {
            LivePacketDevice device = allDevices[i];
            if (device.Description != null)
            {
                if (device.Description.Contains(DEFAULT_INTERFACE_SUBSTRING))
                {
                    deviceIndex = i + 1;
                    break;
                }
            }
        }

        // Take the selected adapter
        PacketDevice selectedDevice = allDevices[deviceIndex - 1];
    }
}
