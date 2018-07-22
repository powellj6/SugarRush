using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NuGet;

namespace SugarRush
{
    class Program
    {
        //TODO: Don't update/save file if nothing has changed

        static void Main(string[] args)
        {
            try
            {
                var config = SugarRushHandler.GetConfiguration();

                if (config.IsInvalid)
                {
                    Logc("Invalid configuration: " + config.Message);
                    return;
                }

                var package = SugarRushHandler.GetPackage(config);

                if (package == null)
                {
                    Logc("Could not find any packages by packageID: " + config.packageID);
                    return;
                }

                var projFiles = SugarRushHandler.FilterFiles(SugarRushHandler.GetCsProjFiles(config.folderPath), config.exclusionPaths);
                var refreshFiles = SugarRushHandler.FilterFiles(SugarRushHandler.GetRefreshFiles(config.folderPath), config.exclusionPaths);
                var packageFiles = SugarRushHandler.FilterFiles(SugarRushHandler.GetPackageFiles(config.folderPath), config.exclusionPaths);

                var assReferences = package.AssemblyReferences.Select(x => (PhysicalPackageAssemblyReference) x);

                var assDic = assReferences.Select(x => AssemblyName.GetAssemblyName(x.SourcePath)).ToDictionary(k => k.Name, v => v);



                Parallel.ForEach<FileInfo>(projFiles, pf =>
                {
                    Console.WriteLine("Updating file: " + pf.Name);
                    var doc = SugarRushHandler.GetXmlDoc(pf.FullName);
                    doc.UpdateCsProjFile(config.packageID + "." + config.packageVersion, assDic);
                    doc.Save(pf.FullName);
                });

                Parallel.ForEach<FileInfo>(refreshFiles, rf =>
                {
                    rf.UpdateRefreshFile(config.packageID + "." + config.packageVersion, assDic);
                });

                Parallel.ForEach<FileInfo>(packageFiles, pf => {
                    var doc = SugarRushHandler.GetXmlDoc(pf.FullName);
                    doc.UpdatePackageConfig(config.packageID, config.packageVersion);
                    doc.Save(pf.FullName);
                });
            }

            catch (Exception exc)
            {
                Logc(String.Format("Something went wrong: Message: {0}, StackTrace: {1} ", exc.Message, exc.StackTrace));
            }
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
    }
}
