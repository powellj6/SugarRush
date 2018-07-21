﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using NuGet;

namespace SugarRush
{
    public static class SugarRushHandler
    {
        public static XmlDocument UpdateCsProjFile(this XmlDocument doc, string newPackageWithVersion, 
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
                    var currentPackageID = GetPackageIdFromIncludeAttribute(includeAttribute);

                    System.Reflection.AssemblyName ass;

                    if (!assDic.TryGetValue(currentPackageID, out ass))
                        continue;

                    var hintPath = node.GetElementsByTagName("HintPath")?[0];
                    if (hintPath == null)
                        continue;

                    var oldPackageWithVersion = GetPackageWithVersionFromHintPath(hintPath.InnerText);

                    includeAttribute.InnerText = ass.FullName;
                    hintPath.InnerText = hintPath.InnerText.Replace(oldPackageWithVersion, newPackageWithVersion);
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

        public static FileInfo UpdateRefreshFile(this FileInfo file, string newPackageWithVersion, Dictionary<string, System.Reflection.AssemblyName> assDic)
        {
            string text = File.ReadAllText(file.FullName);

            var dllName = GetDllNamefromRefreshFile(text);

            if (assDic.ContainsKey(dllName))
            {
                var oldPackageWithVersion = GetPackageWithVersionFromHintPath(text);

                var updatedText = text.Replace(oldPackageWithVersion, newPackageWithVersion);

                File.WriteAllText(file.FullName, updatedText);
            }

            return file;
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

        private static string GetPackageWithVersionFromHintPath(string hintPath)
        {
            return Regex.Match(hintPath, @"\\packages\\(?<packageWithVersion>.*?)\\lib").Groups["packageWithVersion"].Value;
        }

        private static string GetDllNamefromRefreshFile(string text)
        {
            return Regex.Match(text, @"\\(?<packageID>.*?)\.dll", RegexOptions.RightToLeft)
                .Groups["packageID"].Value;
        }
    }
}
