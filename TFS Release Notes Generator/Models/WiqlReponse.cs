using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS_Release_Notes_Generator.Models
{
    /// <summary>
    /// WIQL query reponse
    /// </summary>
    public class WiqlResponse
    {
        public string queryType { get; set; }
        public string queryResultType { get; set; }
        public DateTime asOf { get; set; }
        public List<Reference> columns { get; set; }
        public List<SortColumn> sortColumns { get; set; }
        public List<WorkItem> workItems { get; set; }
    }
}
