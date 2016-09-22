using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ProjectMunger
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        private static void RenameProject(string slnPath, string projectPath, string newName)
        {
            var oldName = Path.GetFileName(projectPath);

            var slnData = File.ReadAllText(slnPath);
            var newData = slnData.Replace(oldName, newName);
            File.WriteAllText(slnPath, newData);
            var dest = Path.Combine(Path.GetDirectoryName(projectPath), newName);
            GitMv(projectPath, dest);

            foreach (var fi in Directory.GetFiles(dest, "*", SearchOption.AllDirectories))
            {
                var actualFile = Path.Combine(Path.GetDirectoryName(fi), fi.Replace(Path.GetFileName(projectPath), newName));
                if (!string.Equals(actualFile, fi, StringComparison.OrdinalIgnoreCase))
                {
                    GitMv(fi, actualFile);
                }

                var data = File.ReadAllText(actualFile);
                newData = data.Replace(Path.GetFileName(projectPath), newName);
                if (!string.Equals(data, newData))
                {
                    File.WriteAllText(actualFile, newData);
                }
            }
        }

        private static void GitMv(string source, string destination)
        {
            var s = Path.GetFullPath(source).Split('\\');
            var d = Path.GetFullPath(destination).Split('\\');
            int i;
            var end = Math.Min(s.Length, d.Length);
            for (i = 0; i < end; ++i)
            {
                if (!string.Equals(s[i], d[i], StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }

            var workDir = string.Join("\\", s.Take(i).ToArray());
            source = string.Join("\\", s.Skip(i).ToArray());
            destination = string.Join("\\", d.Skip(i).ToArray());

            var process = new Process { StartInfo = new ProcessStartInfo { WindowStyle = ProcessWindowStyle.Hidden, WorkingDirectory = workDir, FileName = "git.exe", Arguments = $"mv \"{source}\" \"{destination}\"" } };
            process.Start();
            process.WaitForExit();

        }

        private static void FixupDocumentationPath(string path)
        {
            foreach (var fi in Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories))
            {
                var data = File.ReadAllText(fi);
                var changed = Regex.Replace(data, @"\<DocumentationFile\>.+\<\/DocumentationFile\>", @"<DocumentationFile>$(MSBuildProjectDirectory)\$(OutputPath)$(MSBuildProjectName).xml</DocumentationFile>");
                if (!string.Equals(data, changed, System.StringComparison.OrdinalIgnoreCase))
                {
                    File.WriteAllText(fi, changed);
                }
            }
        }

        private static void CloneProject(string source, IList<string> destinations)
        {
            var projectName = Path.GetFileName(source);

            foreach (var dir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                foreach (var newProjectName in destinations)
                {
                    var newDest = dir.Replace(projectName, newProjectName);

                    Directory.CreateDirectory(newDest);
                }
            }

            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                var ext = Path.GetExtension(file);

                if (ext != ".cs" && ext != ".csproj" && ext != ".config" && ext != ".settings")
                {
                    continue;
                }

                var data = File.ReadAllText(file);

                foreach (var newProjectName in destinations)
                {
                    var newDest = file.Replace(projectName, newProjectName);
                    var newData = data.Replace(projectName, newProjectName);
                    if (file.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
                    {
                        newData = Regex.Replace(newData, @"\<ProjectGuid\>.+\<\/ProjectGuid\>", $@"<ProjectGuid>{Guid.NewGuid().ToString("B").ToUpper()}</ProjectGuid>");
                    }
                    File.WriteAllText(newDest, newData);
                }
            }
        }
    }
}
