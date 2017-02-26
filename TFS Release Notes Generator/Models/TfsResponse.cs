using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS_Release_Notes_Generator.Models
{
    #region Commit Response

    public class Author
    {
        public string name { get; set; }
        public string email { get; set; }
        public DateTime date { get; set; }
    }

    public class Committer
    {
        public string name { get; set; }
        public string email { get; set; }
        public DateTime date { get; set; }
    }

    public class ChangeCounts
    {
        public int Add { get; set; }
        public int Edit { get; set; }
        public int Delete { get; set; }
    }

    public class Commit
    {
        public string commitId { get; set; }
        public Author author { get; set; }
        public Committer committer { get; set; }
        public string comment { get; set; }
        public ChangeCounts changeCounts { get; set; }
        public string url { get; set; }
        public string remoteUrl { get; set; }
        public bool? commentTruncated { get; set; }
    }

    #endregion

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

    public class WiqlResponse
    {
        public string queryType { get; set; }
        public string queryResultType { get; set; }
        // might need to be string
        public DateTime asOf { get; set; }
        public List<Reference> columns { get; set; }
        public List<SortColumn> sortColumns { get; set; }
        public List<WorkItem> workItems { get; set; }
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

    public class Links
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
        public Links _links { get; set; }
    }

    #endregion

    public class CommitsResponse : Response<Commit>
    {
    }

    public class Response<TEntity>
    {
        public int count { get; set; }
        [JsonProperty("value")]
        public List<TEntity> items { get; set; }
    }

    #region ViewModels


    /// <summary>
    /// Release Commit View Model.
    /// </summary>
    public class ReleaseCommit : Commit
    {
        /// <summary>
        /// Regex extracted version name/number from comment text.
        /// </summary>
        public string version { get; set; }

        public ReleaseCommit(Commit commit, string version)
        {

            commitId = commit.commitId;
            author = commit.author;
            committer = commit.committer;
            comment = commit.comment;
            changeCounts = commit.changeCounts;
            url = commit.url;
            remoteUrl = commit.remoteUrl;
            commentTruncated = commit.commentTruncated;

            this.version = version;
        }
    }

    #endregion

    #region WIWL Query object for POST requests

    public class Query
    {
        public string query { get; set; }
    }

    #endregion
}
