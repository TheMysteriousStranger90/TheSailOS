using System;
using TheSailOSProject.Network;
using TheSailOSProject.Styles;

namespace TheSailOSProject.Commands.Network
{
    public class NetworkTestCommand : ICommand
    {
        public string Name => "nettest";
        public string Description => "Test network functionality";

        public void Execute(string[] args)
        {
            if (args.Length < 1)
            {
                ShowUsage();
                return;
            }

            string action = args[0].ToLower();

            switch (action)
            {
                case "init":
                    TestInitialization();
                    break;

                case "dns":
                    if (args.Length < 2)
                    {
                        ConsoleManager.WriteLineColored("Usage: nettest dns <hostname>", 
                            ConsoleStyle.Colors.Error);
                        return;
                    }
                    TestDns(args[1]);
                    break;

                case "ping":
                    if (args.Length < 2)
                    {
                        ConsoleManager.WriteLineColored("Usage: nettest ping <host>", 
                            ConsoleStyle.Colors.Error);
                        return;
                    }
                    TestPing(args[1]);
                    break;

                case "http":
                    if (args.Length < 2)
                    {
                        ConsoleManager.WriteLineColored("Usage: nettest http <url>", 
                            ConsoleStyle.Colors.Error);
                        return;
                    }
                    TestHttp(args[1]);
                    break;

                case "status":
                    NetworkManager.ShowStatus();
                    break;

                default:
                    ShowUsage();
                    break;
            }
        }

        private void TestInitialization()
        {
            ConsoleManager.WriteLineColored("=== Network Initialization Test ===", 
                ConsoleStyle.Colors.Primary);
            
            bool success = NetworkManager.Initialize();
            
            if (success)
            {
                ConsoleManager.WriteLineColored("Network initialized successfully!", 
                    ConsoleStyle.Colors.Success);
                NetworkManager.ShowStatus();
            }
            else
            {
                ConsoleManager.WriteLineColored("Network initialization failed!", 
                    ConsoleStyle.Colors.Error);
            }
        }

        private void TestDns(string hostname)
        {
            ConsoleManager.WriteLineColored($"=== DNS Resolution Test for '{hostname}' ===", 
                ConsoleStyle.Colors.Primary);

            var address = DnsResolver.GetAddress(hostname);

            if (address != null)
            {
                ConsoleManager.WriteColored("Resolved to: ", ConsoleStyle.Colors.Success);
                ConsoleManager.WriteLineColored(address.ToString(), ConsoleStyle.Colors.Accent);
            }
            else
            {
                ConsoleManager.WriteLineColored("DNS resolution failed!", 
                    ConsoleStyle.Colors.Error);
            }
        }

        private void TestPing(string host)
        {
            ConsoleManager.WriteLineColored($"=== Ping Test for '{host}' ===", 
                ConsoleStyle.Colors.Primary);

            var pingClient = new PingClient();
            bool success = pingClient.Ping(host, 4);

            if (success)
            {
                ConsoleManager.WriteLineColored("\nPing test completed successfully!", 
                    ConsoleStyle.Colors.Success);
            }
            else
            {
                ConsoleManager.WriteLineColored("\nPing test failed!", 
                    ConsoleStyle.Colors.Error);
            }
        }

        private void TestHttp(string url)
        {
            ConsoleManager.WriteLineColored($"=== HTTP Download Test ===", 
                ConsoleStyle.Colors.Primary);

            try
            {
                string content = HttpClient.DownloadString(url);
                
                ConsoleManager.WriteLineColored("Download successful!", ConsoleStyle.Colors.Success);
                ConsoleManager.WriteColored("Content length: ", ConsoleStyle.Colors.Primary);
                ConsoleManager.WriteLineColored($"{content.Length} bytes", ConsoleStyle.Colors.Number);
                
                Console.WriteLine("\nFirst 500 characters:");
                Console.WriteLine(new string('=', 70));
                
                if (content.Length > 500)
                {
                    Console.WriteLine(content.Substring(0, 500) + "...");
                }
                else
                {
                    Console.WriteLine(content);
                }
                
                Console.WriteLine(new string('=', 70));
            }
            catch (Exception ex)
            {
                ConsoleManager.WriteLineColored($"HTTP download failed: {ex.Message}", 
                    ConsoleStyle.Colors.Error);
            }
        }

        private void ShowUsage()
        {
            ConsoleManager.WriteLineColored("Network Test Command", ConsoleStyle.Colors.Primary);
            Console.WriteLine("\nUsage:");
            Console.WriteLine("  nettest init                - Initialize network");
            Console.WriteLine("  nettest status              - Show network status");
            Console.WriteLine("  nettest dns <hostname>      - Test DNS resolution");
            Console.WriteLine("  nettest ping <host>         - Test ping");
            Console.WriteLine("  nettest http <url>          - Test HTTP download");
            Console.WriteLine("\nExamples:");
            Console.WriteLine("  nettest init");
            Console.WriteLine("  nettest dns google.com");
            Console.WriteLine("  nettest ping 8.8.8.8");
            Console.WriteLine("  nettest http http://example.com");
        }
    }
}