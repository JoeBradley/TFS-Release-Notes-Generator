using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS_Release_Notes_Generator
{
    class Program
    {
        static TfsApiManager api = new TfsApiManager();

        static void Main(string[] args)
        {
            PrintReleaseDetails();

            Console.WriteLine();

            Console.ReadLine();
        }

        private static void PrintReleseMergeCommits()
        {
            var releaseCommits = api.GetReleaseMergeCommits();

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
            var releases = api.GetReleaseDetails();
            foreach (var release in releases)
            {
                Console.WriteLine($"Release: {release.commit.version}");

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
    }
}
