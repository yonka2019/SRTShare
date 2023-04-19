using NAudio.Wave;
using PcapDotNet.Packets;
using SRTShareLib;
using SRTShareLib.PcapManager;
using SRTShareLib.SRTManager.Encryption;
using SRTShareLib.SRTManager.RequestsFactory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using CConsole = SRTShareLib.CColorManager;  // Colored Console

namespace Server.Managers
{
    internal class AudioManager : IManager
    {
        private readonly SClient client;

        private readonly BaseEncryption clientEncryption;
        private readonly WasapiLoopbackCapture capture;

        private static uint current_sequence_number;

        private const int SAMPLE_RATE = 44100;
        private const int CHANNELS = 2;
        private const int AUDIO_SEQNUM_OFFSET = 50000000; 

        internal AudioManager(SClient client, BaseEncryption baseEncryption, uint intial_sequence_number)
        {
            current_sequence_number = intial_sequence_number + AUDIO_SEQNUM_OFFSET; // the image seq numbers and audio seq numbers will be the same, to prevent that, we are ading offset to audio sequence numbers

            this.client = client;

            clientEncryption = baseEncryption;
            capture = new WasapiLoopbackCapture();
        }

        /// <summary>
        /// The function starts the thread responsible for audio sending
        /// </summary>
        public void Start()
        {
            capture.DataAvailable += Capture_DataAvailable;
            capture.WaveFormat = new WaveFormat(SAMPLE_RATE, CHANNELS);

            capture.StartRecording();

            CConsole.WriteLine($"[Server] [{client.IPAddress}] Audio is being shared\n", MessageType.bgSuccess);
        }

        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] capturedAudio = new byte[e.BytesRecorded];
            Array.Copy(e.Buffer, capturedAudio, e.BytesRecorded);

            SplitAndSend(capturedAudio, current_sequence_number);
            current_sequence_number++;
        }

        /// <summary>
        /// Stops the audio via the condition variable
        /// </summary>
        public void Stop()
        {
            capture.StopRecording();
            capture.Dispose();
        }

        private void SplitAndSend(byte[] audio, uint sequence_number)
        {
            AudioDataRequest dataRequest = new AudioDataRequest(
                               OSIManager.BuildBaseLayers(NetworkManager.MacAddress, client.MacAddress.ToString(), NetworkManager.LocalIp, client.IPAddress.ToString(), ConfigManager.PORT, client.Port));

            // (.MTU - 150; explanation) : To avoid errors with sending, because this field used to set fixed size of splitted data packet,
            // while the real mtu that the interface provides refers the whole size of the packet which get sent,
            // and with the whole srt packet and all layers in will much more.
            // In addition, the encryption will give EXTRA bytes (which is padding for example in AES), it's also one of the reasons why we take extra space
            List<Packet> data_packets = dataRequest.SplitToPackets(audio, sequence_number, (int)client.MTU - 150, clientEncryption);

            foreach (Packet packet in data_packets)
            {
                PacketManager.SendPacket(packet);
                DataDebug.IncAudioSent();
            }
        }
    }
}
