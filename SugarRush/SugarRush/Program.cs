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

            try
            {
                var config = SugarRushHandler.GetConfiguration();

                if (config.IsInvalid)
                {
                    Console.WriteLine("Invalid configuration: " + config.Message);
                    return;
                }

                var projFiles = SugarRushHandler.GetCsProjFiles(config.folderPath);

                var filteredProjFiles = SugarRushHandler.FilterFiles(projFiles, config.exclusionPaths).ToList();

                var package = SugarRushHandler.GetPackage(config);

                if (package == null)
                {
                    Console.WriteLine("Could not find any packages by packageID: " + config.packageID);
                    return;
                }

                var assReferences = package.AssemblyReferences.Select(x => (PhysicalPackageAssemblyReference) x);

                var path = assReferences.First().SourcePath;

                var name = System.Reflection.AssemblyName.GetAssemblyName(path);
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
