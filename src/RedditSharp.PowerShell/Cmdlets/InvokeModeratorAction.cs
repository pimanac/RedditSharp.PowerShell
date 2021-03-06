﻿using System;
using System.Management.Automation;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="description">Perform a mod action on an item.</para>
    /// <example>
    ///    <code>$comment | Invoke-ModeratorAction -Action Flair -Text "good content" -CssClass "my-css-class" </code>
    /// </example>
    /// <example>
    ///    <code>Get-Subreddit "example" |  Get-Post -Sort Hot -Limit 10 | % { $_ | Invoke-Moderator-Action Approve }</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Invoke, "ModeratorAction",
        ConfirmImpact = ConfirmImpact.Medium,
        SupportsShouldProcess = true
    )]
    [OutputType(typeof(VotableThing))]
    public class InvokeModeratorAction : PSCmdlet, IDynamicParameters
    {
        private DistinguishTypeDynamicParameter distinguishTypeContext;
        private BanDynamicParameter banContext;
        private UnbanDynamicParameter unbanContext;
        private FlairDynamicParameter flairContext;

        /// <summary>
        /// Action to perform on the item.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Action to perform")]
        [ValidateSet(new[] { "Approve", "Remove", "Ban", "Unban", "Flair", "Unflair", "Distinguish", "IgnoreReports", "UnIgnoreReports" }, IgnoreCase = true)]
        public string Action { get; set; }

        /// <summary>
        /// Item to action.  Must be a Comment, Post or User.
        /// </summary>
        [Parameter(Mandatory = false, Position = 1, ValueFromPipeline = true, HelpMessage = "Target RedditSharp.Thing")]
        public VotableThing InputObject { get; set; }

        protected override void BeginProcessing()
        {
            if (Session.Reddit == null)
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "NoRedditSession",
                    ErrorCategory.InvalidOperation, Session.Reddit));
        }

        protected override void ProcessRecord()
        {
            switch (Action.ToLower())
            {
                case "approve":
                    Approve();
                    break;
                case "remove":
                    Remove();
                    break;
                case "ban":
                    Ban();
                    break;
                case "unban":
                    Unban();
                    break;
                case "flair":
                    Flair(flairContext.Text, flairContext.CssClass);
                    break;
                case "unflair":
                    Flair("", "");
                    break;
                case "distinguish":
                    Distinguish();
                    break;
                case "ignorereports":
                    IgnoreReports();
                    break;
                case "unignorereports":
                    UnIngoreReports();
                    break;
            }
        }

        private void Ban()
        {
            if (banContext != null)
            {
                if (!ShouldProcess(banContext.User, "Ban user"))
                    return;

                Subreddit sub;
                try
                {
                    sub = Session.GetCacheItem<Subreddit>("xsub_" + banContext.Subreddit);
                    if (sub == null)
                        sub = Session.Reddit.GetSubreddit(banContext.Subreddit);

                    if (ParameterSetName == "BanMessage")
                        sub.BanUser(banContext.User, banContext.Reason, banContext.Note, banContext.Duration,
                            banContext.Message);
                    else
                        sub.BanUser(banContext.User, banContext.Note);
                    WriteVerbose($"Banned user {banContext.User} in subreddit {banContext.Subreddit}");
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "CantInvokeModeAction", ErrorCategory.InvalidOperation, null));
                }
            }
        }

        private void Unban()
        {
            try
            {
                Subreddit sub;
                sub = Session.GetCacheItem<Subreddit>("xsub_" + banContext.Subreddit);
                if (sub == null)
                    sub = Session.Reddit.GetSubreddit(banContext.Subreddit);

                sub.UnBanUser(unbanContext.User);
                WriteVerbose($"Unban user {banContext.User} in subreddit {banContext.Subreddit}");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "UnableToDoModAction", ErrorCategory.InvalidOperation, null));
            }
        }

        private void Flair(string text, string cssClass)
        {
            if (InputObject.Kind != "t3")
            {
                WriteError(new ErrorRecord(
                            new InvalidOperationException("Cannot flair this Thing."),
                        "CannotFlair",
                        ErrorCategory.InvalidOperation,
                        InputObject)
                );
            }
            else
            {
                try
                {
                    var p = InputObject as Post;
                    p.SetFlair(text, cssClass);
                    WriteVerbose($"Set flair on {p.FullName} to {text}  /  {cssClass}");
                    WriteObject(InputObject);
                }
                catch (Exception ex)
                {
                    WriteError(new ErrorRecord(ex, "CannotFlair", ErrorCategory.InvalidOperation, InputObject));
                }
            }
        }

        private void Remove()
        {
            if (!ShouldProcess(InputObject.FullName, "Remove Item"))
                return;

            InputObject.Remove();
            WriteVerbose($"Removed {InputObject.FullName}");
            WriteObject(InputObject);
        }

        private void Approve()
        {
            InputObject.Approve();
            WriteVerbose($"Approved {InputObject.FullName}");
            WriteObject(InputObject);
        }

        private void Distinguish()
        {
            InputObject.Distinguish(distinguishTypeContext.DistinguishType);
            WriteVerbose($"Distinguished {InputObject.FullName} as {distinguishTypeContext.DistinguishType}");
            WriteObject(InputObject);
        }

        private void IgnoreReports()
        {
            if (!ShouldProcess(InputObject.FullName, "Ignore Reports"))
                return;

            InputObject.IgnoreReports();
            WriteVerbose($"Ignore reports on {InputObject.FullName}");
            WriteObject(InputObject);
        }

        private void UnIngoreReports()
        {
            InputObject.UnIgnoreReports();
            WriteVerbose($"UnIgnore reports on {InputObject.FullName}");
            WriteObject(InputObject);
        }

        // IDynamicParameters
        public object GetDynamicParameters()
        {
            switch (Action.ToLower())
            {
                case "distinguish":
                    distinguishTypeContext = new DistinguishTypeDynamicParameter();
                    return distinguishTypeContext;
                case "ban":
                    banContext = new BanDynamicParameter();
                    return banContext;
                case "unban":
                    unbanContext = new UnbanDynamicParameter();
                    return unbanContext;
                case "flair":
                    flairContext = new FlairDynamicParameter();
                    return flairContext;
            }
            return null;
        }
    }

    public class DistinguishTypeDynamicParameter
    {
        [Parameter()]
        public VotableThing.DistinguishType DistinguishType { get; set; }
    }

    public class BanDynamicParameter
    {
        [Parameter(Mandatory = true, HelpMessage = "User to ban")]
        [ValidateNotNullOrEmpty]
        public string User { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Subreddit from which to ban the user.")]
        [ValidateNotNullOrEmpty]
        public string Subreddit { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Ban note (not visible to user).")]
        [ValidateNotNullOrEmpty]
        public string Note { get; set; }

        [Parameter(ParameterSetName = "BanMessage", HelpMessage = "Reason for ban (sent to user).")]
        [ValidateNotNullOrEmpty]
        public string Reason { get; set; }

        [Parameter(ParameterSetName = "BanMessage", HelpMessage = "Duration in days to ban the user.")]
        [ValidateNotNullOrEmpty]
        public int Duration { get; set; }

        [Parameter(ParameterSetName = "BanMessage", Mandatory = true, HelpMessage = "Message text/markdown to send to the user.")]
        [ValidateNotNullOrEmpty]
        public string Message { get; set; }
    }

    public class UnbanDynamicParameter
    {
        [Parameter(Mandatory = true, HelpMessage = "User to ban")]
        [ValidateNotNullOrEmpty]
        public string User { get; set; }
    }

    public class FlairDynamicParameter
    {
        [Parameter(Mandatory = true, HelpMessage = "Flair text.")]
        [ValidateNotNullOrEmpty]
        public string Text { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Flair css class.")]
        [ValidateNotNullOrEmpty]
        public string CssClass { get; set; }
    }
}
