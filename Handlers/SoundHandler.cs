using SimpleCTRL.Utils;
using System;
using NAudio.Wave;
using System.IO;
using Common.Native;
using Rage;

namespace SimpleCTRL.Handlers
{
    internal static class SoundHandler
    {
        private static readonly WaveOutEvent output = new WaveOutEvent();
        private static DateTime lastAudioPlayTime = DateTime.MinValue;
        private static readonly TimeSpan audioCooldown = TimeSpan.FromSeconds(5);

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

                reader.Volume = N.GetProfileSetting(300) / 20f;
                output.Init(reader);
                output.Play();
            }
            catch (Exception ex)
            {
                Logging.Error($"could not play sound file: {audioFilePath}", "SoundHandler", ex);
            }
        }

        public static void PlayAudioSequence(string[] audioFiles, int[] delays)
        {
            if (audioFiles.Length != delays.Length)
            {
                Logging.Error("Mismatch in the number of audio files and delays", "SoundHandler");
                return;
            }

            DateTime currentTime = DateTime.Now;

            if ((currentTime - lastAudioPlayTime) < audioCooldown)
            {
                Logging.Info("Audio spam prevention: Cooldown in effect", "SoundHandler");
                return;
            }

            for (int i = 0; i < audioFiles.Length; i++)
            {
                string audioFilePath = Path.Combine(ConfigHandler.AudioPath, audioFiles[i]);

                try
                {
                    AudioFileReader reader = new AudioFileReader(audioFilePath);

                    if (output.PlaybackState != PlaybackState.Stopped)
                    {
                        output.Stop();
                    }

                    output.Volume = N.GetProfileSetting(300) / 20f;
                    output.Init(reader);
                    output.Play();

                    GameFiber.Wait(delays[i]);

                    reader.Dispose(); 
                }
                catch (Exception ex)
                {
                    Logging.Error($"could not play sound file: {audioFilePath}", "SoundHandler", ex);
                }
            }

            lastAudioPlayTime = DateTime.Now;
        }

        private static string GetAudioFileName(Audio audio)
        {
            switch (audio)
            {
                case Audio.LowFuel:
                    return "LOW_FUEL.wav";
                case Audio.Indicator:
                    return "INDICATOR.wav";
                case Audio.ShiftParkPull:
                    return "SHIFT_PARK_PULL_01.wav";
                case Audio.ShiftParkRelease:
                    return "SHIFT_PARK_RELEASE_01.wav";
                case Audio.Arrived:
                    return "UHAVEARRIVED.wav";
                default:
                    Logging.Warning("unknown audio type " + Enum.GetName(typeof(Audio), audio), "SoundHandler");
                    return string.Empty;
            }
        }

        public enum Audio
        {
            LowFuel,
            Indicator,
            ShiftParkPull,
            ShiftParkRelease,
            Arrived
        }
    }
}
