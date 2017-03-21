using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS_Release_Notes_Generator.Models
{
    public class Repository
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public Project project { get; set; }
        public string defaultBranch { get; set; }
        public string remoteUrl { get; set; }
    }
}
