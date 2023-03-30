using SRTShareLib;
using SRTShareLib.SRTManager.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class AudioManager
    {
        private readonly SClient client;
        private bool connected;
        public readonly BaseEncryption ClientEncryption;

        private static uint current_sequence_number;

        internal AudioManager(SClient client, bool connected, BaseEncryption clientEncryption)
        {
            this.client = client;
            this.connected = connected;
            ClientEncryption = clientEncryption;
        }

        private void AudioInit()
        {

        }
    }
}
