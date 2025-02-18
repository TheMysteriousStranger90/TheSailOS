using Cosmos.HAL.Audio;
using Cosmos.System.Audio;
using Cosmos.System.Audio.DSP.Processing;
using Cosmos.System.Audio.IO;
using System;
using Cosmos.HAL.Drivers.PCI.Audio;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Audio
{
    public class AudioManager : IAudioManager
    {
        private AudioDriver _driver;
        private AudioMixer _mixer;
        private AudioStream _audioStream;
        private Cosmos.System.Audio.AudioManager _audioManager;
        private bool _isInitialized;
        private const float DefaultGain = 0.5f;
        private const int BufferSize = 4096;

        public bool IsAudioEnabled => _isInitialized;

        public void Initialize()
        {
            try
            {
                ConsoleManager.WriteLineColored("[INFO] Initializing audio driver...", ConsoleStyle.Colors.Warning);

                _driver = AC97.Initialize(BufferSize);
                _mixer = new AudioMixer();
                
                _audioManager = new Cosmos.System.Audio.AudioManager()
                {
                    Stream = _mixer,
                    Output = _driver
                };

                _audioManager.Enable();
                _isInitialized = true;

                ConsoleManager.WriteLineColored("[INFO] Audio driver initialized successfully.",
                    ConsoleStyle.Colors.Success);
            }
            catch (InvalidOperationException)
            {
                ConsoleManager.WriteLineColored("[ERROR] No AC97 device found.", ConsoleStyle.Colors.Error);
                _isInitialized = false;
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"[ERROR] Audio initialization failed: {ex.Message}",
                    ConsoleStyle.Colors.Error);
                _isInitialized = false;
            }
        }

        public void Play(byte[] audioData)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("Audio system is not initialized.");
            }

            try
            {
                _audioStream = MemoryAudioStream.FromWave(audioData);
                _audioStream.PostProcessors.Add(new GainPostProcessor(DefaultGain));
                _mixer.Streams.Add(_audioStream);

                _audioManager = new Cosmos.System.Audio.AudioManager
                {
                    Stream = _mixer,
                    Output = _driver
                };

                _audioManager.Enable();

                ConsoleManager.WriteLineColored("[INFO] Audio playback started.", ConsoleStyle.Colors.Success);
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"[ERROR] Failed to play audio: {ex.Message}",
                    ConsoleStyle.Colors.Error);
                throw;
            }
        }

        public void Stop()
        {
            if (!_isInitialized)
            {
                return;
            }

            try
            {
                _audioManager?.Disable();
                _mixer?.Streams.Clear();

                ConsoleManager.WriteLineColored("[INFO] Audio playback stopped.", ConsoleStyle.Colors.Success);
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"[ERROR] Failed to stop audio: {ex.Message}",
                    ConsoleStyle.Colors.Error);
            }
        }

        public void SetGain(float gain)
        {
            if (!_isInitialized || _audioStream == null)
            {
                return;
            }

            try
            {
                var gainProcessor = _audioStream.PostProcessors.Find(p => p is GainPostProcessor) as GainPostProcessor;
                if (gainProcessor != null)
                {
                    gainProcessor.Gain = Math.Clamp(gain, 0.0f, 1.0f);
                }
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"[ERROR] Failed to set gain: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }
    }
}