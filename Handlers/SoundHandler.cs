using SimpleCTRL.Utils;
using System;
using NAudio.Wave;
using System.IO;
using Rage.Native;

namespace SimpleCTRL.Handlers
{
    internal static class SoundHandler
    {
        private static readonly WaveOutEvent output = new WaveOutEvent();

        public static void PlayAudio(Audio audio)
        {
            string audioFilePath = Path.Combine(ConfigHandler.AudioPath, GetAudioFileName(audio));

            try
            {
                AudioFileReader reader = new AudioFileReader(audioFilePath);

                if (output.PlaybackState != PlaybackState.Stopped)
                {
                    output.Stop();
                }

                output.Volume = NativeFunction.CallByHash<int>(0xC488FF2356EA7791, 300) / 20f;
                output.Init(reader);
                output.Play();
            }
            catch (Exception ex)
            {
                Logging.Error($"could not play sound file: {audioFilePath}", "SoundHandler", ex);
            }
        }

        private static string GetAudioFileName(Audio audio)
        {
            switch (audio)
            {
                case Audio.LowFuel:
                    return "lowfuel.wav";
                default:
                    Logging.Warning("unknown audio type " + Enum.GetName(typeof(Audio), audio), "SoundHandler");
                    return string.Empty;
            }
        }

        public enum Audio
        {
            LowFuel,
        }
    }
}
