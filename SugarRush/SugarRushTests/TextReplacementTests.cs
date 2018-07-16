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
        public void ShouldUpdateHintPathVersionsInCsProjFile()
        {
            var file = SugarRushHandler.GetCsProjFiles(_dir).Where(x => x.Name == "CsProjExample1.csproj").First();
            var doc = new XmlDocument();
            doc.Load(file.FullName);

            var expectedFile = SugarRushHandler.GetCsProjFiles(_dir).Where(x => x.Name == "CsProjOnlyUpdateHintPath.csproj").First();
            var expectedDoc = new XmlDocument();
            expectedDoc.Load(expectedFile.FullName);

            var list = new List<AssemblyName> {
                AssemblyName.GetAssemblyName(_dir + @"\Resources\DLLs\AjaxControlToolkit.dll")
            };

            SugarRushHandler.UpdateCsProjFile(ref doc, "Domain.NettiersDAL.1.0.339", "Domain.NettiersDAL.1.0.340", list);

            Assert.AreEqual(expectedDoc.OuterXml, doc.OuterXml);
        }

        [TestMethod]
        public void ShouldUpdatePackageVersionInPackagesConfig()
        {

        }

        [TestMethod]
        public void ShouldUpdateVersionInRefreshFile()
        {

        }
    }
}
