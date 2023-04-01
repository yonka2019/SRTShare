using NAudio.Wave;
using System.Collections.Generic;
using System.Diagnostics;

using Data = SRTShareLib.SRTManager.ProtocolFields.Data;

namespace Client
{
    internal static class AudioPlay
    {
        private static ushort lastDataPosition;
        private static readonly object _lock = new object();

        private static readonly List<Data.AudioData> audioDataPackets = new List<Data.AudioData>();

        private static WaveOutEvent waveOut;
        private static BufferedWaveProvider waveProvider;

        private const int SAMPLE_RATE = 44100;
        private const int CHANNELS = 2;

        static AudioPlay()
        {
            waveOut = new WaveOutEvent();
            waveProvider = new BufferedWaveProvider(new WaveFormat(SAMPLE_RATE, CHANNELS));

            waveOut.Init(waveProvider);
            waveOut.Play();
        }

        private static byte[] Audio  // if full image already built, return it. otherwise, build the full image from the all chunks 
        {
            get
            {
                List<byte> fullData = new List<byte>();

                foreach (Data.AudioData srtHeader in audioDataPackets)  // add each data into [List<byte> fullData]
                {
                    fullData.AddRange(srtHeader.DATA);
                }
                // convert to byte array and return
                return fullData.ToArray();
            }
        }

        internal static void DisposeAudio()
        {
            waveOut.Stop();
            waveOut.Dispose();
        }

        internal static void ProduceAudio(Data.AudioData audio_chunk)
        {
            // in case if chunk had received while other chunk is building (in this method), the new chunk will create new task and
            // will intervene the proccess, so to avoid multi access tries, lock the global resource (allChunks) until the task will finish
            lock (_lock)
            {
                if (audio_chunk.PACKET_POSITION_FLAG == (ushort)Data.PositionFlags.SINGLE_DATA_PACKET)
                {
                    audioDataPackets.Add(audio_chunk);
                    PlayAudio(Audio, true);
                    audioDataPackets.Clear();
                }

                else if (audio_chunk.PACKET_POSITION_FLAG == (ushort)Data.PositionFlags.FIRST)
                {
                    if (lastDataPosition == (ushort)Data.PositionFlags.MIDDLE)  // LAST lost, image received [FIRST MID MID MID ---- FIRST]
                    {
                        PlayAudio(Audio, false);
                        audioDataPackets.Clear();
                    }
                    audioDataPackets.Add(audio_chunk);
                }

                else if (audio_chunk.PACKET_POSITION_FLAG == (ushort)Data.PositionFlags.LAST)  // full image received (but maybe middle packets get lost)
                {
                    audioDataPackets.Add(audio_chunk);
                    PlayAudio(Audio, true);
                    audioDataPackets.Clear();
                }

                else
                {
                    audioDataPackets.Add(audio_chunk);
                }

                lastDataPosition = audio_chunk.PACKET_POSITION_FLAG;
            }
        }

        private static void PlayAudio(byte[] audio, bool lastChunkReceived)
        {
            if (!lastChunkReceived)
                Debug.WriteLine("[AUDIO BUILDER] ERROR: LAST chunk missing (PLAYING AUDIO)\n");

            try
            {
                waveProvider.AddSamples(audio, 0, audio.Length);
            }
            catch
            {
                Debug.WriteLine("[AUDIO BUILDER] ERROR: Can't build audio at all\n");
            }
        }
    }
}
