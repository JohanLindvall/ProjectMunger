namespace ProjectMunger
{
    using System;
    using System.IO;
    using System.Diagnostics;
    using System.Linq;

    public static class Git
    {
        public static void Mv(string source, string destination)
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

        public static void Add(string path)
        {
            var workDir = Path.GetDirectoryName(path);
            var item = Path.GetFileName(path);
            var process = new Process { StartInfo = new ProcessStartInfo { WindowStyle = ProcessWindowStyle.Hidden, WorkingDirectory = workDir, FileName = "git.exe", Arguments = $"add \"{item}\"" } };
            process.Start();
            process.WaitForExit();
        }
    }
}
