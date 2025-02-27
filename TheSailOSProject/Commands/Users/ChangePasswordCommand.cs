using System;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Users
{
    public class ChangePasswordCommand : ICommand
    {
        public void Execute(string[] args)
        {
            if (Kernel.CurrentUser == null)
            {
                ConsoleManager.WriteLineColored("You must be logged in to change your password.",
                    ConsoleStyle.Colors.Error);
                return;
            }

            if (Kernel.CurrentUser.PasswordExpired)
            {
                ConsoleManager.WriteLineColored("Your password has expired and must be changed.",
                    ConsoleStyle.Colors.Warning);
            }

            ConsoleManager.WriteLineColored("Changing password for " + Kernel.CurrentUser.Username,
                ConsoleStyle.Colors.Primary);

            ConsoleManager.WriteColored("Current password: ", ConsoleStyle.Colors.Primary);
            string currentPassword = ReadPasswordSecurely();

            if (!Kernel.CurrentUser.VerifyPassword(currentPassword))
            {
                ConsoleManager.WriteLineColored("Current password is incorrect.", ConsoleStyle.Colors.Error);
                return;
            }

            ConsoleManager.WriteColored("New password: ", ConsoleStyle.Colors.Primary);
            string newPassword = ReadPasswordSecurely();

            ConsoleManager.WriteColored("Confirm new password: ", ConsoleStyle.Colors.Primary);
            string confirmPassword = ReadPasswordSecurely();

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 4)
            {
                ConsoleManager.WriteLineColored("Password is too short. It must be at least 4 characters.",
                    ConsoleStyle.Colors.Error);
                return;
            }

            if (newPassword != confirmPassword)
            {
                ConsoleManager.WriteLineColored("Passwords do not match.", ConsoleStyle.Colors.Error);
                return;
            }

            if (UserManager.ChangePassword(Kernel.CurrentUser, newPassword))
            {
                ConsoleManager.WriteLineColored("Password changed successfully.", ConsoleStyle.Colors.Success);
            }
            else
            {
                ConsoleManager.WriteLineColored("Failed to change password.", ConsoleStyle.Colors.Error);
            }
        }

        private string ReadPasswordSecurely()
        {
            var password = string.Empty;
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter && !char.IsControl(key.KeyChar))
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }
    }
}