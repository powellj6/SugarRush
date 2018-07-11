using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace SugarRush
{
    public static class SugarRushHandler
    {
        public static string UpdateCsProjFile(string file)
        {
            return "";
        }

        public static SugarRushConfiguration GetConfiguration()
        {
            var config = new SugarRushConfiguration
            {
                folderPath = ConfigurationManager.AppSettings["folderPath"],
                exclusionPaths = new HashSet<string>(ConfigurationManager.AppSettings["exclusionPaths"].Split(',')),
                packageID = ConfigurationManager.AppSettings["packageID"],
                packageVersion = ConfigurationManager.AppSettings["packageVersion"],
                nugetRepoUrl = ConfigurationManager.AppSettings["nugetRepoUrl"]
            };

            return ValidateConfiguration(ref config);
        }

        public static SugarRushConfiguration ValidateConfiguration(ref SugarRushConfiguration config)
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

        public static IEnumerable<FileInfo> FilterFiles(FileInfo[] files, HashSet<string> exclusionPaths)
        {
            return files.Where(file => !exclusionPaths.Contains(file.DirectoryName));
        }

        public static IPackage GetPackage(SugarRushConfiguration config)
        {
            return GetPackages(config).Where(p => p.Version.ToString() == config.packageVersion).FirstOrDefault();
        }

        public static IEnumerable<IPackage> GetPackages(SugarRushConfiguration config)
        {
            var repo = PackageRepositoryFactory.Default.CreateRepository(config.nugetRepoUrl);

            return repo.FindPackagesById(config.packageID);
        }

        public static FileInfo[] GetCsProjFiles(string folderPath)
        {
            return new DirectoryInfo(folderPath).GetFiles("*.csproj", SearchOption.AllDirectories);
        }
    }
}
