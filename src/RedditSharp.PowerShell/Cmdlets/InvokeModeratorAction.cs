﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets
{
    [Cmdlet(VerbsLifecycle.Invoke, "ModeratorAction")]
    public class InvokeModeratorAction : PSCmdlet, IDynamicParameters
    {
        private DistinguishTypeDynamicParameter distinguishTypeContext;
        private BanDynamicParameter banContext;
        private UnbanDynamicParameter unbanContext;
        private FlairDynamicParameter flairContext;

        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Action to perform")]
        [ValidateSet(new[] { "Approve", "Remove", "Ban", "Unban", "Flair", "Unflair", "Distinguish" }, IgnoreCase = true)]
        public string Action { get; set; }

        [Parameter(Mandatory = false, Position = 1, ValueFromPipeline = true, HelpMessage = "Target RedditSharp.Thing")]
        public Thing Target { get; set; }

        protected override void BeginProcessing()
        {
            if (Session.Reddit == null)
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "NoRedditSession",
                    ErrorCategory.InvalidOperation, Session.Reddit));
        }

        protected override void ProcessRecord()
        {
            switch (Action)
            {
                case "Approve":
                    Approve();
                    break;
                case "Remove":
                    Remove();
                    break;
                case "Ban":
                    Ban();
                    break;
                case "Unban":
                    Unban();
                    break;
                case "Flair":
                    Flair(flairContext.Text, flairContext.CssClass);
                    break;
                case "Unflair":
                    Flair("", "");
                    break;
                case "Distinguish":
                    Distinguish();
                    break;
            }
        }

        private void Ban()
        {
            if (banContext != null)
            {
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
                    Session.Log.Info($"Banned user {banContext.User} in subreddit {banContext.Subreddit}");
                }
                catch (Exception ex)
                {
                    Session.Log.Error("Could not ban user", ex);
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
                Session.Log.Info($"Unban user {banContext.User} in subreddit {banContext.Subreddit}");
            }
            catch (Exception ex)
            {
                Session.Log.Error("Could not unban user", ex);
                WriteError(new ErrorRecord(ex, "UnableToDoModAction", ErrorCategory.InvalidOperation, null));
            }
        }

        private void Flair(string text,string cssClass)
        {
            if (Target.Kind != "t3")
            {
                Session.Log.Warn("Cannot flair this this");
                WriteError(new ErrorRecord(
                            new InvalidOperationException("Cannot flair this Thing."),
                        "CannotFlair",
                        ErrorCategory.InvalidOperation,
                        Target)
                );
            }
            else
            {
                try
                {
                    var p = Target as Post;
                    p.SetFlair(text, cssClass);
                    Session.Log.Debug($"Set flair on {p.FullName} to {text}  /  {cssClass}");
                    WriteObject(Target);
                }
                catch (Exception ex)
                {
                    Session.Log.Error("Could not set flair");
                    WriteError(new ErrorRecord(ex, "CannotFlair", ErrorCategory.InvalidOperation, Target));
                }
            }
        }

        private void Remove()
        {
            try
            {
                Comment c;
                Post p;
                c = Target as Comment;

                if (c != null)
                {
                    c.Remove();
                }
                else
                {
                    p = Target as Post;
                    p.Remove();
                }
                Session.Log.Debug($"Removed {Target.FullName}");
                WriteObject(Target);
            }
            catch (Exception ex)
            {
                Session.Log.Error($"Could not remove {Target.FullName}",ex);
                WriteError(new ErrorRecord(ex, "This item cannot be removed.", ErrorCategory.InvalidOperation, Target));
            }
        }

        private void Approve()
        {
            try
            {
                Comment c;
                Post p;
                c = Target as Comment;

                if (c != null)
                {
                    c.Approve();
                }
                else
                {
                    p = Target as Post;
                    p.Approve();
                }
                Session.Log.Debug($"Approved {Target.FullName}");
                WriteObject(Target);
            }
            catch (Exception ex)
            {
                Session.Log.Error($"Could not approve {Target.FullName}",ex);
                WriteError(new ErrorRecord(ex, "This item cannot be approved.", ErrorCategory.InvalidOperation, Target));
            }
        }

        private void Distinguish()
        {
            try
            {
                var t = Target as VotableThing;
                t.Distinguish(distinguishTypeContext.DistinguishType);
                Session.Log.Debug($"Distinguished {Target.FullName} as {distinguishTypeContext.DistinguishType}");
                WriteObject(Target);
            }
            catch (NullReferenceException ex)
            {
                Session.Log.Error($"Could not distinguish {Target.FullName}");
                WriteError(new ErrorRecord(ex, "This item cannot be distinguished.", ErrorCategory.InvalidOperation, Target));
            }
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
                    break;
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