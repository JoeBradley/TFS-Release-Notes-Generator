using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS_Release_Notes_Generator
{
    class Program
    {
        static string tfsInstance;
        static string defaultRepositoryGuid;
        static string defaultProjectName;
        static TfsApiClient api;

        static void Main(string[] args)
        {
            tfsInstance = ConfigurationManager.AppSettings["TfsInstance"];
            defaultRepositoryGuid = ConfigurationManager.AppSettings["RepositoryGUID"];
            defaultProjectName = ConfigurationManager.AppSettings["ProjectName"];

            api = new TfsApiClient(tfsInstance);

            Console.WriteLine("Loading report...");

            //PrintReleaseDetails();
            PrintNugetPackages();
            
            Console.WriteLine();

            //Console.ReadLine();
        }

        private static void PrintReleaseMergeCommits()
        {

            var releaseCommits = api.GetReleaseMergeCommits(defaultRepositoryGuid);

            foreach (var item in releaseCommits)
            {
                Console.WriteLine($"Release: {item.version}");
            }
        }

        private static void PrintWorkItems()
        {
            Console.WriteLine("Work Items:");

            var workItems = api.GetWorkItems();

            foreach (var item in workItems)
            {
                Console.WriteLine($"Work Item: {item.id}");
            }
        }

        private static void PrintReleaseDetails()
        {
            var releases = api.GetReleaseDetails(defaultProjectName, defaultRepositoryGuid);

            Console.Clear();

            foreach (var release in releases)
            {
                Console.WriteLine($"Release: {release.commit.version}");
                Console.WriteLine($"Start: {release.commit.start}, End: {release.commit.end}");

                Console.WriteLine("\tFeatures:");
                release.features.ForEach(x => Console.WriteLine($"\t\t[{x.id}] \t{x.fields.Title}: {x.fields.Description}"));
                Console.WriteLine();

                Console.WriteLine("\tUser Stories:");
                release.userStories.ForEach(x => Console.WriteLine($"\t\t[{x.id}] \t{x.fields.Title}: {x.fields.Description}"));
                Console.WriteLine();

                Console.WriteLine("\tBugs:");
                release.bugs.ForEach(x => Console.WriteLine($"\t\t[{x.id}] \t{x.fields.Title}: {x.fields.Description}"));
                Console.WriteLine();
            }
        }

        private static void PrintNugetPackages()
        {
            var repos = api.GetRepositories().items;
            foreach (var repo in repos)
            {
                var files = api.GetCsProjects(repo.id, "develop");
                Console.WriteLine($"Repo: {repo.project.name}");

                foreach (var file in files)
                {
                    Console.WriteLine($"Project Files: {file}");
                }
                Console.WriteLine();
            }
        }
    }
}
