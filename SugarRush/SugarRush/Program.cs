using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using NuGet;

namespace SugarRush
{
    class Program
    {
        static void Main(string[] args)
        {
            //run through .csproj files for the HintPaths
            //run through /packages.config files for the package 
            //run through .dll.refresh files
            //Logger

            try
            {
                var config = GetConfiguration();

                if (config.IsInvalid)
                {
                    Console.WriteLine("Invalid configuration: " + config.Message);
                    return;
                }

                var projFiles = new DirectoryInfo(config.folderPath).GetFiles("*.csproj", SearchOption.AllDirectories);

                var filteredProjFiles = FilterFiles(projFiles, config.exclusionPaths).ToList();

                var package = GetPackage(config);

                if (package == null)
                {
                    Console.WriteLine("Could not find any packages by packageID: " + config.packageID);
                    return;
                }

                var assReferences = package.AssemblyReferences.Select(x => (PhysicalPackageAssemblyReference)x);

                //var path = assReference.SourcePath;

                //var name = System.Reflection.AssemblyName.GetAssemblyName(path);
            }
            catch (Exception exc)
            {
                Logc(String.Format("Something went wrong: Message: {0}, StackTrace: {1} ", exc.Message, exc.StackTrace));
            }
        }

        private static IPackage GetPackage(SugarRushConfiguration config)
        {
            return GetPackages(config).Where(p => p.Version.ToString() == config.packageVersion).FirstOrDefault();
        }

        private static IEnumerable<IPackage> GetPackages(SugarRushConfiguration config)
        {
            var repo = PackageRepositoryFactory.Default.CreateRepository(config.nugetRepoUrl);

            return repo.FindPackagesById(config.packageID);
        }

        private static IEnumerable<FileInfo> FilterFiles(FileInfo[] files, HashSet<string> exclusionPaths)
        {
            return files.Where(file => !exclusionPaths.Contains(file.DirectoryName));
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

        private static SugarRushConfiguration GetConfiguration()
        {
            var config = new SugarRushConfiguration {
                folderPath = ConfigurationManager.AppSettings["folderPath"],
                exclusionPaths = new HashSet<string>(ConfigurationManager.AppSettings["exclusionPaths"].Split(',')),
                packageID = ConfigurationManager.AppSettings["packageID"],
                packageVersion = ConfigurationManager.AppSettings["packageVersion"],
                nugetRepoUrl = ConfigurationManager.AppSettings["nugetRepoUrl"]
            };

            return ValidateConfiguration(ref config);
        }

        private static SugarRushConfiguration ValidateConfiguration(ref SugarRushConfiguration config)
        {
            var errorMessages = new List<string>();

            if (config.folderPath.IsEmpty())
                errorMessages.Add("Missing folderPath");

            if (config.packageID.IsEmpty())
                errorMessages.Add("Missing packageID");

            if (config.packageVersion.IsEmpty())
                errorMessages.Add("Missing packageVersion");

            if (config.nugetRepoUrl.IsEmpty())
                errorMessages.Add("Missing nugetRepoUrl");

            if (!errorMessages.IsEmpty())
            {
                config.IsInvalid = true;
                config.Message = string.Join(",", errorMessages);
            }

            return config;
        }

        public class SugarRushConfiguration
        {
            public string folderPath { get; set; }
            public HashSet<string> exclusionPaths { get; set; }
            public string packageID { get; set; }
            public string packageVersion { get; set; }
            public string nugetRepoUrl { get; set; }
            public bool IsInvalid { get; set; }
            public string Message { get; set; }
        }
    }
}
