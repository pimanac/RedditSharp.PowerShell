using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Returns a the modqueue for a specified subreddit.  Optionally filtered by domain or Author name.</para>
    /// <example>
    ///    <code>Get-ModQueue -Subreddit example</code>
    /// </example>
    /// <example>
    ///    <code>Get-Subreddit "example" | Get-ModQueue</code>
    /// </example>
    /// <example>
    ///    <code>Get-Subreddit "example" | Get-ModQueue -Kind Post -Domain reddit.com -Author foo</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "ModQueue")]
    [OutputType(typeof(IEnumerable<Post>))]
    [OutputType(typeof(IEnumerable<Comment>))]
    [OutputType(typeof(IEnumerable<VotableThing>))]
    [CmdletBinding]
    public class GetModQueue : PSCmdlet, IDynamicParameters
    {
        /// <summary>
        /// <para type="description">Get the modqueue for this subreddit.</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByInputObject",
             Mandatory = true,
             ValueFromPipeline = true,
             HelpMessage = "Target Subreddit.")]
        [ValidateNotNullOrEmpty]
        public Subreddit InputObject { get; set; }

        /// <summary>
        /// <para type="description">Get the modqueue for this subreddit.</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByName", Mandatory = true, Position = 0, HelpMessage = "Subreddit name.")]
        [Alias("Sub")]
        public string Subreddit { get; set; }

        /// <summary>
        /// <para type="description">Return comments, posts, or all.  Defaults to 'All'.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Return comments, posts, or all.  Defaults to 'All'")]
        [ValidateSet("All", "Comment", "Post")]
        public string Kind { get; set; }

        /// <summary>
        /// <para type="description">Return comments, posts, or all.  Defaults to 'All'.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Return items by this author.")]
        [ValidateNotNullOrEmpty()]
        [Alias("Author", "User")]
        public string AuthorName { get; set; }

        /// <summary>
        /// <para type="description">Limits the number of records to return.  Defaults to 100.  Set to -1 for no limit</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Limits the number of records to return.  Default 100.  Set to -1 for no limit.")]
        public int Limit { get; set; }

        /// <summary>
        /// AsJob
        /// </summary>
        [Parameter()]
        public SwitchParameter AsJob
        {
            get { return asjob; }
            set { asjob = value; }
        }

        private bool asjob;

        private GetModQueueJob job;

        private DomainNameDynamicParameter domainContext;

        public GetModQueue()
        {
            this.Limit = 100;
            this.Kind = "All";
            AuthorName = null;
        }

        protected override void BeginProcessing()
        {
            if (Session.Reddit == null)
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "NoRedditSession",
                    ErrorCategory.InvalidOperation, Session.Reddit));
        }

        protected override void ProcessRecord()
        {
            if (!asjob)
            {
                try
                {
                    WriteObject(DoWork(), true);
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "cantgetmodqueue", ErrorCategory.InvalidOperation, null));
                }
            }
            else
            {
                if (ParameterSetName == "ByName")
                    InputObject = GetSubreddit();

                string domain = null;
                if (domainContext != null)
                    domain = domainContext.DomainName;

                job = new GetModQueueJob
                {
                    Subreddit = InputObject,
                    Limit = Limit,
                    Name = "Get-ModQueue",
                    Domain = domain,
                    Author = AuthorName
                };
                JobRepository.Add(job);
                WriteObject(job);
                ThreadPool.QueueUserWorkItem(o => job.ProcessJob());
            }
        }

        private IEnumerable<VotableThing> DoWork()
        {
            if (ParameterSetName == "ByName")
                InputObject = GetSubreddit();

            switch (Kind.ToLower())
            {
                case "post":
                    return GetPosts();
                case "comment":
                    return GetComments();
            }
            return InputObject.ModQueue.GetListing(Limit, 100).ToList();
        }

        private IEnumerable<Post> GetPosts()
        {
            var posts = InputObject.ModQueue.GetListing(Limit, 100).Where(x => x.Kind == "t3").Select(y => y as Post);

            if (domainContext != null && domainContext.DomainName != "")
            {
                posts = posts.Where(x => x.Domain == domainContext.DomainName);
            }

            if (AuthorName != null)
            {
                posts = posts.Where(x => x.AuthorName == AuthorName);
            }

            return posts.Take(Limit);
        }

        private IEnumerable<Comment> GetComments()
        {
            var posts = InputObject.ModQueue.GetListing(Limit, 100).Where(x => x.Kind == "t1").Select(y => y as Comment);

            if (AuthorName != null)
            {
                posts = posts.Where(x => x.Author == AuthorName);
            }

            return posts.Take(Limit);
        }

        private Subreddit GetSubreddit()
        {
            if (Session.Cache.ContainsKey("xsub_" + Subreddit))
                return Session.GetCacheItem<Subreddit>("xsub_" + Subreddit);
            try
            {
                return Session.Reddit.GetSubreddit(Subreddit);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "cantgetmodlog", ErrorCategory.InvalidOperation, InputObject));
                throw ex;
            }
        }

        /// <inheritdoc />
        // IDynamicParameters
        public object GetDynamicParameters()
        {
            if (Kind.ToLower() == "post")
                return new DomainNameDynamicParameter();

            return null;
        }
    }

    internal class GetModQueueJob : Job
    {
        public override void StopJob()
        {
            throw new NotImplementedException();
        }

        public override string StatusMessage { get; }
        public override bool HasMoreData { get; }
        public override string Location { get; }

        public Subreddit Subreddit { get; set; }
        public int Limit { get; set; }
        public string Kind { get; set; }
        public string Domain { get; set; }
        public string Author { get; set; }

        public GetModQueueJob()
        {
            Kind = null;
            Domain = null;
            Author = null;
        }

        internal void ProcessJob()
        {
            SetJobState(JobState.Running);

            GetModLog();

            Output.Complete();
            SetJobState(JobState.Completed);
        }

        private void GetModLog()
        {
            if (Kind == null)
                Kind = "all";

            try
            {
                switch (Kind.ToLower())
                {
                    case "all":
                        Output.Add(PSObject.AsPSObject(Subreddit.UnmoderatedLinks.GetListing(Limit, 100).ToList()));
                        break;
                    case "comment":
                        Output.Add(PSObject.AsPSObject(GetComments()));
                        break;
                    case "post":
                        Output.Add(PSObject.AsPSObject(GetPosts()));
                        break;
                }
            }
            catch (Exception ex)
            {
                Error.Add(new ErrorRecord(ex, "cantgetunmod", ErrorCategory.InvalidOperation, Subreddit));
                throw ex;
            }
        }

        private IEnumerable<Post> GetPosts()
        {
            var posts = Subreddit.ModQueue.GetListing(Limit, 100).Where(x => x.Kind == "t3").Select(y => y as Post);

            if (Domain != null)
            {
                posts = posts.Where(x => x.Domain == Domain);
            }

            if (Author != null)
            {
                posts = posts.Where(x => x.AuthorName == Author);
            }

            return posts.Take(Limit);
        }

        private IEnumerable<Comment> GetComments()
        {
            var posts = Subreddit.ModQueue.GetListing(Limit, 100).Where(x => x.Kind == "t1").Select(y => y as Comment);

            if (Author != null)
            {
                posts = posts.Where(x => x.Author == Author);
            }

            return posts.Take(Limit);
        }
    }

    public class DomainNameDynamicParameter
    {
        /// <summary>
        /// <para type="description">Filter by this domain.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Domain name.")]
        [ValidateNotNullOrEmpty]
        [Alias("Domain")]
        public string DomainName { get; set; }
    }

}
