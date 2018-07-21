using SugarRush;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SugarRushTests
{
    [TestClass]
    public class SugarRushHandlerTests
    {
        string _dir = Helper.GetExecutingAssemblyDirectory();

        [TestMethod]
        public void ShouldGetAllNotExcludedFilesInDir()
        {
            var exclusionPaths = new HashSet<string> { _dir + @"\Resources\Files\FilesSub" };

            var files = SugarRushHandler.GetFiles(_dir + @"\Resources\Files\", "*.txt");
            var excludedFiles = SugarRushHandler.FilterFiles(files, exclusionPaths);

            Assert.IsTrue(excludedFiles.Count() == 3);
        }

        [TestMethod]
        public void ShouldGetAllFilesInDir()
        {
            var files = SugarRushHandler.GetFiles(_dir + @"\Resources\Files\", "*.txt");

            Assert.IsTrue(files.Length == 5);
        }

        [TestMethod]
        public void ShouldValidateConfiguration()
        {
            var config = new SugarRushConfiguration {
                folderPath = @"C:\Some\Random\Bull",
                nugetRepoUrl = "SomeBullRepo.com",
                packageID = "SomePackage",
                packageVersion = "SomePackage-1.2.3"
            };

            SugarRush.SugarRushHandler.ValidateConfiguration(ref config);

            Assert.IsFalse(config.IsInvalid);
        }

        [TestMethod]
        public void ShouldUpdateHintPathInCsProjFile()
        {
            var updatedDoc = GetXmlDoc("CsProjExample1.csproj");
            var expectedDoc = GetXmlDoc("CsProjOnlyUpdateHintPath.csproj");
            var assList = GetAssemblyNames(_dir + @"\Resources\DLLs\AjaxControlToolkit.dll");
            var assDic = assList.ToDictionary(k => k.Name, v => v);

            updatedDoc.UpdateCsProjFile("Domain.NettiersDAL.1.0.340", assDic);

            Assert.AreEqual(expectedDoc.OuterXml, updatedDoc.OuterXml);
        }

        [TestMethod]
        public void ShouldUpdateHintPathAndReferenceVersionsInCsProjFile()
        {
            var updatedDoc = GetXmlDoc("CsProjExample1.csproj");
            var expectedDoc = GetXmlDoc("CsProjUpdateHintPathAndReferenceVersion.csproj");
            var assList = GetAssemblyNames(_dir + @"\Resources\DLLs\Couchbase.dll", _dir + @"\Resources\DLLs\Enyim.Caching.dll");
            var assDic = assList.ToDictionary(k => k.Name, v => v);

            updatedDoc.UpdateCsProjFile("CouchbaseNetClient.1.3.10", assDic);

            Assert.AreEqual(expectedDoc.OuterXml, updatedDoc.OuterXml);
        }

        [TestMethod]
        public void ShouldUpdatePackageVersionInPackagesConfig()
        {
            var updatedDoc = GetXmlDoc("PackageConfigExample1.config");
            var expectedDoc = GetXmlDoc("PackageConfigUpdatePackageVersion.config");

            updatedDoc.UpdatePackageConfig("CouchbaseNetClient", "1.3.10");

            Assert.AreEqual(expectedDoc.OuterXml, updatedDoc.OuterXml);
        }

        [TestMethod]
        public void ShouldUpdateVersionInRefreshFile()
        {
            var updatedFile = SugarRushHandler.GetFiles(_dir, "*.refresh").FirstOrDefault(f => f.Name == "RefreshFileExample1.dll.refresh");
            var expectedFile = SugarRushHandler.GetFiles(_dir, "*.refresh").FirstOrDefault(f => f.Name == "RefreshFileUpdatePackageVersion.dll.refresh");
            var assList = GetAssemblyNames(_dir + @"\Resources\DLLs\Couchbase.dll", _dir + @"\Resources\DLLs\Enyim.Caching.dll");
            var assDic = assList.ToDictionary(k => k.Name, v => v);

            updatedFile.UpdateRefreshFile("CouchbaseNetClient.1.3.10", assDic);

            Assert.AreEqual(System.IO.File.ReadAllText(updatedFile.FullName), System.IO.File.ReadAllText(expectedFile.FullName));
        }

        private List<AssemblyName> GetAssemblyNames(params string[] paths)
        {
            return paths.Select(path => AssemblyName.GetAssemblyName(path)).ToList();
        }

        private XmlDocument GetXmlDoc(string fileName)
        {
            var file = SugarRushHandler.GetFiles(_dir, "*").Where(f => f.Name == fileName).First();
            var doc = new XmlDocument();
            doc.Load(file.FullName);

            return doc;
        }
    }
}
