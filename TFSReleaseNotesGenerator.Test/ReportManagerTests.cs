using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using TFS_Release_Notes_Generator;

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

            var report = rm.CreateProjectLibrariesReport();

            Assert.IsNotNull(report);
        }
    }
}
