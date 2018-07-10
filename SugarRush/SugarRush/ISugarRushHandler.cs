using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;
using SugarRush;

namespace SugarRush
{
    public interface ISugarRushHandler
    {
        SugarRushConfiguration GetConfiguration();

        SugarRushConfiguration ValidateConfiguration(ref SugarRushConfiguration config);

        IEnumerable<FileInfo> FilterFiles(FileInfo[] files, HashSet<string> exclusionPaths);

        IPackage GetPackage(SugarRushConfiguration config);

        IEnumerable<IPackage> GetPackages(SugarRushConfiguration config);

        FileInfo[] GetCsProjFiles(string folderPath);
    }
}
