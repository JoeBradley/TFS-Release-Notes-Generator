using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TFS_Release_Notes_Generator;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections.Generic;

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
        public void GetItems_ReturnsPathItems()
        {
            var repositoryGuid = "c5fa7207-cd50-4c74-9546-c9a81b471e0c";
            var branch = "develop";
            string path = HttpUtility.UrlEncode("/");
            string regexPattern = null;
            bool includeFolders = false;


            var result = client.GetItems(repositoryGuid, branch, path, regexPattern, includeFolders);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public void GetCsProjectFiles_ReturnProjectFiles()
        {
            var repositoryGuid = "c5fa7207-cd50-4c74-9546-c9a81b471e0c";
            var branch = "develop";
            string path = HttpUtility.UrlEncode("/");

            var result = client.GetCsProjects(repositoryGuid, branch);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
        }

        [TestMethod]
        public void GetCsProjectFile_ReturnProjectFile()
        {
            var repositoryGuid = "c5fa7207-cd50-4c74-9546-c9a81b471e0c";
            var branch = "develop";
            var path = "/Puffin.Service/Puffin.Service.csproj";

            var result = client.GetItem(repositoryGuid, branch, path);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetCsProjectFileReferences_ReturnReferences()
        {
            var repositoryGuid = "c5fa7207-cd50-4c74-9546-c9a81b471e0c";
            var branch = "develop";
            var path = "/Puffin.Service/Puffin.Service.csproj";

            var result = client.GetItem(repositoryGuid, branch, path);
            Assert.IsNotNull(result);

            using (var ms = new MemoryStream(result))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(ms);

                var ns = xmlDoc.DocumentElement.Attributes["xmlns"].Value;
                //var ns = "http://schemas.microsoft.com/developer/msbuild/2003";
                var nsm = new XmlNamespaceManager(xmlDoc.NameTable);
                nsm.AddNamespace("ns", ns);

                var pathExp = "descendant::ns:ItemGroup/ns:Reference";

                var nodes = xmlDoc.DocumentElement.SelectNodes(pathExp, nsm);
                var references = new List<string>();
                foreach (XmlNode node in nodes)
                {
                    references.Add(node.Attributes["Include"].Value);
                }
                references = references.Distinct().OrderBy(x => x).ToList();
                Assert.IsTrue(references.Any());
            }
        }

        [TestMethod]
        public void GetLocalCsProjectFileReferences_ReturnReferences()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "/data/test.csproj";

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(path);

            var ns = xmlDoc.DocumentElement.Attributes["xmlns"]?.Value;
            var nsPattern = string.Empty;
            XmlNamespaceManager nsm = null;
            if (!string.IsNullOrEmpty(ns))
            {
                var nsPrefix = "ns";
                nsm = new XmlNamespaceManager(xmlDoc.NameTable);
                nsm.AddNamespace(nsPrefix, ns);
                nsPattern = $"{nsPrefix}:";
            }

            var pathExp = $"descendant::{nsPattern}ItemGroup/{nsPattern}Reference";

            var nodes = nsm != null ? 
                xmlDoc.DocumentElement.SelectNodes(pathExp, nsm) :
                xmlDoc.DocumentElement.SelectNodes(pathExp);
            
            var includes = new List<string>();
            foreach (XmlNode node in nodes)
            {
                includes.Add(node.Attributes["Include"].Value);
            }
            includes = includes.Distinct().OrderBy(x => x).ToList();
            Assert.IsTrue(includes.Any());

            Dictionary<string, List<string>> references = new Dictionary<string, List<string>>();

            foreach (var include in includes)
            {
                var properties = include.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var library = properties[0].Trim();
                var version = string.Empty;
                if (properties.Length > 1) {
                    for (int i = 1; i < properties.Length; i++) {
                        var propertyKvp = properties[i].Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        if (propertyKvp.Length > 1 && propertyKvp[0].Trim().ToLower() == "version") {
                            version = propertyKvp[1].Trim();
                            break;
                        }
                    }
                }

                if (references.Keys.Contains(library))
                    references[library].Add(version);
                else
                    references.Add(library, new List<string> { version });
            }

            Assert.IsNotNull(references);
        }
    }
}
