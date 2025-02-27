using System;
using TheSailOSProject.Styles;
using TheSailOSProject.Users;

namespace TheSailOSProject.Commands.Users
{
    public class LoginCommand : ICommand
    {
        private readonly ILoginHandler _loginHandler;

        public LoginCommand(ILoginHandler loginHandler)
        {
            _loginHandler = loginHandler;
        }

        public void Execute(string[] args)
        {
            if (args.Length < 2)
            {
                ConsoleManager.WriteLineColored("Usage: login <username> <password>", ConsoleStyle.Colors.Error);
                return;
            }

            string username = args[0];
            string password = args[1];

            if (UserManager.VerifyLogin(username, password, out User user))
            {
                if (user.PasswordExpired)
                {
                    ConsoleManager.WriteLineColored("Your password has expired and must be changed.",
                        ConsoleStyle.Colors.Warning);

                    _loginHandler.OnLoginSuccess(user);

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

                    if (UserManager.ChangePassword(user, newPassword))
                    {
                        ConsoleManager.WriteLineColored("Password changed successfully.", ConsoleStyle.Colors.Success);
                    }
                    else
                    {
                        ConsoleManager.WriteLineColored("Failed to change password.", ConsoleStyle.Colors.Error);
                    }
                }
                else
                {
                    _loginHandler.OnLoginSuccess(user);
                }
            }
            else
            {
                ConsoleManager.WriteLineColored("Invalid username or password.", ConsoleStyle.Colors.Error);
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