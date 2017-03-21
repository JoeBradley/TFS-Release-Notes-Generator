using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFS_Release_Notes_Generator.Models
{
    #region Commit Response

    public class Commit
    {
        public string commitId { get; set; }
        public Author author { get; set; }
        public Committer committer { get; set; }
        public string comment { get; set; }
        public ChangeCounts changeCounts { get; set; }
        public string url { get; set; }
        public string remoteUrl { get; set; }
        public bool? commentTruncated { get; set; }
    }

    public class Author
    {
        public string name { get; set; }
        public string email { get; set; }
        public DateTime date { get; set; }
    }

    public class Committer
    {
        public string name { get; set; }
        public string email { get; set; }
        public DateTime date { get; set; }
    }

    public class ChangeCounts
    {
        public int Add { get; set; }
        public int Edit { get; set; }
        public int Delete { get; set; }
    }

    #endregion

    #region ViewModels

    /// <summary>
    /// Release Commit View Model.
    /// </summary>
    public class ReleaseCommit : Commit
    {
        /// <summary>
        /// Regex extracted version name/number from comment text.
        /// </summary>
        public string version { get; set; }

        /// <summary>
        /// Date of previous release.  1.1.1970 for first release.
        /// </summary>
        public DateTime start { get; set; }

        /// <summary>
        /// Release Date
        /// </summary>
        public DateTime end { get; set; }

        public ReleaseCommit(Commit commit, string version, DateTime start, DateTime end)
        {

            commitId = commit.commitId;
            author = commit.author;
            committer = commit.committer;
            comment = commit.comment;
            changeCounts = commit.changeCounts;
            url = commit.url;
            remoteUrl = commit.remoteUrl;
            commentTruncated = commit.commentTruncated;

            this.version = version;
            this.start = start;
            this.end = end;
        }
    }

    #endregion

}
