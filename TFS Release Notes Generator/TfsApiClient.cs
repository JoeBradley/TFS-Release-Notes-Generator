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
using System.Web;

namespace TFS_Release_Notes_Generator
{
    public class TfsApiClient
    {
        private const string apiVersion = "1.0";
        private const string ISO_8601 = "yyyy-MM-ddTHH:mm:ss.fffffff";
        private const string DefaultCollection = "DefaultCollection";
        
        private DateTime MinDate { get { return new DateTime(1970, 1, 1); } }

        private string BaseUrl;

        public TfsApiClient(string tfsInstance)
        {
            BaseUrl = string.Format("https://{0}", tfsInstance);
        }

        /// <summary>
        /// Base Project API Url
        /// </summary>
        /// <example>
        ///     https://{instance}/DefaultCollection/_apis/projects?api-version={version}[&stateFilter{string}&$top={integer}&skip={integer}]
        /// </example>
        /// <returns></returns>
        public string GetApiUrl()
        {
            return string.Format("{0}/{1}/_apis",
                BaseUrl,
                DefaultCollection);
        }

        /// <summary>
        /// Base for GIT api
        /// </summary>
        /// <example>
        ///     https://{instance}/DefaultCollection/_apis/git/repositories?api-version={version}
        ///     https://{instance}/DefaultCollection/_apis/git/{project}/repositories/{repository}/items?api-version={version}&scopePath={filePath}[&includeContentMetadata={bool}&lastProcessedChange={bool}]
        /// </example>
        /// <returns></returns>
        public string GetGitUrl()
        {
            return string.Format("{0}/{1}/_apis/git",
                BaseUrl,
                DefaultCollection);
        }

        /// <summary>
        /// base for WIT api
        /// </summary>
        /// <example>
        ///     https://{instance}/DefaultCollection/_apis/wit/workitems?api-version={version}&ids={string}[&fields={string}&asOf={DateTime}&$expand={enum{relations}&ErrorPolicy={string}]            
        ///     https://{instance}/DefaultCollection/[{project}/]_apis/wit/wiql?api-version={version}
        /// </example>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public string GetWitUrl(string projectName = null)
        {
            return string.Format("{0}/{1}/_apis/{2}wit",
                BaseUrl,
                DefaultCollection,
                projectName == null ? "" : projectName + "/");
        }

        // POST https://{instance}/DefaultCollection/[{project}/]_apis/wit/wiql?api-version={version}
        public string GetWiqlUrl(string projectName = null)
        {
            return $"{GetWitUrl(projectName)}/wit/wiql";
        }

        /// <summary> Get a list of projects
        /// </summary>
        /// <example>https://{instance}/DefaultCollection/_apis/projects?api-version={version}[&stateFilter{string}&$top={integer}&skip={integer}]</example>
        /// <see ref="https://www.visualstudio.com/en-us/docs/integrate/api/tfs/projects"/>
        /// <returns></returns>
        public Response<Project> GetProjects()
        {
            var action = "projects";
            var url = $"{GetApiUrl()}/{action}?api-version={apiVersion}";

            return GetApiResponse<Response<Project>>(url);
        }

        // https://{instance}/DefaultCollection/[{project}]/_apis/git/repositories/{repository}?api-version={version}
        public Response<Repository> GetRepositories()
        {
            var action = "repositories";
            var url = $"{GetGitUrl()}/{action}?api-version={apiVersion}";
            
            return GetApiResponse<Response<Repository>>(url);
        }

        public List<ReleaseCommit> GetReleaseMergeCommits(string repositoryGuid)
        {
            const string regexPattern = "^Merge branch 'release/(?<version>[a-zA-z0-9\\.\\-_]+)'$";
            List<ReleaseCommit> releaseCommits = new List<ReleaseCommit>();
            var start = MinDate;

            var commits = GetBranchCommits(repositoryGuid, "master");

            foreach (var item in commits.items)
            {
                var match = Regex.Match(item.comment, regexPattern);
                if (match.Success)
                {
                    var release = new ReleaseCommit(item, match.Groups["version"].Value, start, item.committer.date);
                    releaseCommits.Add(release);
                    start = release.end;
                }
            }

            return releaseCommits;
        }

        public List<ReleaseDetails> GetReleaseDetails(string projectName, string repositoryGuid)
        {
            List<ReleaseDetails> releases = new List<ReleaseDetails>();

            var commits = GetReleaseMergeCommits(repositoryGuid);
            var start = commits.Max(x => x.end);
            var end = DateTime.Now;

            var futureRelease = new ReleaseCommit(new Commit { }, "Future", start, end);
            commits.Add(futureRelease);

            commits.Reverse();

            foreach (var item in commits)
            {
                var release = new ReleaseDetails
                {
                    commit = item,
                    workItems = GetClosedWorkItems(projectName, item.start, item.end),
                    workItemDetails = new List<WorkItemDetails>()
                };

                var result = Parallel.ForEach(release.workItems,
                    new ParallelOptions { MaxDegreeOfParallelism = 5 },
                    x => release.workItemDetails.Add(GetWorkItem(x.url)));

                releases.Add(release);
            }

            // Revert back to descending order
            releases.Reverse();

            return releases;
        }

        public List<string> GetNuGetPackages(string repositoryGuid, string branch)
        {
            var items = GetItems(repositoryGuid, branch, HttpUtility.UrlEncode("/"), "*\\.nupkg", false, true);

            var files = items.SelectMany(x => x.Value.Select(f => f.contentMetadata.fileName).ToList()).ToList();

            return files;
        }

        public List<string> GetNuGetPackagesSimple(string repositoryGuid, string branch)
        {
            var items = GetItems(repositoryGuid, branch, HttpUtility.UrlEncode("/"), 4,"*\\.nupkg", false);

            var files = items.Select(f => f.contentMetadata.fileName).ToList();

            return files;
        }

        // https://christopher-cassidy.visualstudio.com/_apis/git/repositories/bcc4856d-6444-4ff5-abb2-123032967d54/items?versionType=branch&version=develop&path=&scopePath=%2f&recursionLevel=4&includeContentMetadata=true&latestProcessedChange=false&download=false
        // https://christopher-cassidy.visualstudio.com/_apis/git/repositories/c5fa7207-cd50-4c74-9546-c9a81b471e0c/items?versionType=branch&version=develop&path=&scopePath=%2FSCD.Business&recursionLevel=4&includeContentMetadata=true&latestProcessedChange=false&download=false";
        public List<Item> GetItems(string repositoryGuid, string branch, string path, string regexPattern = "", bool includeFolders = false)
        {
            var action = "items";
            var url = $"{GetGitUrl()}/repositories/{repositoryGuid}/{action}?versionType=branch&version={branch}&path=&scopePath={path}&recursionLevel=4&includeContentMetadata=true&latestProcessedChange=false&download=false";

            List<Item> items = new List<Item>();

            var result = GetApiResponse<Response<Item>>(url).items;

            if (!includeFolders)
                result = result.Where(x => !x.isFolder).ToList();

            if (string.IsNullOrEmpty(regexPattern))
                result = result.Where(x => x.isFolder || (!x.isFolder && Regex.IsMatch(x.contentMetadata.fileName, regexPattern))).ToList();

            return result;
        }

        // https://christopher-cassidy.visualstudio.com/_apis/git/repositories/bcc4856d-6444-4ff5-abb2-123032967d54/items?versionType=branch&version=develop&path=&scopePath=%2f&recursionLevel=4&includeContentMetadata=true&latestProcessedChange=false&download=false
        // https://christopher-cassidy.visualstudio.com/_apis/git/repositories/c5fa7207-cd50-4c74-9546-c9a81b471e0c/items?versionType=branch&version=develop&path=&scopePath=%2FSCD.Business&recursionLevel=4&includeContentMetadata=true&latestProcessedChange=false&download=false";
        public List<Item> GetItems(string repositoryGuid, string branch, string path, int recursionLevels, string regexPattern = "", bool includeFolders = false)
        {
            var action = "items";
            var url = $"{GetGitUrl()}/repositories/{repositoryGuid}/{action}?versionType=branch&version={branch}&path=&scopePath={path}&recursionLevel={recursionLevels}&includeContentMetadata=true&latestProcessedChange=false&download=false";

            List<Item> items = new List<Item>();

            var result = GetApiResponse<Response<Item>>(url).items;

            if (!includeFolders)
                result = result.Where(x => !x.isFolder).ToList();

            if (string.IsNullOrEmpty(regexPattern))
                result = result.Where(x => x.isFolder || (!x.isFolder && Regex.IsMatch(x.contentMetadata.fileName, regexPattern))).ToList();

            return result;
        }

        public Dictionary<string, List<Item>> GetItems(string repositoryGuid, string branch, string path, string regexPattern = "", bool includeFolders = false, bool recursive = true)
        {
            var list = new Dictionary<string, List<Item>>();

            var items = GetItems(repositoryGuid, branch, path, regexPattern, true);

            list.Add(path, items.Where(x => !x.isFolder).ToList());

            foreach (var item in items.Where(x => x.isFolder))
            {
                var subitems = GetItems(repositoryGuid, branch, item.path, regexPattern, true, true);
                foreach (var subitem in subitems)
                {
                    list.Add(subitem.Key, subitem.Value);
                }
            }

            return list;
        }

        #region Api Wrappers

        /// <summary>
        /// <example>https://{instance}/DefaultCollection/_apis/git/repositories/{repository}/commits?api-version={version}[&branch={string}&commit={string}&itemPath={string}&committer={string}&author={string}&fromDate={dateTime}&toDate={dateTime}[&$top={integer}&$skip={integer}]</example>
        /// </summary>    
        /// <param name="repositoryGuid"></param>
        /// <param name="branch"></param>
        /// <returns></returns>
        public Response<Commit> GetBranchCommits(string repositoryGuid, string branch)
        {
            var action = "commits";
            var filter = $"branch={branch}";
            var url = $"{GetWitUrl(repositoryGuid)}/{action}?api-version={apiVersion}&{filter}";

            return GetApiResponse<Response<Commit>>(url);
        }

        public WorkItemDetails GetWorkItem(string url)
        {
            return GetApiResponse<WorkItemDetails>(url);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <example>
        ///     https://{instance}/DefaultCollection/_apis/wit/workitems?api-version={version}&ids={string}[&fields={string}&asOf={DateTime}&$expand={enum{relations}]
        /// </example>
        /// <param name="workItemId"></param>
        /// <returns></returns>
        public WorkItemDetails GetWorkItem(int workItemId)
        {
            try
            {
                var action = "workitems";
                var url = $"{GetApiUrl()}/{action}?api-version={apiVersion}&ids={workItemId}";

                return GetApiResponse<WorkItemDetails>(url);
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

        public List<WorkItem> GetClosedWorkItems(string projectName, DateTime start, DateTime end)
        {
            var qry =
            $"Select [System.Id], [System.Title], [System.State] " +
            "From WorkItems " +
            "Where " +
            "[System.TeamProject] = @project AND" +
            "[State] = 'Closed' AND [State] <> 'Removed' AND " +
            $"[Microsoft.VSTS.Common.ClosedDate] >= '{start.Date.ToString(ISO_8601)}' AND " +
            $"[Microsoft.VSTS.Common.ClosedDate] < '{end.Date.AddDays(1).ToString(ISO_8601)}' " +
            "order by [System.Title] asc";

            var response = GetWiqlResponse(qry, projectName);

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

        private WiqlResponse GetWiqlResponse(string queryString, string projectName = null)
        {
            var url = $"{GetWiqlUrl(projectName)}?api-version={apiVersion}";
            var query = new Query { query = queryString };
            var json = JsonConvert.SerializeObject(query);

            var response = WebApi.Post(url, json);

            return JsonConvert.DeserializeObject<WiqlResponse>(response);
        }

        private T GetApiResponse<T>(string url)
        {
            var json = WebApi.Get(url);

            return JsonConvert.DeserializeObject<T>(json);
        }

        #endregion

    }
}
