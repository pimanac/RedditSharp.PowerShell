using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Security.Cryptography.X509Certificates;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "PrivateMessage")]
    [OutputType(typeof(PrivateMessage))]
    [OutputType(typeof(IEnumerable<PrivateMessage>))]
    public class GetPrivateMessage : PSCmdlet
    {
        [Parameter(ParameterSetName = "ByInputObject",
                   HelpMessage = "Target object",
                   ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public Thing InputObject { get; set; }

        [Parameter(HelpMessage = "Return only unread messages.")]
        public SwitchParameter Unread { get; set; }

        [Parameter(Mandatory = false,
                   HelpMessage = "Filter subreddit modmail")]
        public SwitchParameter Modmail { get; set; }

        [Parameter(Mandatory = false,
                   HelpMessage = "Return messages from users in this list.")]
        public string[] UserFilter { get; set; }

        [Parameter(Mandatory = false,
                   HelpMessage = "Return messages from subreddits in this list.")]
        public string[] SubredditFilter { get; set; }

        [Parameter(Mandatory = false,
                   HelpMessage = "Maximum number of items to fetch from the listing.  Default: 1000")
        ]
        public int Limit { get; set; }

        public GetPrivateMessage()
        {
            this.Limit = 1000;
            this.UserFilter = null;
            this.SubredditFilter = null;
        }

        protected override void BeginProcessing()
        {
            if (Session.Reddit == null)
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException(), "NoRedditSession",
                    ErrorCategory.InvalidOperation, Session.Reddit));
        }

        protected override void ProcessRecord()
        {
            try
            {
                if (Modmail.IsPresent)
                    DoGetModmail();
                else
                    DoGetMessages();
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CantGetUnread", ErrorCategory.InvalidOperation, Session.Reddit));
            }
        }

        /// <summary>
        /// Grab items that match the filter.
        /// </summary>
        private void DoGetMessages()
        {
            if (Unread.IsPresent)
            {
                // Unread messages from inbox
                WriteObject(Session.AuthenticatedUser.UnreadMessages
                        .Where(x => MessageFilter(x, UserFilter, SubredditFilter))
                        .Take(Limit),
                    true);
            }
            else
            {

                // all messages from inbox
                WriteObject(Session.AuthenticatedUser.Inbox
                        .Where(x => MessageFilter(x, UserFilter, SubredditFilter))
                        .Take(Limit),
                    true);
            }
        }

        /// <summary>
        /// Grab items that match the filter.
        /// </summary>
        private void DoGetModmail()
        {
            Session.Log.Debug("Getting Modmail");
            if (Unread.IsPresent)
            {
                Session.Log.Debug("Unread flag is set");
                // Unread messages from inbox
                WriteObject(Session.AuthenticatedUser.ModMail
                        .Where(x => MessageFilter(x, UserFilter, SubredditFilter))
                        .Where(x => x.Unread)
                        .Take(Limit),
                    true);
            }
            else
            {
                WriteObject(Session.AuthenticatedUser.ModMail
                        .Where(x => MessageFilter(x, UserFilter, SubredditFilter))
                        .Where(x => x.Unread)
                        .Take(Limit),
                    true);
            }
        }

        /// <summary>
        ///filters private messages
        /// </summary>
        /// <param name="message">PrivateMessage to check</param>
        /// <param name="userFilter">list of users to filter.  null assumes we're not filtering on this property</param>
        /// <param name="subFilter">list of subreddits to filter.  null assumes we're not filtering on this property</param>
        /// <returns></returns>
        private static bool MessageFilter(Thing message, string[] userFilter = null, string[] subFilter = null)
        {
            bool result;
            if (userFilter == null && subFilter == null)
            {
                Session.Log.Debug("No message filter");
                return true;
            }

            // don't return things that aren't messages
            var theMessage = message as PrivateMessage;
            if (theMessage == null)
            {
                Session.Log.Warn("Reddit returned something that wasn't a PrivateMessage");
                return false;
            }

            // user filter only
            if ((userFilter != null && userFilter.Length > 0) && subFilter == null)
            {
                result = Array.IndexOf(userFilter, theMessage.Author) > 0;

                if (result)
                    Session.Log.Debug("PrivateMessage matches userFilter : " + theMessage.Author);

                return result;
            }

            // subreddit filter only
            if ((subFilter != null && subFilter.Length > 0) && userFilter == null)
            {
                result = Array.IndexOf(subFilter, theMessage.Subreddit) > 0;

                if (result)
                    Session.Log.Debug("PrivateMessage matches subredditFilter : " + theMessage.Subreddit);

                return result;
            }

            result = (Array.IndexOf(userFilter, theMessage.Author) > 0) &&
                         (Array.IndexOf(subFilter, theMessage.Subreddit) > 0);

            if (result)
                Session.Log.Debug("PrivateMessage matches userFilter : " + theMessage.Author + "  /  subredditFilter : " + theMessage.Subreddit);

            // filter on both subreddit and user
            return result;
        }

    }
}
