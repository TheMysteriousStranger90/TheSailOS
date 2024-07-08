using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TheSailOS.Configuration;

public class TheSailOSCfg
{
    private static string _path = @"0:\thesail.cfg";
    private static string NameOs { get; set; } = "TheSailOS";
    private static DateTime InstallDate { get; set; } = DateTime.Now;
    public static bool BootLock { get; set; } = false;
    private static string InstallVer { get; set; } = Kernel.VersionOs;

    internal static void Load()
    {
        try
        {
            if (!File.Exists(_path))
            {
                Flush();
                return;
            }

            var lines = File.ReadAllLines(_path);
            var sectionValues = ParseIniLines(lines);

            NameOs = sectionValues["SysMeta"]["NameOs"];
            InstallDate = new DateTime(long.Parse(sectionValues["SysMeta"]["InstallDate"]));
            InstallVer = sectionValues["SysMeta"]["InstallVer"];
            BootLock = bool.Parse(sectionValues["Config"]["BootLock"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading configuration: {ex.Message}");
        }
    }

    private static void Flush()
    {
        var builder = new StringBuilder();
        AppendSection(builder, "SysMeta", new Dictionary<string, string>
        {
            ["NameOs"] = NameOs,
            ["InstallDate"] = InstallDate.Ticks.ToString(),
            ["InstallVer"] = InstallVer
        });
        AppendSection(builder, "Config", new Dictionary<string, string>
        {
            ["BootLock"] = BootLock.ToString()
        });

        File.WriteAllText(_path, builder.ToString());
    }

    private static void AppendSection(StringBuilder builder, string sectionName, Dictionary<string, string> values)
    {
        builder.AppendLine($"[{sectionName}]");
        foreach (var kvp in values)
        {
            builder.AppendLine($"{kvp.Key}={kvp.Value}");
        }
    }

    private static Dictionary<string, Dictionary<string, string>> ParseIniLines(string[] lines)
    {
        var result = new Dictionary<string, Dictionary<string, string>>();
        Dictionary<string, string> currentSection = null;

        foreach (var line in lines)
        {
            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                var sectionName = line[1..^1];
                currentSection = new Dictionary<string, string>();
                result[sectionName] = currentSection;
            }
            else if (currentSection != null)
            {
                var kvp = line.Split(new[] { '=' }, 2);
                if (kvp.Length == 2)
                {
                    currentSection[kvp[0].Trim()] = kvp[1].Trim();
                }
            }
        }

        return result;
    }
}