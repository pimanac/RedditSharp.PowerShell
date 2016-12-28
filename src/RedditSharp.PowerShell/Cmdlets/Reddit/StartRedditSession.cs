using System;
using System.Management.Automation;


namespace RedditSharp.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Start a reddit session with the specified credentials</para>
    /// <para type="description">Start a reddit session using the specified credentials.</para>
    /// <para type="description">Only OAUTH clients are supported.  See https://github.com/reddit/reddit/wiki/OAuth2 for more info.</para>
    /// <param type="description">See https://www.reddit.com/prefs/apps/ to configure oauth for your account.</param>
    /// <example>
    ///    <code>Start-RedditSession -UserName</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Start,"RedditSession")]
    [OutputType(typeof(RedditSharp.Reddit))]
    public class StartRedditSession : Cmdlet
    {
        /// <summary>
        /// <para type="description">Reddit username.</para>
        /// </summary>
        [Parameter(Mandatory = true,Position = 0,HelpMessage = "Reddit username")]
        public string UserName { get; set; }

        /// <summary>
        /// <para type="description">Reddit password.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Reddit password")]
        public string Password { get; set; }

        /// <summary>
        /// <para type="description">Reddit Client Id.</para>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Reddit app key id")]
        public string ClientId { get; set; }

        /// <summary>
        /// <para type="description">Reddit Client Secret.</para>
        /// <param type="description">See https://www.reddit.com/prefs/apps/ for more info.</param>
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Reddit app secret")]
        public string Secret { get; set; }

        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Redirect URL")]
        public string RedirectUrl { get; set; }

        protected override void BeginProcessing()
        {
            try
            {
                Session.WebAgent = new BotWebAgent(UserName, Password, ClientId, Secret,RedirectUrl);
                Session.Reddit = new RedditSharp.Reddit(Session.WebAgent, true);
                Session.AuthenticatedUser = Session.Reddit.User;
                Session.Start();
                WriteObject(Session.Reddit);
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(ex, "UnableToConnectToReddit", ErrorCategory.OpenError, null));
            }
        }

        protected override void ProcessRecord()
        {
            if (Session.AuthenticatedUser != null)
            {
                WriteObject("Connected to reddit.");
            }
            else
                ThrowTerminatingError(new ErrorRecord(new Exception("Unknown reddit error - user is null"),
                    "UnknownError",
                    ErrorCategory.NotSpecified, Session.Reddit));
        }

        protected override void StopProcessing()
        {
            Session.Cache = null;
            Session.WebAgent = null;
            Session.Reddit = null;
            Session.AuthenticatedUser = null;
        }
        

        public StartRedditSession()
        {
            this.RedirectUrl = String.Empty;
        }
    }
}
