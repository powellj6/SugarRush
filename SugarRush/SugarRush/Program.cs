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
        public static string _packagesFolder = ConfigurationManager.AppSettings["packageDownloadFolder"];
        static void Main(string[] args)
        {
            try
            {
                Logc("Getting configuration...");
                var config = SugarRushHandler.GetConfiguration();

                if (!SugarRushHandler.IsValidConfig(config))
                {
                    var errors = SugarRushHandler.GetValidationErrors(config);
                    Logc("Invalid configuration: " + string.Join(", ", errors));
                    return;
                }
                
                LogConfig(config);

                Logc($"Getting nuget package: {config.packageID}.{config.packageVersion}");

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

                var assDic = GetAssemblyReferenceDic(package);

                UpdateProjFiles(projFiles, config, assDic);
                UpdateRefreshFiles(projFiles, config, assDic);
                UpdatePackageFiles(projFiles, config, assDic);
            }

            catch (Exception exc)
            {
                Logc(String.Format("Something went wrong: Message: {0}, StackTrace: {1} ", exc.Message, exc.StackTrace));
            }
        }

        private static void UpdateProjFiles(IEnumerable<FileInfo> files, SugarRushConfiguration config, Dictionary<string, string> assDic)
        {
            Parallel.ForEach<FileInfo>(files, pf =>
            {
                Console.WriteLine("Updating file: " + pf.FullName);
                var doc = SugarRushHandler.GetXmlDoc(pf.FullName);
                doc.UpdateCsProjFile(config.packageID + "." + config.packageVersion, assDic);
            });
        }

        private static void UpdateRefreshFiles(IEnumerable<FileInfo> files, SugarRushConfiguration config, Dictionary<string, string> assDic)
        {
            Parallel.ForEach<FileInfo>(files, rf =>
            {
                Console.WriteLine("Updating file: " + rf.FullName);
                rf.UpdateRefreshFile(config.packageID + "." + config.packageVersion, assDic);
            });
        }

        private static void UpdatePackageFiles(IEnumerable<FileInfo> files, SugarRushConfiguration config, Dictionary<string, string> assDic)
        {
            Parallel.ForEach<FileInfo>(files, pf => {
                Console.WriteLine("Updating file: " + pf.FullName);
                var doc = SugarRushHandler.GetXmlDoc(pf.FullName);
                doc.UpdatePackageConfig(config.packageID, config.packageVersion);
            });
        }

        private static Dictionary<string, string> GetAssemblyReferenceDic(IPackage package)
        {
            var dir = Path.GetFullPath($"{_packagesFolder}\\{package.GetFullName().Replace(' ', '.')}");

            return package.AssemblyReferences
                .Select(ar => AssemblyName.GetAssemblyName($"{dir}\\{ar.Path}"))
                .ToDictionary(k => k.Name, v => v.Version.ToString());
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
            var dirPath = ConfigurationManager.AppSettings["logFilePath"];

            var filePath = Path.Combine(dirPath, "Log.txt");

            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(dirPath);

            using (StreamWriter tw = new StreamWriter(filePath, true))
            {
                tw.WriteLine(DateTime.Now + ": " + message);
            }
        }

        private static void LogConfig(SugarRushConfiguration config)
        {
            Logc($"Config: ");

            config.GetType().GetProperties()
                .ForEach(x => {
                    var value = x.GetValue(config);

                    if (value is HashSet<string>)
                    {
                        var set = (HashSet<string>)value;

                        Logc($"  {x.Name}: ");
                        foreach (var setValue in set)
                        {
                            Logc($"    {setValue}");
                        }
                    }

                    else
                        Logc($"  {x.Name}: {value}");

                });
        }
    }
}
