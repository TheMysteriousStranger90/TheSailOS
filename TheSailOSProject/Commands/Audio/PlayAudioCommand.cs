using System;
using System.IO;
using TheSailOSProject.Audio;
using TheSailOSProject.Commands.Directories;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.Audio
{
    public class PlayAudioCommand : ICommand
    {
        private readonly IAudioManager _audioManager;
        private readonly ICurrentDirectoryManager _currentDirectoryManager;

        public PlayAudioCommand(IAudioManager audioManager, ICurrentDirectoryManager currentDirectoryManager)
        {
            _audioManager = audioManager;
            _currentDirectoryManager = currentDirectoryManager;
        }

        public void Execute(string[] args)
        {
            if (args.Length != 1)
            {
                ConsoleManager.WriteLineColored("Usage: play <audio_file>", ConsoleStyle.Colors.Warning);
                return;
            }

            try
            {
                string fullPath = Path.Combine(_currentDirectoryManager.GetCurrentDirectory(), args[0]);
                
                if (!File.Exists(fullPath))
                {
                    ConsoleManager.WriteLineColored($"Audio file not found: {args[0]}", ConsoleStyle.Colors.Error);
                    return;
                }

                ConsoleManager.WriteLineColored($"Playing audio file: {args[0]}", ConsoleStyle.Colors.Primary);
                var audioData = File.ReadAllBytes(fullPath);
                _audioManager.Play(audioData);
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"Error playing audio: {ex.Message}", ConsoleStyle.Colors.Error);
            }
        }
    }
}