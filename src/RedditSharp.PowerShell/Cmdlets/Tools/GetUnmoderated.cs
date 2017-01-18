using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Returns a unmoderated links in specified subreddit.</para>
    /// <example>
    ///    <code>Get-Unmoderated -Subreddit example</code>
    /// </example>
    /// <example>
    ///    <code>Get-Subreddit "example" | Get-Unmoderated</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "Unmoderated")]
    [OutputType(typeof(IEnumerable<Post>))]
    [CmdletBinding]
    public class GetUnmoderated : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Get unmoderated links in this subreddit.</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByInputObject",
             Mandatory = false,
             ValueFromPipeline = true,
             HelpMessage = "Target Subreddit.")]
        [ValidateNotNullOrEmpty]
        public Subreddit InputObject { get; set; }

        /// <summary>
        /// <para type="description">Get unmoderated links in this subreddit.</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByName", Mandatory = true, Position = 0, HelpMessage = "Subreddit name.")]
        [Alias("Sub")]
        public string Subreddit { get; set; }

        /// <summary>
        /// <para type="description">Limits the number of records to return.  Defaults to 100.  Set to -1 for no limit</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Limits the number of records to return.  Default 100.  Set to -1 for no limit.")]
        public int Limit { get; set; }

        /// <summary>
        /// <para type="description">Filter by this domain.</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Domain name.")]
        [ValidateNotNullOrEmpty]
        [Alias("Domain")]
        public string DomainName { get; set; }

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

        private GetUnmoderatedJob job;

        private DomainNameDynamicParameter domainContext;

        public GetUnmoderated()
        {
            this.Limit = 100;
            this.DomainName = null;
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
                    // get the listing
                    WriteObject(DoWork(), true);
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "Cantgetunmoderated", ErrorCategory.InvalidOperation, null));
                }
            }
            else
            {
                if (ParameterSetName == "ByName")
                    InputObject = GetSubreddit();

                job = new GetUnmoderatedJob();
                job.Subreddit = InputObject;
                job.Limit = Limit;
                job.Name = "Get-Unmoderated";
                job.DomainName = DomainName;
                JobRepository.Add(job);

                WriteObject(job);
                ThreadPool.QueueUserWorkItem(o => job.ProcessJob());
            }
        }

        private IEnumerable<Post> DoWork()
        {
            if (ParameterSetName == "ByName")
                InputObject = GetSubreddit();

            return GetPosts();
        }

        private IEnumerable<Post> GetPosts()
        {
            if (DomainName != null)
                return InputObject.UnmoderatedLinks.GetListing(Limit, 100).Where(x => x.Domain == DomainName).ToList();
            else
                return InputObject.UnmoderatedLinks.GetListing(Limit, 100).ToList();
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

    }

    internal class GetUnmoderatedJob : Job
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
        public string DomainName { get; set; }

        internal void ProcessJob()
        {
            SetJobState(JobState.Running);

            GetUnmoderated();

            Output.Complete();
            SetJobState(JobState.Completed);
        }

        private void GetUnmoderated()
        {
            try
            {
                Output.Add(PSObject.AsPSObject(GetPosts()));
            }
            catch (Exception ex)
            {
                Error.Add(new ErrorRecord(ex, "cantgetunmod", ErrorCategory.InvalidOperation, Subreddit));
                throw ex;
            }
        }

        private IEnumerable<Post> GetPosts()
        {
            if (DomainName != null)
                return Subreddit.UnmoderatedLinks.GetListing(Limit, 100).Where(x => x.Domain == DomainName).ToList();
            else
                return Subreddit.UnmoderatedLinks.GetListing(Limit, 100).ToList();
        }
    }
}
