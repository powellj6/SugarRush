using SugarRush;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SugarRushTests
{
    [TestClass]
    public class TextReplacementTests
    {
        string _dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        [TestMethod]
        public void ShouldGetAllFilesInDirNotExcluded()
        {

        }

        [TestMethod]
        public void ShouldGetAllFilesInDir()
        {

        }

        [TestMethod]
        public void ShouldUpdateHintPathInCsProjFile()
        {
            var docToUpdate = GetXmlDoc("CsProjExample1.csproj");
            var expectedDoc = GetXmlDoc("CsProjOnlyUpdateHintPath.csproj");
            var list = GetAssemblyNames(_dir + @"\Resources\DLLs\AjaxControlToolkit.dll");

            SugarRushHandler.UpdateCsProjFile(ref docToUpdate, "Domain.NettiersDAL.1.0.339", "Domain.NettiersDAL.1.0.340", list);

            Assert.AreEqual(expectedDoc.OuterXml, docToUpdate.OuterXml);
        }

        [TestMethod]
        public void ShouldUpdateHintPathAndReferenceVersionsInCsProjFile()
        {
            var docToUpdate = GetXmlDoc("CsProjExample1.csproj");
            var expectedDoc = GetXmlDoc("CsProjUpdateHintPathAndReferenceVersion.csproj");
            var list = GetAssemblyNames(_dir + @"\Resources\DLLs\Couchbase.dll", _dir + @"\Resources\DLLs\Enyim.Caching.dll");

            SugarRushHandler.UpdateCsProjFile(ref docToUpdate, "CouchbaseNetClient.1.3.9", "CouchbaseNetClient.1.3.10", list);

            Assert.AreEqual(expectedDoc.OuterXml, docToUpdate.OuterXml);
        }

        [TestMethod]
        public void ShouldUpdatePackageVersionInPackagesConfig()
        {

        }

        [TestMethod]
        public void ShouldUpdateVersionInRefreshFile()
        {

        }

        private List<AssemblyName> GetAssemblyNames(params string[] paths)
        {
            return paths.Select(path => AssemblyName.GetAssemblyName(path)).ToList();
        }

        private XmlDocument GetXmlDoc(string fileName)
        {
            var file = SugarRushHandler.GetCsProjFiles(_dir).Where(f => f.Name == fileName).First();
            var doc = new XmlDocument();
            doc.Load(file.FullName);

            return doc;
        }
    }
}
