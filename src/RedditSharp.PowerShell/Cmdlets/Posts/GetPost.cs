using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Returns a post or multiple posts from a subreddit</para>
    /// <para type="description">By default limit to 1000 records.  Set -Limit to -1 for unlimited.</para>
    /// <example>
    ///    <code>Get-Post -Subreddit example</code>
    /// </example>
    /// <example>
    ///    <code>Get-Subreddit "example" | Get-Post -Sort Hot</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Get,"Post")]
    [OutputType(typeof(Post))]
    [OutputType((typeof(IEnumerable<Post>)))]
    public class GetPost : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Fully qualified url to the reddit post</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByUrl",Mandatory=false,HelpMessage = "Url to the reddit post.")]
        [ValidateNotNullOrEmpty]
        public string Url { get; set; }

        /// <summary>
        /// <para type="description">Subreddit name</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByName",Mandatory = false,HelpMessage = "Subreddit name from which")]
        [Alias("Sub")]
        public string Subreddit { get; set; }

        /// <summary>
        /// <para type="description">The page to return.  Gilded is sorted by new.</para>
        /// </summary>
        [Parameter(Mandatory = true,HelpMessage = "Subreddit page.")]
        [ValidateSet("New","Rising","Hot")]
        [Alias("Sort")]
        public string SortBy { get; set; }

        /// <summary>
        /// <para type="description">Limits the number of records to return.  Defaults to 100.  Set to -1 for no limit</para>
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Limits the number of records to return.  Default 100.  Set to -1 for no limit.")]
        public int Limit { get; set; }

        /// <summary>
        /// process as job
        /// </summary>
        [Parameter()]
        public SwitchParameter AsJob
        {
            get { return asjob; }
            set { asjob = value; }
        }
        private bool asjob;

        [Parameter(
             ParameterSetName = "ByInputObject",
             Mandatory = true,
             ValueFromPipeline = true,
             HelpMessage = "Subreddit object")]
        [ValidateNotNullOrEmpty]
        public Subreddit InputObject
        {
            get { return inputObject; }
            set { inputObject = value; }
        }

        private Subreddit inputObject;

        private GetPostJob job;

        public GetPost()
        {
            Limit = 100;
        }

        protected override void BeginProcessing()
        {
            if (Session.Reddit == null)
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "NoRedditSession",
                    ErrorCategory.InvalidOperation, Session.Reddit));
        }

        protected override void ProcessRecord()
        {
            switch (ParameterSetName)
            {
                case "ByInputObject":
                    ByInputObject();
                    break;
                case "ByUrl":
                    ByUrl();
                    break;
                case "ByName":
                    ByName();
                    break;
                default:
                    break;
            }
        }

        private void ByUrl()
        {
            if (AsJob)
            {
                job = new GetPostJob
                {
                    Subreddit = null,
                    Limit = Limit,
                    Name = "Get-Post  -Uri" + Url,
                    Sort = null,
                    Url = Url
                };
                JobRepository.Add(job);
                WriteObject(job);
                ThreadPool.QueueUserWorkItem(o => job.ProcessJob());
            }
            else
            {
                try
                {
                    WriteObject(Session.Reddit.GetPost(new Uri(Url)));
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "CantGetPost", ErrorCategory.InvalidOperation, Url));
                }
            }
        }

        private void ByInputObject()
        {
            if (AsJob)
            {
                job = new GetPostJob
                {
                    Subreddit = inputObject,
                    Limit = Limit,
                    Name = "Get-Post",
                    Sort = SortBy
                };
                JobRepository.Add(job);
                WriteObject(job);
                ThreadPool.QueueUserWorkItem(o => job.ProcessJob());
            }
            else
            {
                try
                {
                    switch (SortBy.ToLower())
                    {
                        case "hot":
                            WriteObject(InputObject.Hot.GetListing(Limit, 100),true);
                            break;
                        case "new":
                            WriteObject(InputObject.New.GetListing(Limit, 100),true);
                            break;
                        case "controversial":
                            // todo: add this later
                            break;
                        case "gilded":
                            // todo: add this later
                            break;
                        case "rising":
                            WriteObject(InputObject.Rising.GetListing(Limit, 100),true);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "CantGetPost", ErrorCategory.InvalidOperation, Url));
                }
            }
        }

        private void ByName()
        {
            inputObject = GetSubreddit();
            ByInputObject();
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
                Session.Log.Error("Unable to get subreddit", ex);
                throw ex;
            }
        }

    }

    internal class GetPostJob : Job
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

        public string Sort { get; set; }

        public string Url { get; set; }


        internal void ProcessJob()
        {
            SetJobState(JobState.Running);

            if (!String.IsNullOrEmpty(Sort))
            {
                switch (Sort.ToLower())
                {
                    case "hot":
                        Output.Add(PSObject.AsPSObject(Subreddit.Hot.GetListing(Limit, 100)));
                        break;
                    case "new":
                        Output.Add(PSObject.AsPSObject(Subreddit.New.GetListing(Limit, 100)));
                        break;
                    case "controversial":
                        // todo: add this later
                        break;
                    case "gilded":
                        // todo: add this later
                        break;
                    case "rising":
                        Output.Add(PSObject.AsPSObject(Subreddit.Rising.GetListing(Limit, 100)));
                        break;
                    default:
                        break;
                }
            }
            else if (!String.IsNullOrEmpty(Url))
            {
                try
                {
                    Output.Add(PSObject.AsPSObject(Session.Reddit.GetPost(new Uri(Url))));
                }
                catch (Exception ex)
                {
                    Output.Add(PSObject.AsPSObject(new ErrorRecord(ex, "CantGetPost", ErrorCategory.InvalidOperation, Url)));
                }
            }

            Output.Complete();
            SetJobState(JobState.Completed);
        }
    }
}
