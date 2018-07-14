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

            var originalDoc = new XmlDocument();
            originalDoc.Load(file.FullName);


            var doc = new XmlDocument();
            doc.Load(file.FullName);

            var list = new List<AssemblyName> {
                AssemblyName.GetAssemblyName(_dir + @"\Resources\DLLs\AjaxControlToolkit.dll")
            };

            var updatedDoc = SugarRushHandler.UpdateCsProjFile(doc, "Domain.NettiersDAL.1.0.339", "Domain.NettiersDAL.1.0.340", list);


            updatedDoc.Save(file.FullName);

            //TODO: Actually compare
            Assert.AreNotEqual(doc, updatedDoc);
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
