using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TFS_Release_Notes_Generator.Models;

namespace TFS_Release_Notes_Generator
{
    public class ReportManager
    {
        private readonly TfsApiClient api;

        public ReportManager(string tfsInstance)
        {
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

        public IEnumerable<Library> CreateProjectLibrariesReport()
        {
            List<Library> tfsLibraries = new List<Library>();

            var repositories = api.GetRepositories();

            foreach (var repository in repositories.items)
            {
                var repoReferences = GetRepositoryReferences(repository.id, "develop");
                List<Library> repoLibraries = new List<Library>();

                foreach (var reference in repoReferences)
                {
                    repoLibraries.Add(new Library()
                    {
                        Name = reference.Key,
                        LibraryVersions = reference.Value.Select(x => new LibraryVersion()
                        {
                            Version = x,
                            Projects = new List<string> { repository.project.name }
                        }).ToList()
                    });
                }

                MergeLibraries(ref tfsLibraries, repoLibraries);
            }

            return tfsLibraries;
        }

        private void MergeLibraries(ref List<Library> destLibraries, List<Library> srcLibraries)
        {
            foreach (var srcLibrary in srcLibraries)
            {
                if (!destLibraries.Select(x => x.Name).Contains(srcLibrary.Name))
                {
                    destLibraries.Add(srcLibrary);
                }
                else
                {
                    var destLib = destLibraries.Single(x => x.Name.Equals(srcLibrary.Name));
                    foreach (var version in srcLibrary.LibraryVersions)
                    {
                        if (!destLib.LibraryVersions.Select(x => x.Version).Contains(version.Version))
                        {
                            destLib.LibraryVersions.Add(version);
                        }
                        else {
                            var destLibraryVersion = destLib.LibraryVersions.Single(x => x.Version.Equals(version.Version));
                            destLibraryVersion.Projects.AddRange(version.Projects);
                        }
                    }
                }
            }
        }
        private Dictionary<string, List<string>> GetRepositoryReferences(string repositoryGuid, string branch)
        {
            Dictionary<string, List<string>> repositoryReferences = new Dictionary<string, List<string>>();

            var filePaths = api.GetCsProjects(repositoryGuid, branch);
            foreach (var filePath in filePaths)
            {
                var bytes = api.GetItem(repositoryGuid, branch, filePath);
                using (var ms = new MemoryStream(bytes))
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(ms);

                    var references = GetProjectReferences(xmlDoc);
                    foreach (KeyValuePair<string, List<string>> kvp in references)
                    {
                        if (!repositoryReferences.Keys.Contains(kvp.Key))
                        {
                            repositoryReferences.Add(kvp.Key, kvp.Value);
                        }
                        else
                        {
                            repositoryReferences[kvp.Key].AddRange(kvp.Value);
                            repositoryReferences[kvp.Key] = repositoryReferences[kvp.Key].Distinct().ToList();
                        }
                    }
                }
            }

            return repositoryReferences;
        }

        private Dictionary<string, List<string>> GetProjectReferences(XmlDocument xmlDoc)
        {

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

            Dictionary<string, List<string>> references = new Dictionary<string, List<string>>();

            foreach (var include in includes)
            {
                var properties = include.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var library = properties[0].Trim();
                var version = string.Empty;
                if (properties.Length > 1)
                {
                    for (int i = 1; i < properties.Length; i++)
                    {
                        var propertyKvp = properties[i].Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                        if (propertyKvp.Length > 1 && propertyKvp[0].Trim().ToLower() == "version")
                        {
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

            return references;
        }
    }
}
