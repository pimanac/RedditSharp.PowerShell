using System;
using System.Management.Automation;
using RedditSharp.Things;


namespace RedditSharp.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="description">Return a subreddit by name.</para>
    /// <example>
    ///    <code>Get-Subreddit "example"</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Get,"Subreddit")]
    [OutputType(typeof(Subreddit))]
    public class GetSubreddit : Cmdlet
    {
        /// <summary>
        /// <para type="description">Subreddit name.</para>
        /// </summary>
        [Parameter(Mandatory=true,Position = 0,HelpMessage = "Subreddit name")]
        public string Name { get; set; }

        protected override void BeginProcessing()
        {
            if (Session.Reddit == null)
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "NoRedditSession",
                    ErrorCategory.InvalidOperation, Session.Reddit));

            if (this.Name.StartsWith("/r/"))
                this.Name = this.Name.Remove(0, 3);

            if (this.Name.StartsWith("r/"))
                this.Name = this.Name.Remove(0, 2);
        }

        protected override void ProcessRecord()
        {
            var sub = Session.Reddit.GetSubreddit(this.Name);
            // long term cache
            Session.AddCacheItem("xsub_" + sub.Name,sub);
            if (sub == null)
                WriteError(new ErrorRecord(new InvalidOperationException("Invalid subreddit name"), "InvalidSubreddit",
                    ErrorCategory.InvalidOperation, null));
            else
                WriteObject(sub);
        }
    }
}
