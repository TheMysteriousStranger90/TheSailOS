using System;
using TheSailOSProject.Logging;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Users
{
    public class ChangeUsernameCommand : ICommand
    {
        public void Execute(string[] args)
        {
            // Check if user is logged in
            if (Kernel.CurrentUser == null)
            {
                ConsoleManager.WriteLineColored("You must be logged in to change your username.", 
                    ConsoleStyle.Colors.Error);
                return;
            }
            
            if (args.Length < 1)
            {
                ConsoleManager.WriteLineColored("Usage: changeusername <new_username>", ConsoleStyle.Colors.Error);
                return;
            }
            
            string newUsername = args[0];
            string oldUsername = Kernel.CurrentUser.Username;
            
            string validationError;
            if (!UserManager.IsValidUsername(newUsername, out validationError))
            {
                ConsoleManager.WriteLineColored($"Invalid username: {validationError}", ConsoleStyle.Colors.Error);
                return;
            }
            
            if (UserManager.GetUser(newUsername) != null)
            {
                ConsoleManager.WriteLineColored($"Username '{newUsername}' is already taken.", ConsoleStyle.Colors.Error);
                return;
            }
            
            ConsoleManager.WriteColored($"Change username from '{oldUsername}' to '{newUsername}'? (y/n): ", 
                ConsoleStyle.Colors.Warning);
            string confirmation = Console.ReadLine()?.ToLower() ?? "";
            
            if (confirmation != "y" && confirmation != "yes")
            {
                ConsoleManager.WriteLineColored("Username change canceled.", ConsoleStyle.Colors.Primary);
                return;
            }
            
            if (UserManager.ChangeUsername(oldUsername, newUsername))
            {
                ConsoleManager.WriteLineColored($"Username changed successfully to '{newUsername}'.", 
                    ConsoleStyle.Colors.Success);
                
                Log.WriteLog(LogPriority.Info, "User", 
                    $"User '{oldUsername}' changed their username to '{newUsername}'", newUsername);
            }
            else
            {
                ConsoleManager.WriteLineColored("Failed to change username.", ConsoleStyle.Colors.Error);
            }
        }
    }
}