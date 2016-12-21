using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "PrivateMessage")]
    [OutputType(typeof(bool))]
    public class NewPrivateMessage : PSCmdlet
    {
        [Parameter(ParameterSetName = "ByInputObject",
            Position = 0,
            ValueFromPipeline = true,
            HelpMessage = "Reddit User to which to send the message.")]
        public RedditUser InputObject { get; set; }

        [Parameter(ParameterSetName = "ByUserName", Position = 0, HelpMessage = "Reddit username to which to send the message.")]
        public string To { get; set; }

        [Parameter(Mandatory = true, Position = 1,HelpMessage = "Subject of the message.")]
        public string Subject { get; set; }

        [Parameter(Mandatory = true, Position = 2,HelpMessage = "Body text/markdown.")]
        public string Body { get; set; }

        [Parameter(Mandatory = false, Position = 3,HelpMessage = "Compose the message as a subreddit (modmail).")]
        [ValidateNotNullOrEmpty]
        public string AsSubreddit { get; set; }

        protected override void BeginProcessing()
        {
            if (Session.Reddit == null)
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "NoRedditSession",
                    ErrorCategory.InvalidOperation, Session.Reddit));
        }

        protected override void ProcessRecord()
        {
            if (ParameterSetName == "ByInputObject")
                SendMessage(InputObject.Name);
            else
                SendMessage(To);
        }

        private void SendMessage(string user)
        {
            if (String.IsNullOrEmpty(AsSubreddit))
                Session.Reddit.ComposePrivateMessage(Subject, Body, user);
            else
                Session.Reddit.ComposePrivateMessage(Subject, Body, user, AsSubreddit);
        }
    }
}
