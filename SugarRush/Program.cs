using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NuGet;

namespace SugarRush
{
    class Program
    {
        private static string _packagesFolder = ConfigurationManager.AppSettings["packageDownloadFolder"];
        private static Action<string> _log = Logc();
        static void Main(string[] args)
        {
            try
            {
                var sw = new Stopwatch();
                sw.Start();

                _log("Getting configuration...");
                var config = SugarRushHandler.GetConfiguration();

                if (!SugarRushHandler.IsValidConfig(config))
                {
                    var errors = SugarRushHandler.GetValidationErrors(config);
                    _log("Invalid configuration: " + string.Join(", ", errors));
                    return;
                }
                
                LogConfig(config);

                _log($"Getting nuget package: {config.packageID}.{config.packageVersion}");
                var package = SugarRushHandler.GetPackage(config);

                if (package == null)
                {
                    _log($"Could not find nuget package \"{config.packageID}\" by version \"{config.packageVersion}\"" + config.packageID);
                    return;
                }

                _log("Getting csproj files...");
                var projFiles = SugarRushHandler.FilterFiles(SugarRushHandler.GetCsProjFiles(config.folderPath), config.exclusionPaths);

                _log("Getting Refresh files...");
                var refreshFiles = SugarRushHandler.FilterFiles(SugarRushHandler.GetRefreshFiles(config.folderPath), config.exclusionPaths);

                _log("Getting package.config files...");
                var packageFiles = SugarRushHandler.FilterFiles(SugarRushHandler.GetPackageFiles(config.folderPath), config.exclusionPaths);

                _log("Downloading nuget package locally...");
                DownloadNugetPackage(package, config.nugetRepoUrl);

                var assDic = GetAssemblyReferenceDic(package);

                _log("Updating files...");
                UpdateProjFiles(projFiles, config, assDic);
                UpdateRefreshFiles(refreshFiles, config, assDic);
                UpdatePackageFiles(packageFiles, config, assDic);

                sw.Stop();
                _log("Finished updating");
                _log("Ellapsed time: " + sw.Elapsed);

            }

            catch (Exception exc)
            {
                _log(String.Format($"Something went wrong: {exc.Message}"));
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

        private static Action<string> Logc()
        {
            var dirPath = ConfigurationManager.AppSettings["logFilePath"];

            var filePath = Path.Combine(dirPath, "Log.txt");

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            Action<string> act = delegate (string message) {
                using (StreamWriter tw = new StreamWriter(filePath, true))
                {
                    tw.WriteLine(DateTime.Now + ": " + message);
                }
                Console.WriteLine(message);
            };

            return act;
        }

        private static void LogConfig(SugarRushConfiguration config)
        {
            _log($"Config: ");

            config.GetType().GetProperties()
                .ForEach(x => {
                    var value = x.GetValue(config);

                    if (value is HashSet<string>)
                    {
                        var set = (HashSet<string>)value;

                        _log($"  {x.Name}: ");
                        foreach (var setValue in set)
                        {
                            _log($"    {setValue}");
                        }
                    }

                    else
                        _log($"  {x.Name}: {value}");

                });
        }
    }
}
