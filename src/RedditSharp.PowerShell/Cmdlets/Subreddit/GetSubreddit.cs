using System;
using System.Management.Automation;
using RedditSharp.Things;


namespace RedditSharp.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get,"Subreddit")]
    [OutputType(typeof(Subreddit))]
    public class GetSubreddit : Cmdlet
    {
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
