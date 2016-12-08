using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Returns a ModerationLog listing in descending order by date.</para>
    /// <para type="description">User must have the 'modlog' scope.</para>
    /// <para type="description">By default limit to 1000 records.  Set -Limit to -1 for unlimited.</para>
    /// <para type="description">Optionally filter [server side] by moderator name and/or action type.</para>
    /// <example>
    ///    <para>Get the unfiltered modlog</para>
    ///    <code>Get-ModerationLog -Subreddit example</code>
    /// </example>
    /// <example>
    ///    <para>Get a filtered modlog by moderators</para>
    ///    <code>Get-Subreddit "example" | Get-ModerationLog -Moderators @('foo','bar')</code>
    /// </example>
    /// <example>
    ///    <para>Get a filtered modlog by action</para>
    ///    <code>Get-Modlog -Subreddit "example" -Action RemoveComment</code>
    /// </example>
    /// <example>
    ///    <para>Get a filtered modlog</para>
    ///    <code>Get-Modlog -Subreddit "example" -Action ApproveComment -Mods @('foo') | % { Send-PrivateMessage -To $_.TargetAuthorName -Subject 'Hi' -Body 'foo approved your comment!' } </code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "ModerationLog")]
    [OutputType(typeof(IEnumerable<ModAction>))]
    [CmdletBinding]
    public class GetModerationLog : PSCmdlet
    {
        /// <summary>
        /// <para type="description">Get the Moderation log for this subreddit.</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByTarget",
             Mandatory = false,
             ValueFromPipeline = true,
             HelpMessage = "Target Subreddit.")]
        [ValidateNotNullOrEmpty]
        public Subreddit Target { get; set; }

        /// <summary>
        /// <para type="description">Get the Moderation for this subreddit.</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByName", Mandatory = true, Position = 0, HelpMessage = "Subreddit name.")]
        [Alias("Sub")]
        public string Subreddit { get; set; }

        /// <summary>
        /// <para type="description">Filter by this ActionType</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, HelpMessage = "Filter by action (reddit server side).")]
        [ValidateNotNullOrEmpty]
        public ModActionType? Action { get; set; }

        /// <summary>
        /// <para type="description">Filter by these moderators</para>
        /// </summary>
        [Parameter(Mandatory = false, Position = 2, HelpMessage = "Filter by action (reddit server side).")]
        [Alias("Mods", "Mod")]
        [ValidateNotNullOrEmpty]
        public string[] Moderators { get; set; }


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

        private GetModerationLogJob job;

        public GetModerationLog()
        {
            ModAction a = new ModAction();
            // a.TargetAuthorName
            this.Limit = 1000;
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
                    WriteObject(DoWork(),true);
                }
                catch (Exception ex)
                {
                    Session.Log.Error("Unable to get modlog", ex);
                    WriteError(new ErrorRecord(ex, "CantGetModlog", ErrorCategory.InvalidOperation, null));
                }
            }
            else
            {
                job = new GetModerationLogJob();
                job.Subreddit = Target;
                job.Action = Action;
                job.Moderators = Moderators;
                job.Limit = Limit;
                job.Name = "Get-ModerationLog";
                JobRepository.Add(job);
                WriteObject(job);
                ThreadPool.QueueUserWorkItem(o => job.ProcessJob());
            }

        }

        private IEnumerable<ModAction> DoWork()
        {
            if (ParameterSetName == "ByName")
                Target = GetSubreddit();

            if (Action != null && Moderators != null && Moderators.Length > 0)
                return GetModLog(Action.Value, Moderators);

            if (Action != null && Moderators == null)
                return GetModLog(Action.Value);

            if (Action == null && Moderators != null && Moderators.Length > 0)
                return GetModLog(Moderators);

            if (Action == null && Moderators == null)
                return GetModLog();

            Session.Log.Error("Unable to get listing");
            return null;
        }

        private IEnumerable<ModAction> GetModLog()
        {
            try
            {
                return Target.GetModerationLog().GetListing(Limit, 100);
            }
            catch (Exception ex)
            {
                Session.Log.Error("Unable to get modlog", ex);
                throw ex;
            }
        }

        private IEnumerable<ModAction> GetModLog(ModActionType action)
        {
            try
            {
                return Target.GetModerationLog(action).GetListing(Limit, 100);
            }
            catch (Exception ex)
            {
                Session.Log.Error("Unable to get modlog", ex);
                throw ex;
            }
        }

        private IEnumerable<ModAction> GetModLog(string[] moderators)
        {
            try
            {
                return Target.GetModerationLog(moderators).GetListing(Limit, 100);
            }
            catch (Exception ex)
            {
                Session.Log.Error("Unable to get modlog", ex);
                throw ex;
            }
        }

        private IEnumerable<ModAction> GetModLog(ModActionType action, string[] moderators)
        {
            try
            {
                return Target.GetModerationLog(action, moderators).GetListing(Limit, 100);
            }
            catch (Exception ex)
            {
                Session.Log.Error("Unable to get modlog", ex);
                throw ex;
            }
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

    internal class GetModerationLogJob : Job
    {
        public override void StopJob()
        {
            throw new NotImplementedException();
        }

        public override string StatusMessage { get; }
        public override bool HasMoreData { get; }
        public override string Location { get; }

        public Subreddit Subreddit { get; set; }
        public string[] Moderators { get; set; }
        public ModActionType? Action { get; set; }
        public int Limit { get; set; }



        internal void ProcessJob()
        {
            SetJobState(JobState.Running);

            if (Action != null && Moderators != null && Moderators.Length > 0)
                GetModLog(Action.Value, Moderators);

            if (Action != null && Moderators == null)
                GetModLog(Action.Value);

            if (Action == null && Moderators != null && Moderators.Length > 0)
                GetModLog(Moderators);

            if (Action == null && Moderators == null)
                GetModLog();

            Output.Complete();
            SetJobState(JobState.Completed);
        }
        
        private void GetModLog()
        {
            try
            {
                Output.Add(PSObject.AsPSObject(Subreddit.GetModerationLog().GetListing(Limit, 100).ToList()));
            }
            catch (Exception ex)
            {
                Session.Log.Error("Unable to get modlog", ex);
                throw ex;
            }
        }

        private void GetModLog(ModActionType action)
        {
            try
            {
                Output.Add(PSObject.AsPSObject(Subreddit.GetModerationLog(action).GetListing(Limit, 100).ToList()));
            }
            catch (Exception ex)
            {
                Session.Log.Error("Unable to get modlog", ex);
                throw ex;
            }
        }

        private void GetModLog(string[] moderators)
        {
            try
            {
                Output.Add(PSObject.AsPSObject(Subreddit.GetModerationLog(moderators).GetListing(Limit, 100).ToList()));
            }
            catch (Exception ex)
            {
                Session.Log.Error("Unable to get modlog", ex);
                throw ex;
            }
        }

        private void GetModLog(ModActionType action, string[] moderators)
        {
            try
            {
                Output.Add(PSObject.AsPSObject(Subreddit.GetModerationLog(action, moderators).GetListing(Limit, 100).ToList()));
            }
            catch (Exception ex)
            {
                Session.Log.Error("Unable to get modlog", ex);
                throw ex;
            }
        }
    }
}
