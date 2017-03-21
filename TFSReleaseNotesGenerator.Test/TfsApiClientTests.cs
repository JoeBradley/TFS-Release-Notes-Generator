using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TFS_Release_Notes_Generator;
using System.Configuration;

namespace TFSReleaseNotesGenerator.Test
{
    [TestClass]
    public class TfsApiClientTests
    {
        TfsApiClient client = new TfsApiClient(ConfigurationManager.AppSettings["TfsInstance"]);

        [TestMethod]
        public void GetApiUrl_Test()
        {
            var result = client.GetApiUrl();
            var result2 = client.GetGitUrl();
            var result3 = client.GetWitUrl();
            var result4 = client.GetWitUrl("Puffin");
            var result5 = client.GetWiqlUrl("Puffin");
        }

        [TestMethod]
        public void GetProjects_Test()
        {
            var result = client.GetProjects();

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetRepositories_Test()
        {
            var result = client.GetRepositories();

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetNuGetPackages_Test()
        {
            var repoGuid = "c5fa7207-cd50-4c74-9546-c9a81b471e0c";
            var branch = "develop";

            var result = client.GetNuGetPackagesSimple(repoGuid, branch);

            Assert.IsNotNull(result);
        }
    }
}
