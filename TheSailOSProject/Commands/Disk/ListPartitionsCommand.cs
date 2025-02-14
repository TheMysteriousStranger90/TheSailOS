using System;
using TheSailOSProject.FileSystem;

namespace TheSailOSProject.Commands.Disk
{
    public class ListPartitionsCommand : ICommand
    {
        private readonly IDiskManager _diskManager;

        public ListPartitionsCommand(IDiskManager diskManager)
        {
            _diskManager = diskManager;
        }

        public void Execute(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Usage: partinfo <drive_letter>");
                return;
            }

            _diskManager.ListPartitions(args[0]);
        }
    }
}