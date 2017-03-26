using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS_Release_Notes_Generator.Models
{
    public class Library
    {
        public string Name { get; set; }
        public List<LibraryVersion> LibraryVersions { get; set; }
    }

    public class LibraryVersion
    {
        public string Version { get; set; }
        public List<string> Projects { get; set; }
    }
}
