using System;
using System.Management.Automation;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Compose a new message.</para>
    /// <para type="description">Compose a private message or modmail message.</para>
    /// <example>
    ///    <code>Get-User "example" | New-PrivateMessage -Subject "Hello" -Body" Welcome to the subreddit!"</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.New, "PrivateMessage")]
    [OutputType(typeof(bool))]
    public class NewPrivateMessage : PSCmdlet, IDynamicParameters
    {
        /// <summary>
        /// <para type="description">Reddit user to whom to send the message.</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByInputObject",
            Position = 0,
            ValueFromPipeline = true,
            HelpMessage = "Reddit User to whom to send the message.")]
        public RedditUser InputObject { get; set; }

        /// <summary>
        /// <para type="description">Reddit user to whom to send the message.</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByUserName", Position = 0, HelpMessage = "Reddit username to whome to send the message.")]
        public string To { get; set; }

        /// <summary>
        /// <para type="description">Subject of the message.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 1,HelpMessage = "Subject of the message.")]
        public string Subject { get; set; }

        /// <summary>
        /// <para type="description">Body of the message.  Supports markdown.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 2,HelpMessage = "Body text/markdown.")]
        public string Body { get; set; }

        /// <summary>
        /// <para type="description">Compose the message as this subreddit.  (modmail)</para>
        /// </summary>
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

        public object GetDynamicParameters()
        {
            throw new NotImplementedException();
        }
    }
}
