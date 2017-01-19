using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using OpenRasta.Collections;
using OpenRasta.Hosting.AspNet.Tests.Integration;
using OpenRasta.IO;

public class FileCopySetup
{
    public static DirectoryInfo TempFolder;
    [OneTimeSetUp]
    public void CreateFolder()
    {
        TempFolder = PrepareFolderStructure();
    }

    static DirectoryInfo PrepareFolderStructure()
    {
        var assembly = typeof(aspnet_server_context).Assembly;
        var localPath = new Uri(assembly.CodeBase).LocalPath;
        var rootFolder = Path.GetDirectoryName(localPath);

        var tempFolder = CreateTempFolder();

        var filesToCopy = Directory.GetFiles(rootFolder);
        foreach (var file in filesToCopy)
        {
            var source = file;
            var destination = Path.Combine(Path.Combine(tempFolder.FullName, "bin"), Path.GetFileName(source));

            Console.WriteLine("Copying " + file);
            File.Copy(source, destination);
        }
        using (var webConfig = assembly.GetManifestResourceStream("OpenRasta.Hosting.AspNet.Tests.Integration.Web.config"))
        {
            var content = webConfig.ReadToEnd();

            File.WriteAllBytes(Path.Combine(tempFolder.FullName, "web.config"), content);
        }

        return tempFolder;
    }
    static DirectoryInfo CreateTempFolder()
    {
        var tempFolder = Path.GetTempPath();
        string createdFolder;
        var count = 0;
        do
        {
            createdFolder = Path.Combine(tempFolder, "_ORTEST_" + count++);
        } while (Directory.Exists(createdFolder));
        Console.WriteLine("Creating {0}", createdFolder);
        var tempRoot = Directory.CreateDirectory(createdFolder);
        Console.WriteLine("Creating " + Directory.CreateDirectory(Path.Combine(createdFolder, "bin")).FullName);

        return tempRoot;
    }
    [OneTimeTearDown]
    public void DeleteFiles()
    {
        try
        {
            TempFolder.GetDirectories()
                .SelectMany(subfolders => subfolders.GetFiles())
                .Concat(TempFolder.GetFiles())
                .ForEach(f =>
                {
                    try
                    {
                        f.Delete();
                    }
                    catch
                    {
                    }
                });

            TempFolder.Delete(true);
        }
        catch
        {
        }
    }
}
