using System;
using System.Management.Automation;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets.Posts
{
    /// <summary>
    /// <para type="description">Create a new reddit post</para>
    /// <example>
    ///    <code>Get-Subreddit "example" | New-Post -Type Link -Title "Hello, world!" -Body "This is my new post"</code>
    /// </example>
    /// <example>
    ///    <code>New-Post -Subreddit example -Type Link -Title "Hello, world!" -Body "This is my new post"</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.New,"Post")]
    [OutputType(typeof(Post))]
    public class NewPost : PSCmdlet, IDynamicParameters
    {
        private SelfPostDynamicParameter selfPostContext;
        private LinkPostDynamicParameter linkPostContext;

        /// <summary>
        /// <para type="description">Subreddit in which to post.</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByInputObject",
                   Mandatory = true,
                   Position = 0,
                   ValueFromPipeline = true,
                   HelpMessage = "Subreddit in which to post.")]
        [ValidateNotNullOrEmpty]
        public Subreddit InputObject { get; set; }

        /// <summary>
        /// <para type="description">Subreddit in which to post.</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByName",
                   Mandatory = true,
                   Position = 0,
                   HelpMessage = "Subreddit in which to post.")]
        [ValidateNotNullOrEmpty]
        public string Subreddit { get; set; }

        /// <summary>
        /// <para type="synopsis">Link or self post.</para>
        /// <para type="description">Type of post to submit.  Link or self.</para>
        /// </summary>
        [Parameter(Mandatory = true,
                   Position = 1,
                   HelpMessage = "Type of post (link or self)")]
        [ValidateSet(new []{"Link","Self"})]
        public string Type { get; set; }

        /// <summary>
        /// <para type="description">Title of the post.</para>
        /// </summary>
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
            if (ParameterSetName == "ByInputObject")
                NewRedditPost(InputObject);
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
                WriteError(new ErrorRecord(ex, "CantSubmitPost", ErrorCategory.InvalidOperation, InputObject));
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
        /// <summary>
        /// <para type="description">Body of the self post.  Markdown is supported.</para>
        /// </summary>
        [Parameter(Mandatory=true,Position = 2,HelpMessage = "Self post body text/markdown.")]
        public string Body { get; set; }
    }

    public class LinkPostDynamicParameter
    {
        /// <summary>
        /// <para type="description">Url to link.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, HelpMessage = "Url to link.")]
        public string Url { get; set; }
    }
}
