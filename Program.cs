using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class FileName
{
    private static readonly Regex VersionRegex = new Regex(
        @"^(.*?)\s*\(\s*v\s*(\d+)\s*\)\s*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string OriginalName { get; }
    public string BaseName { get; }
    public int? Version { get; }
    public string FinalName { get; private set; }

    public FileName(string name)
    {
        OriginalName = name.Trim();
        var match = VersionRegex.Match(OriginalName);

        if (match.Success)
        {
            BaseName = match.Groups[1].Value.Trim();
            Version = int.Parse(match.Groups[2].Value);
        }
        else
        {
            BaseName = OriginalName;
            Version = null;
        }

        FinalName = OriginalName;
    }

    public void AssignNextVersion(IEnumerable<string> existingNames)
    {
        var existing = existingNames.Select(n => n.ToLower()).ToHashSet();
        var currentVersion = Version ?? 0;
        var candidate = OriginalName;

        if (!existing.Contains(candidate.ToLower()))
        {
            FinalName = candidate;
            return;
        }

        var highestVersion = existing
            .Select(n => VersionRegex.Match(n))
            .Where(m => m.Success)
            .Where(m => string.Equals(m.Groups[1].Value.Trim(), BaseName, StringComparison.OrdinalIgnoreCase))
            .Select(m => int.Parse(m.Groups[2].Value))
            .DefaultIfEmpty(0)
            .Max();

        currentVersion = Math.Max(currentVersion, highestVersion);

        do
        {
            currentVersion++;
            candidate = $"{BaseName} (v{currentVersion})";
        } while (existing.Contains(candidate.ToLower()));

        FinalName = candidate;
    }
}

public class FileNameProcessor
{
    public static List<string> ProcessNames(List<string> originalNames, List<string> newNames)
    {
        var allNames = new List<string>(originalNames);
        var result = new List<string>();

        foreach (var name in newNames)
        {
            var fileName = new FileName(name);
            fileName.AssignNextVersion(allNames);
            result.Add(fileName.FinalName);
            allNames.Add(fileName.FinalName);
        }

        return result;
    }
}

namespace File_Name_Versioning
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter existing file names (comma-separated):");
            var original = Console.ReadLine()?
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(n => n.Trim())
                .ToList() ?? new List<string>();

            Console.WriteLine("Enter new file names (comma-separated):");
            var incoming = Console.ReadLine()?
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(n => n.Trim())
                .ToList() ?? new List<string>();

            var result = FileNameProcessor.ProcessNames(original, incoming);

            Console.WriteLine("\nProcessed names:");
            foreach (var name in result)
            {
                Console.WriteLine(name);
            }
        }
    }
}
