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

            var body = new StringBuilder();
            var menu = new StringBuilder();

            foreach (DirectoryInfo nestedDir in projectDir.GetDirectories())
            {
                PopulateReadme(body, menu, projectDir.FullName, nestedDir);
            }

            var document = new StringBuilder();

            document.Append(menu.ToString());
            document.AppendLine();
            document.AppendLine("<hr />");
            document.AppendLine(body.ToString());

            File.WriteAllText("README.md", document.ToString(), Encoding.UTF8);
        }

        static void PopulateReadme(
            StringBuilder body, StringBuilder menu, string projectRoot, DirectoryInfo directory)
        {
            foreach (DirectoryInfo nestedDir in directory.GetDirectories())
            {
                PopulateReadme(body, menu, projectRoot, nestedDir);
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

                    menu.AppendLine($" * [{name}](#link-{name})");

                    body.AppendLine();
                    body.AppendLine($"## <a name=\"link-{name}\"></a>[{name}]({link})");
                    body.AppendLine();
                    body.Append(File.ReadAllText(file.FullName));
                }
            }
        }
    }
}
