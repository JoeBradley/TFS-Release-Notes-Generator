using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TFS_Release_Notes_Generator.Models;
using System.Text.RegularExpressions;
using System.Net.Http.Headers;

namespace TFS_Release_Notes_Generator
{
    public class TfsApiManager
    {
        private const string apiVersion = "1.0";
        private const string ISO_8601 = "yyyy-MM-ddTHH:mm:ss.fffffff";

        private DateTime MinDate { get { return new DateTime(2000, 1, 1); } }

        private string BaseUrl;
        private string BaseRepositoryUrl;
        private string BaseWiqlUrl;

        public TfsApiManager()
        {
            BaseUrl = $"https://{ConfigurationManager.AppSettings["TfsInstance"]}/DefaultCollection/{ConfigurationManager.AppSettings["ProjectName"]}/_apis";
            BaseRepositoryUrl = $"{BaseUrl}/git/repositories/{ConfigurationManager.AppSettings["RepositoryGUID"]}/";
            BaseWiqlUrl = $"{BaseUrl}/wit/wiql";
        }

        public List<ReleaseCommit> GetReleaseMergeCommits()
        {
            const string regexPattern = "^Merge branch 'release/(?<version>[a-zA-z0-9\\.\\-_]+)'$";
            List<ReleaseCommit> releaseCommits = new List<ReleaseCommit>();

            var commits = GetBranchCommits("master");

            foreach (var item in commits.items)
            {
                var match = Regex.Match(item.comment, regexPattern);
                if (match.Success)
                {
                    releaseCommits.Add(new ReleaseCommit(item, match.Groups["version"].Value));
                }
            }

            return releaseCommits;
        }

        public List<ReleaseDetails> GetReleaseDetails()
        {
            List<ReleaseDetails> releases = new List<ReleaseDetails>();

            var start = MinDate;
            var commits = GetReleaseMergeCommits();
            commits.Reverse();

            foreach (var item in commits)
            {
                var end = item.committer.date;

                var release = new ReleaseDetails
                {
                    commit = item,
                    workItems = GetClosedWorkItems(start, end),
                    workItemDetails = new List<WorkItemDetails>()
                };

                var result = Parallel.ForEach(release.workItems, 
                    new ParallelOptions { MaxDegreeOfParallelism = 5 }, 
                    x => release.workItemDetails.Add(GetWorkItem(x.url)));
                
                releases.Add(release);

                start = end;
            }

            // Revert back to descending order
            releases.Reverse();

            return releases;
        }

        #region Api Wrappers

        public CommitsResponse GetBranchCommits(string branch)
        {
            var action = "commits";
            var filter = $"branch={branch}";
            var url = $"{BaseRepositoryUrl}/{action}?api-version={apiVersion}&{filter}";

            var json = WebApi.Get(url);

            return JsonConvert.DeserializeObject<CommitsResponse>(json);
        }

        public WorkItemDetails GetWorkItem(string url)
        {
            try
            {
                var json = WebApi.Get(url);
                var item = JsonConvert.DeserializeObject<WorkItemDetails>(json);
                return item;
            }
            catch (Exception ex) {
                return null;
            }
        }
        
        public WorkItemDetails GetWorkItem(int workItemId)
        {
            try
            {
                var action = "workitems";
                var url = $"{BaseRepositoryUrl}/{action}?api-version={apiVersion}&ids={workItemId}";
            //https://{instance}/DefaultCollection/_apis/wit/workitems?api-version={version}&ids={string}[&fields={string}&asOf={DateTime}&$expand={enum{relations}]

                var json = WebApi.Get(url);
                var item = JsonConvert.DeserializeObject<WorkItemDetails>(json);
                return item;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<WorkItem> GetWorkItems()
        {

            var qry =
                    "Select [System.Id], [System.Title], [System.State] " +
                    "From WorkItems " +
                    "Where [System.WorkItemType] = 'Task' AND [State] <> 'Closed' AND [State] <> 'Removed' " +
                    "order by [Microsoft.VSTS.Common.Priority] asc, [System.CreatedDate] desc";

            var response = GetWiqlResponse(qry);

            return response.workItems;
        }

        public List<WorkItem> GetClosedWorkItems(DateTime start, DateTime end)
        {
            var qry =
            $"Select [System.Id], [System.Title], [System.State] " +
            "From WorkItems " +
            "Where [State] = 'Closed' AND [State] <> 'Removed' AND " +
            $"[Microsoft.VSTS.Common.ClosedDate] >= '{start.Date.ToString(ISO_8601)}' AND " +
            $"[Microsoft.VSTS.Common.ClosedDate] < '{end.Date.AddDays(1).ToString(ISO_8601)}' " +
            "order by [System.Title] asc";
            
            var response = GetWiqlResponse(qry);

            return response.workItems;
        }

        public List<WorkItem> GetClosedWorkItems(DateTime start, DateTime end, string workItemType)
        {
            var qry =
            $"Select [System.Id], [System.Title], [System.State] " +
            "From WorkItems " +
            $"Where [System.WorkItemType] = '{workItemType}' AND [State] = 'Closed' AND [State] <> 'Removed' AND " +
            $"[Microsoft.VSTS.Common.ClosedDate] >= '{start.Date.ToString(ISO_8601)}' AND " +
            $"[Microsoft.VSTS.Common.ClosedDate] < '{end.Date.AddDays(1).ToString(ISO_8601)}' " +
            "order by [System.Title] asc";

            var response = GetWiqlResponse(qry);

            return response.workItems;
        } 

        #endregion

        #region Helpers


        private WiqlResponse GetWiqlResponse(string queryString)
        {
            var url = $"{BaseWiqlUrl}?api-version={apiVersion}";
            var query = new Query { query = queryString };
            var json = JsonConvert.SerializeObject(query);

            var response = WebApi.Post(url, json);

            return JsonConvert.DeserializeObject<WiqlResponse>(response);
        }

        #endregion

    }
}
