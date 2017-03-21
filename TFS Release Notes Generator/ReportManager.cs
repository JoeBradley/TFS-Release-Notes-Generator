using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS_Release_Notes_Generator
{
    public class ReportManager
    {
        private const string NuGetPackagePattern = "(?<name>[a-zA-Z\\.]+)\\.(?<version>[0-9\\.]+)\\.nupkg";

        private readonly TfsApiClient api;
        
        ReportManager(string tfsInstance) {
            api = new TfsApiClient(tfsInstance);
        }

        public string CreateProjectsReport()
        {
            var repositories = api.GetRepositories();
            foreach (var repository in repositories.items)
            {
                api.GetReleaseDetails(repository.project.name, repository.id);
            }
            return string.Empty;
        }
    }
}
