using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using TFS_Release_Notes_Generator;
using TFS_Release_Notes_Generator.Models;
using System.Linq;

namespace TFSReleaseNotesGenerator.Test
{
    [TestClass]
    public class ReportManagerTests
    {
        private string tfsInstance = ConfigurationManager.AppSettings["TfsInstance"];

        [TestMethod]
        public void CreateProjectLibrariesReport_ReturnsReport()
        {
            var rm = new ReportManager(tfsInstance);

            var report = rm.CreateProjectLibrariesReport()
                .OrderBy(x => x.Name)
                .ToList();

            var nonMsLibraries = report
                .Where(x => !x.Name.StartsWith("System"))
                .Where(x => !x.Name.StartsWith("Microsoft"))
                .ToList();

            var leastUsedLibraries = nonMsLibraries
                .Where(x => x.LibraryVersions.Any(y => y.Projects.Count() == 1))
                .ToList();

            var vendors = leastUsedLibraries
                .GroupBy(x => x.Name.Split(new[] { '.' })[0])
                .Select(x => new Library() {Name = x.Key, LibraryVersions = x.SelectMany(y => y.LibraryVersions).Distinct().ToList() } )
                .ToList();

            Assert.IsNotNull(leastUsedLibraries);
        }
    }
}
