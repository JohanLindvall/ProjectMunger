using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace ProjectMunger
{
    public static class CheckNuget
    {
        public static void Check(string directory)
        {
            var checkedCount = 0;
            foreach (var f in Directory.GetFiles(directory, "packages.config", SearchOption.AllDirectories))
            {
                var nugetPackages = new Dictionary<string, string>();
                var doc = XDocument.Load(f);
                foreach (var package in doc.Root.Elements())
                {
                    nugetPackages.Add(package.Attribute("id").Value, package.Attribute("version").Value);
                }

                var projectFiles = Directory.GetFiles(Path.GetDirectoryName(f), "*.csproj").ToList();

                if (projectFiles.Count == 1)
                {
                    var proj = XDocument.Load(projectFiles.First());
                    var references = from d in proj.Descendants()
                                     where d.Descendants()
                                            .Any(o => o.Parent == d && o.Name.LocalName == "HintPath")
                                     select d;

                    foreach (var reference in references)
                    {
                        var include = reference.Attribute("Include");
                        var hintpath = (from d in reference.Descendants()
                                        where d.Name.LocalName == "HintPath"
                                        select d).Single();
                        var re = new Regex(@"^(..\\)*packages\\(?<packageandversion>[A-Za-z\.0-9]+)\\.*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
                        var hintpathStr = hintpath.Value;
                        var matches = re.Matches(hintpathStr);
                        if (matches.Count == 1)
                        {
                            var packageAndVersion = matches[0].Groups["packageandversion"].Value;
                            var versionRe = new Regex(@"((\.[0-9]*))*$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
                            var m2 = versionRe.Matches(packageAndVersion);
                            var actualVersion = m2[0].Groups[0].Value.TrimStart('.');
                            var package = packageAndVersion.Substring(0, packageAndVersion.Length - actualVersion.Length).TrimEnd('.');
                            string specifiedVersion;
                            if (nugetPackages.TryGetValue(package, out specifiedVersion))
                            {
                                if (actualVersion != specifiedVersion)
                                {
                                    Console.WriteLine($"Project '{projectFiles.First()}' package '{package}' version '{actualVersion}' should be '{specifiedVersion}.");
                                }
                                ++checkedCount;
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Unparsed hint path '{hintpathStr}'.");
                        }
                    }
                }
            }

            Console.WriteLine($"Checked {checkedCount} NuGet references.");
        }
    }
}
