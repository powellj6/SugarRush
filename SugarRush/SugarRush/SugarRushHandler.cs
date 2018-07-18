using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using NuGet;

namespace SugarRush
{
    //TODO:
    //text replacement logic will break down if there are multiple versions installed. Need to "replace the text" on the spot
    public static class SugarRushHandler
    {
        public static XmlDocument UpdateCsProjFile(this XmlDocument doc, string oldPackageVersion, string newPackageVersion, 
            Dictionary<string, System.Reflection.AssemblyName> assDic)
        {
            XmlNamespaceManager xnManager = new XmlNamespaceManager(doc.NameTable);
            xnManager.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

            var referenceNodes = doc.SelectNodes("//ns:Reference", xnManager);

            foreach (XmlElement node in referenceNodes)
            {
                var includeAttribute = node.Attributes["Include"];

                if (includeAttribute != null)
                {
                    var packageID = GetPackageIdFromIncludeAttribute(includeAttribute);

                    System.Reflection.AssemblyName ass;

                    if (!assDic.TryGetValue(packageID, out ass))
                        continue;

                    var hintPath = node.GetElementsByTagName("HintPath")?[0];
                    if (hintPath == null)
                        continue;

                    includeAttribute.InnerText = ass.FullName;
                    hintPath.InnerText = hintPath.InnerText.Replace(oldPackageVersion, newPackageVersion);
                }
            }

            return doc;
        }

        public static XmlDocument UpdatePackageConfig(this XmlDocument doc, string packageID, string packageVersion)
        {
            var packageNodes = doc.SelectNodes("//package");
            
            foreach (XmlElement node in packageNodes)
            {
                var id = node.Attributes["id"]?.InnerText;

                if (id == packageID)
                {
                    var version = node.Attributes["version"];
                    version.InnerText = packageVersion;
                }
            }

            return doc;
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
            return GetFiles(folderPath, "*.csproj");
        }

        public static FileInfo[] GetFiles(string folderPath, string extension)
        {
            return new DirectoryInfo(folderPath).GetFiles(extension, SearchOption.AllDirectories);
        }

        private static string GetPackageIdFromIncludeAttribute(XmlAttribute includeAttribute)
        {
            return includeAttribute.InnerText.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)[0];
        }
    }
}
