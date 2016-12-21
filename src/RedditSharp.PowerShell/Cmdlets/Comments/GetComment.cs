using System;
using System.Management.Automation;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get,"Comment")]
    [OutputType(typeof(Comment))]
    public class GetComment : Cmdlet
    {
        [Parameter(
            ParameterSetName = "ByUrl",
            Mandatory = true,
            Position = 0,
            HelpMessage = "Comment Url"
        )]
        [ValidateNotNullOrEmpty]
        public string Url { get; set; }

        [Parameter(
            ParameterSetName = "ByName",
            Mandatory = true,
            Position = 0,
            HelpMessage = "Base36 id of the reddit post.")]
        [ValidateNotNullOrEmpty]
        public string Subreddit { get; set; }

        [Parameter(
            ParameterSetName = "ByName",
            Mandatory = true,
            Position = 0,
            HelpMessage = "Base36 id of the reddit post.")]
        [ValidateNotNullOrEmpty]
        public string PostId { get; set; }

        [Parameter(
            ParameterSetName = "ByName",
            Mandatory = true,
            Position = 1,
            HelpMessage = "Base 36 id of the comment.")]
        [ValidateNotNullOrEmpty]
        public string CommentId { get; set; }


        protected override void BeginProcessing()
        {
            if (Session.Reddit == null)
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "NoRedditSession",
                    ErrorCategory.InvalidOperation, Session.Reddit));
        }

        protected override void ProcessRecord()
        {
            if (Subreddit.StartsWith("/r/"))
                Subreddit = Subreddit.Remove(0, 3);

            if (Subreddit.StartsWith("r/"))
                Subreddit = Subreddit.Remove(0, 2);

            var r = Session.Reddit;
            Session.Log.Debug($"Getting Comment");
            if (Url != String.Empty)
                WriteObject(GetRedditComment(new Uri(Url)));
            else
                WriteObject(GetRedditComment(Subreddit, PostId, CommentId));
        }

        private Comment GetRedditComment(Uri uri)
        {
            try
            {
                return Session.Reddit.GetComment(uri);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CantGetComment", ErrorCategory.InvalidOperation, Url));
                throw;
            }
        }

        private Comment GetRedditComment(string subreddit, string name, string linkName)
        {
            try
            {
                return Session.Reddit.GetComment(subreddit,name,linkName);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CantGetComment", ErrorCategory.InvalidOperation, Url));
                throw;
            }
        }
    }
}
