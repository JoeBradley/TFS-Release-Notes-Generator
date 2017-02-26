using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS_Release_Notes_Generator.Models
{
    public class ReleaseDetails
    {
        public ReleaseCommit commit { get; set; }
        public List<WorkItem> workItems { get; set; }
        public List<WorkItemDetails> workItemDetails { get; set; }

        public List<WorkItemDetails> bugs { get { return GetWorkItemType("Bug"); } }
        public List<WorkItemDetails> tasks { get { return GetWorkItemType("Task"); } }
        public List<WorkItemDetails> userStories { get { return GetWorkItemType("User Story"); } }
        public List<WorkItemDetails> features { get { return GetWorkItemType("Feature"); } }
        public List<WorkItemDetails> epics { get { return GetWorkItemType("Epic"); } }

        private List<WorkItemDetails> GetWorkItemType(string Type)
        {
            var items = workItemDetails.Where(x => x.fields.WorkItemType.Equals(Type));
            return items.Any() ? items.ToList() : new List<WorkItemDetails>();
        }
    }
}
