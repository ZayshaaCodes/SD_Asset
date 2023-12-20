using System;
using System.IO;
using System.Text;

public static class DirectorySummarizer
{
    public static string SummarizeDirectory(string directoryPath)
    {
        StringBuilder summary = new StringBuilder();
        BuildSummary(directoryPath, "", summary);
        return summary.ToString();
    }

    private static void BuildSummary(string directoryPath, string indent, StringBuilder summary)
    {
        // Append the current directory.
        summary.AppendLine($"{indent}D: {Path.GetFileName(directoryPath)}");

        // Get the subdirectories for the specified directory.
        string[] subdirectories = Directory.GetDirectories(directoryPath);

        foreach (string subdirectory in subdirectories)
        {
            // Recurse into subdirectories.
            BuildSummary(subdirectory, indent + "  ", summary);
        }

        // Get the files in the current directory. exclude .meta files
        string[] files = Directory.GetFiles(directoryPath);
        files = Array.FindAll(files, s => !s.EndsWith(".meta"));

        foreach (string file in files)
        {
            // Append the files indented under their parent directory.
            summary.AppendLine($"{indent}  F: {Path.GetFileName(file)}");
        }
    }
}