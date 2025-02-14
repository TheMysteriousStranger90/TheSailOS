using System;
using TheSailOSProject.Audio;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.Audio
{
    public class StopAudioCommand : ICommand
    {
        private readonly IAudioManager _audioManager;
        private readonly ICurrentDirectoryManager _currentDirectoryManager;

        public StopAudioCommand(IAudioManager audioManager, ICurrentDirectoryManager currentDirectoryManager)
        {
            _audioManager = audioManager ?? throw new ArgumentNullException(nameof(audioManager));
            _currentDirectoryManager = currentDirectoryManager ?? throw new ArgumentNullException(nameof(currentDirectoryManager));
        }

        public void Execute(string[] args)
        {
            if (args.Length > 0)
            {
                ConsoleManager.WriteLineColored("Usage: stop (no arguments required)", ConsoleStyle.Colors.Warning);
                return;
            }

            try
            {
                if (!_audioManager.IsAudioEnabled)
                {
                    ConsoleManager.WriteLineColored("Audio system is not initialized.", ConsoleStyle.Colors.Warning);
                    return;
                }

                ConsoleManager.WriteLineColored($"Stopping audio playback in: {_currentDirectoryManager.GetCurrentDirectory()}", 
                    ConsoleStyle.Colors.Primary);
                _audioManager.Stop();
                ConsoleManager.WriteLineColored("Audio playback stopped successfully.", ConsoleStyle.Colors.Success);
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error stopping audio playback: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }
    }
}