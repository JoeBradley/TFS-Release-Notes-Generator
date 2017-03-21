using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS_Release_Notes_Generator.Models
{    
    public class Project
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string state { get; set; }
        public int revision { get; set; }
        public string description { get; set; }
    }

    public class ProjectDetails : Project
    {
        public ProjectLinks _links { get; set; }
        public DefaultTeam defaultTeam { get; set; }
    }

    public class DefaultTeam
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class ProjectLinks
    {
        public Href self { get; set; }
        public Href collection { get; set; }
        public Href web { get; set; }
    }
}
