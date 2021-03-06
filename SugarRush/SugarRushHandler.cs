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
        public static void UpdateCsProjFile(this XmlDocument doc, string newPackageWithVersion, 
            Dictionary<string, string> assDic, bool skipUpdate = false)
        {
            var shouldUpdate = false;

            XmlNamespaceManager xnManager = new XmlNamespaceManager(doc.NameTable);
            xnManager.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003");

            var referenceNodes = doc.SelectNodes("//ns:Reference", xnManager);

            foreach (XmlElement node in referenceNodes)
            {
                var includeAttribute = node.Attributes["Include"];

                if (includeAttribute != null)
                {
                    var currentPackageID = GetPackageIdFromIncludeAttribute(includeAttribute);

                    string assVersion;

                    if (!assDic.TryGetValue(currentPackageID, out assVersion))
                        continue;

                    var hintPath = node.GetElementsByTagName("HintPath")?[0];
                    if (hintPath == null)
                        continue;

                    if (includeAttribute.InnerText.Contains("Version="))
                    {
                        var version = GetVersionFromIncludeNode(includeAttribute.InnerText);
                        includeAttribute.InnerText = includeAttribute.InnerText.Replace(version, assVersion);
                        shouldUpdate = true;
                    }

                    var oldPackageWithVersion = GetPackageWithVersionFromHintPath(hintPath.InnerText);

                    if (!oldPackageWithVersion.IsEmpty())
                    {
                        hintPath.InnerText = hintPath.InnerText.Replace(oldPackageWithVersion, newPackageWithVersion);
                        shouldUpdate = true;
                    }
                }
            }

            if (shouldUpdate && !skipUpdate)
                doc.Save(GetFilePathFromBaseUri(doc.BaseURI));
        }

        public static void UpdatePackageConfig(this XmlDocument doc, string packageID, string packageVersion, bool skipUpdate = false)
        {
            var shouldUpdate = false;

            var packageNodes = doc.SelectNodes("//package");

            foreach (XmlElement node in packageNodes)
            {
                var id = node.Attributes["id"]?.InnerText;

                if (id == packageID)
                {
                    var version = node.Attributes["version"];
                    if (version.InnerText != packageVersion)
                    {
                        version.InnerText = packageVersion;
                        shouldUpdate = true;
                    }
                }
            }

            if (shouldUpdate && !skipUpdate)
                doc.Save(GetFilePathFromBaseUri(doc.BaseURI));
        }

        public static void UpdateRefreshFile(this FileInfo file, string newPackageWithVersion, Dictionary<string, string> assDic)
        {
            string text = File.ReadAllText(file.FullName);

            var dllName = GetDllNamefromRefreshFile(text);

            if (assDic.ContainsKey(dllName))
            {
                var oldPackageWithVersion = GetPackageWithVersionFromHintPath(text);

                if (oldPackageWithVersion != newPackageWithVersion)
                {
                    var updatedText = text.Replace(oldPackageWithVersion, newPackageWithVersion);

                    //TODO: Override/Fix this so the referenced file we're testing with doesn't get written to, screwing future tests
                    File.WriteAllText(file.FullName, updatedText);
                }
            }
        }

        public static bool IsValidConfig(SugarRushConfiguration config)
        {
            if (config.folderPath.IsEmpty())
                return false;

            if (config.packageID.IsEmpty())
                return false;

            if (config.packageVersion.IsEmpty())
                return false;

            if (config.nugetRepoUrl.IsEmpty())
                return false;

            return true;
        }

        public static SugarRushConfiguration GetConfiguration()
        {
            return new SugarRushConfiguration
            {
                folderPath = ConfigurationManager.AppSettings["folderPath"],
                exclusionPaths = new HashSet<string>(ConfigurationManager.AppSettings["exclusionPaths"].Split(',')),
                packageID = ConfigurationManager.AppSettings["packageID"],
                packageVersion = ConfigurationManager.AppSettings["packageVersion"],
                nugetRepoUrl = ConfigurationManager.AppSettings["nugetRepoUrl"]
            };
        }

        public static List<string> GetValidationErrors(SugarRushConfiguration config)
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

            return errorMessages;
        }

        public static IEnumerable<FileInfo> FilterFiles(IEnumerable<FileInfo> files, HashSet<string> exclusionPaths)
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

        public static FileInfo[] GetRefreshFiles(string folderPath)
        {
            return GetFiles(folderPath, "*.refresh");
        }

        public static FileInfo[] GetPackageFiles(string folderPath)
        {
            return GetFiles(folderPath, "*packages.config");
        }

        public static FileInfo[] GetFiles(string folderPath, string extension)
        {
            return new DirectoryInfo(folderPath).GetFiles(extension, SearchOption.AllDirectories);
        }

        public static XmlDocument GetXmlDoc(string fullName)
        {
            var doc = new XmlDocument();
            doc.Load(fullName);

            return doc;
        }

        private static string GetPackageIdFromIncludeAttribute(XmlAttribute includeAttribute)
        {
            return includeAttribute.InnerText.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        private static string GetVersionFromIncludeNode(string includeString)
        {
            return Regex.Match(includeString, @"Version=(?<version>.*?),").Groups["version"].Value;
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

        private static string GetFilePathFromBaseUri(string baseUri)
        {
            return baseUri.Split(new string[] { @"file:///" }, StringSplitOptions.None)[1];
        }
    }
}
