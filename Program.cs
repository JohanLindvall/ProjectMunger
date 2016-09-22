using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace ProjectMunger
{
    class Program
    {
        static void Main(string[] args)
        {
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
