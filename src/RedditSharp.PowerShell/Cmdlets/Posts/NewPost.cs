using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets.Posts
{
    [Cmdlet(VerbsCommon.New,"Post")]
    [OutputType(typeof(Post))]
    public class NewPost : PSCmdlet, IDynamicParameters
    {
        private SelfPostDynamicParameter selfPostContext;
        private LinkPostDynamicParameter linkPostContext;

        [Parameter(ParameterSetName = "ByTarget",
                   Mandatory = true,
                   Position = 0,
                   ValueFromPipeline = true,
                   HelpMessage = "Subreddit in which to post.")]
        [ValidateNotNullOrEmpty]
        public Subreddit Target { get; set; }

        [Parameter(ParameterSetName = "ByName",
                   Mandatory = true,
                   Position = 0,
                   HelpMessage = "Subreddit in which to post.")]
        [ValidateNotNullOrEmpty]
        public string Subreddit { get; set; }

        [Parameter(Mandatory = true,
                   Position = 1,
                   HelpMessage = "Type of post (link or self)")]
        [ValidateSet(new []{"Link","Self"})]
        public string Type { get; set; }

        [Parameter(Mandatory = true,
           Position = 1,
           HelpMessage = "Post title.)")]
        [ValidateNotNullOrEmpty]
        public string Title { get; set; }

        protected override void BeginProcessing()
        {
            if (Session.Reddit == null)
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "NoRedditSession",
                    ErrorCategory.InvalidOperation, Session.Reddit));
        }

        protected override void ProcessRecord()
        {
            if (ParameterSetName == "ByTarget")
                NewRedditPost(Target);
            else
                NewRedditPost(GetSubreddit());
        }

        private void NewRedditPost(Subreddit s)
        {
            try
            {
                if (linkPostContext != null)
                {
                    WriteObject(s.SubmitPost(Title, linkPostContext.Url));
                }

                if (selfPostContext != null)
                {
                    WriteObject(s.SubmitTextPost(Title, selfPostContext.Body));
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CantSubmitPost", ErrorCategory.InvalidOperation, Target));
            }
        }

        private Subreddit GetSubreddit()
        {
            if (Subreddit.StartsWith("/r/"))
                Subreddit = Subreddit.Substring(0, 3);
            if (Subreddit.StartsWith("r/"))
                Subreddit = Subreddit.Substring(0, 2);

            return Session.Reddit.GetSubreddit(Subreddit);
        }

        // IDynamicParameters
        public object GetDynamicParameters()
        {
            if (Type == "Self")
            {
                selfPostContext = new SelfPostDynamicParameter();
                return selfPostContext;
            }
            if (Type == "Link")
            {
                linkPostContext = new LinkPostDynamicParameter();
                return linkPostContext;
            }
            return null;
        }

    }

    public class SelfPostDynamicParameter
    {
        [Parameter(Mandatory=true,Position = 2,HelpMessage = "Self post body text/markdown.")]
        public string Body { get; set; }
    }

    public class LinkPostDynamicParameter
    {
        [Parameter(Mandatory = true, Position = 2, HelpMessage = "Url to link.")]
        public string Url { get; set; }
    }
}
