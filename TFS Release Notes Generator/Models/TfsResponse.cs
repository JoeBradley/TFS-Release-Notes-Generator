using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS_Release_Notes_Generator.Models
{
    #region Wiql response

    public class Reference
    {
        public string referenceName { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class SortColumn
    {
        public Reference field { get; set; }
        public bool descending { get; set; }
    }

    public class WorkItem
    {
        public int id { get; set; }
        public string url { get; set; }
    }
    
    #endregion

    #region WorkItemDetails

    public class Fields
    {
        [JsonProperty("System.AreaPath")]
        public string AreaPath { get; set; }
        [JsonProperty("System.TeamProject")]
        public string TeamProject { get; set; }
        [JsonProperty("System.IterationPath")]
        public string IterationPath { get; set; }
        [JsonProperty("System.WorkItemType")]
        public string WorkItemType { get; set; }
        [JsonProperty("System.State")]
        public string State { get; set; }
        [JsonProperty("System.Reason")]
        public string Reason { get; set; }
        [JsonProperty("System.AssignedTo")]
        public string AssignedTo { get; set; }
        [JsonProperty("System.CreatedDate")]
        public string CreatedDate { get; set; }
        [JsonProperty("System.CreatedBy")]
        public string CreatedBy { get; set; }
        [JsonProperty("System.ChangedDate")]
        public string ChangedDate { get; set; }
        [JsonProperty("System.ChangedBy")]
        public string ChangedBy { get; set; }
        [JsonProperty("System.Title")]
        public string Title { get; set; }
        [JsonProperty("Microsoft.VSTS.Scheduling.RemainingWork")]
        public double RemainingWork { get; set; }
        [JsonProperty("Microsoft.VSTS.Common.Activity")]
        public string Activity { get; set; }
        [JsonProperty("Microsoft.VSTS.Common.ClosedDate")]
        public string ClosedDate { get; set; }
        [JsonProperty("Microsoft.VSTS.Common.ClosedBy")]
        public string ClosedBy { get; set; }
        [JsonProperty("Microsoft.VSTS.Common.StateChangeDate")]
        public string StateChangeDate { get; set; }
        [JsonProperty("Microsoft.VSTS.Common.ActivatedDate")]
        public string ActivatedDate { get; set; }
        [JsonProperty("Microsoft.VSTS.Common.ActivatedBy")]
        public string ActivatedBy { get; set; }
        [JsonProperty("Microsoft.VSTS.Common.Priority")]
        public int Priority { get; set; }
        [JsonProperty("Microsoft.VSTS.Common.StackRank")]
        public double StackRank { get; set; }
        [JsonProperty("Microsoft.VSTS.Scheduling.OriginalEstimate")]
        public double OriginalEstimate { get; set; }
        [JsonProperty("Microsoft.VSTS.Scheduling.CompletedWork")]
        public double CompletedWork { get; set; }
        [JsonProperty("System.Description")]
        public string Description { get; set; }
    }
    
    public class Href
    {
        public string href { get; set; }
    }

    public class WorkItemLinks
    {
        public Href self { get; set; }
        public Href workItemUpdates { get; set; }
        public Href workItemRevisions { get; set; }
        public Href workItemHistory { get; set; }
        public Href html { get; set; }
        public Href workItemType { get; set; }
        public Href fields { get; set; }
    }

    public class WorkItemDetails : WorkItem
    {
        public int rev { get; set; }
        public Fields fields { get; set; }
        [JsonProperty("_links")]
        public WorkItemLinks _links { get; set; }
    }

    #endregion

}
