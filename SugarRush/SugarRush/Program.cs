using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NuGet;

namespace SugarRush
{
    class Program
    {
        //TODO: Don't update/save file if nothing has changed
        public static string _packagesFolder = @"C:\Temp\NugetPackages\";
        static void Main(string[] args)
        {
            try
            {
                Logc("Getting configuration...");
                var config = SugarRushHandler.GetConfiguration();

                if (config.IsInvalid)
                {
                    Logc("Invalid configuration: " + config.Message);
                    return;
                }

                Logc("Getting nuget package...");
                var package = SugarRushHandler.GetPackage(config);

                if (package == null)
                {
                    Logc("Could not find any nuget package by packageID: " + config.packageID);
                    return;
                }

                Logc("Getting csproj files...");
                var projFiles = SugarRushHandler.FilterFiles(SugarRushHandler.GetCsProjFiles(config.folderPath), config.exclusionPaths);
                Logc("Getting Refresh files...");
                var refreshFiles = SugarRushHandler.FilterFiles(SugarRushHandler.GetRefreshFiles(config.folderPath), config.exclusionPaths);
                Logc("Getting package.config files...");
                var packageFiles = SugarRushHandler.FilterFiles(SugarRushHandler.GetPackageFiles(config.folderPath), config.exclusionPaths);

                DownloadNugetPackage(package, config.nugetRepoUrl);

                var dir = Path.GetFullPath($"{_packagesFolder}\\{package.GetFullName().Replace(' ', '.')}");

                //TODO: Not really using anything in the assembly besides "FullName".
                //Can probably just have Dictionary<dll name, FullName>
                var assDic = package.AssemblyReferences
                    .Select(ar => AssemblyName.GetAssemblyName($"{dir}\\{ar.Path}"))
                    .ToDictionary(k => k.Name, v => v);

                Parallel.ForEach<FileInfo>(projFiles, pf =>
                {
                    Console.WriteLine("Updating file: " + pf.FullName);
                    var doc = SugarRushHandler.GetXmlDoc(pf.FullName);
                    doc.UpdateCsProjFile(config.packageID + "." + config.packageVersion, assDic);
                });

                Parallel.ForEach<FileInfo>(refreshFiles, rf =>
                {
                    Console.WriteLine("Updating file: " + rf.FullName);
                    rf.UpdateRefreshFile(config.packageID + "." + config.packageVersion, assDic);
                });

                Parallel.ForEach<FileInfo>(packageFiles, pf => {
                    Console.WriteLine("Updating file: " + pf.FullName);
                    var doc = SugarRushHandler.GetXmlDoc(pf.FullName);
                    doc.UpdatePackageConfig(config.packageID, config.packageVersion);
                });
            }

            catch (Exception exc)
            {
                Logc(String.Format("Something went wrong: Message: {0}, StackTrace: {1} ", exc.Message, exc.StackTrace));
            }
        }

        private static void DownloadNugetPackage(IPackage package, string repoUrl)
        {
            IPackageRepository packageRepository = PackageRepositoryFactory.Default.CreateRepository(repoUrl);
            PackageManager packageManager = new PackageManager(packageRepository, _packagesFolder);
            

            packageManager.InstallPackage(package, true, true, true);
        }

        private static void Logc(string message)
        {
            Console.WriteLine(message);
            Log(message);
        }

        private static void Log(string message)
        {
            var dPath = ConfigurationManager.AppSettings["logFilePath"];

            var path = Path.Combine(dPath, "Log.txt");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(dPath);

            using (StreamWriter tw = new StreamWriter(path, true))
            {
                tw.WriteLine(DateTime.Now + ": " + message);
            }
        }
    }
}
