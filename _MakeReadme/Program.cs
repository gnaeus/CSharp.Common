using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace _MakeReadme
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());

            DirectoryInfo projectDir = currentDir.Parent.Parent.Parent;

            Directory.SetCurrentDirectory(projectDir.FullName);

            var sb = new StringBuilder();

            foreach (DirectoryInfo nestedDir in projectDir.GetDirectories())
            {
                PopulateReadme(sb, projectDir.FullName, nestedDir);
            }

            File.WriteAllText("README.md", sb.ToString(), Encoding.UTF8);
        }

        static void PopulateReadme(StringBuilder sb, string projectRoot, DirectoryInfo directory)
        {
            foreach (DirectoryInfo nestedDir in directory.GetDirectories())
            {
                PopulateReadme(sb, projectRoot, nestedDir);
            }

            foreach (FileInfo file in directory.GetFiles())
            {
                if (file.Name.ToLower() == "readme.md")
                {
                    string relativePath = directory.FullName
                        .Replace(projectRoot, "")
                        .Replace(Path.DirectorySeparatorChar, '/');

                    string link = "." + relativePath;

                    string name = relativePath
                        .Substring(1, relativePath.Length - 1)
                        .Replace('/', '.');

                    sb.AppendLine($@"
<details id=""{name}"">
    <summary style=""font-size: 1.8em"">{name} <a href=""{link}"">[docs]</a></summary>
");
                    sb.Append(File.ReadAllText(file.FullName));
                    sb.AppendLine("</details>");
                }
            }
        }
    }
}
