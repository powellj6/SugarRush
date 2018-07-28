using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SugarRush
{
    public class SugarRushConfiguration
    {
        public string folderPath { get; set; }
        public HashSet<string> exclusionPaths { get; set; }
        public string packageID { get; set; }
        public string packageVersion { get; set; }
        public string nugetRepoUrl { get; set; }
    }
}
