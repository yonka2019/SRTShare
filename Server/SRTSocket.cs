using SRTShareLib;

namespace Server
{
    internal class SRTSocket
    {
        internal SClient SocketAddress { get; private set; }  // address & port
        internal Managers.KeepAliveManager KeepAlive { get; private set; }
        internal Managers.VideoManager Video { get; private set; }
        internal Managers.AudioManager Audio { get; private set; }

        internal SRTSocket(SClient socketAddress, Managers.KeepAliveManager kaManager, Managers.VideoManager dataManager, Managers.AudioManager audio)
        {
            SocketAddress = socketAddress;
            KeepAlive = kaManager;
            Video = dataManager;
            Audio = audio;
        }
    }
}
