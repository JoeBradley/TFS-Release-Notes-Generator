using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS_Release_Notes_Generator.Models
{    
    public class Item
    {
        public string objectId { get; set; }
        public string gitObjectType { get; set; }
        public string commitId { get; set; }
        public string path { get; set; }
        public bool isFolder { get; set; }
        public ContentMetadata contentMetadata { get; set; }
        public string url { get; set; }
    }

    public class ContentMetadata
    {
        public string fileName { get; set; }
    }
}
