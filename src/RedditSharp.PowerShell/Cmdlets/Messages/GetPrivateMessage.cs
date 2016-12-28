using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="synopsis">Get a list of private messages.  Also returns modmail.</para>
    /// <para type="description">Get a list of private messsages and/or modmail with optional filtering.</para>
    /// <para type="description">Get a list of private messsages and/or modmail with optional filtering.</para>
    /// </summary>
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

        /// <summary>
        /// <para type="description">Return only unread messages.</para>
        /// </summary>
        [Parameter(HelpMessage = "Return only unread messages.")]
        public SwitchParameter Unread { get; set; }

        /// <summary>
        /// <para type="description">Return modmail instead of Private Messages.</para>
        /// </summary>
        [Parameter(Mandatory = false,
                   HelpMessage = "Filter subreddit modmail")]
        public SwitchParameter Modmail { get; set; }

        /// <summary>
        /// <para type="description">Filter by these users.</para>
        /// </summary>
        [Parameter(Mandatory = false,
                   HelpMessage = "Return messages from users in this list.")]
        public string[] UserFilter { get; set; }

        /// <summary>
        /// <para type="description">Filter by these subreddits.</para>
        /// </summary>
        [Parameter(Mandatory = false,
                   HelpMessage = "Return messages from subreddits in this list.")]
        public string[] SubredditFilter { get; set; }

        /// <summary>
        /// <para type="description">Maximum number of items to fetch from the listing.  Default: 1000</para>
        /// </summary>
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
            WriteVerbose("Getting Modmail");
            if (Unread.IsPresent)
            {
                WriteDebug("Unread flag is set");
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
                return true;
            }

            // don't return things that aren't messages
            var theMessage = message as PrivateMessage;
            if (theMessage == null)
            {
                return false;
            }

            // user filter only
            if ((userFilter != null && userFilter.Length > 0) && subFilter == null)
            {
                result = Array.IndexOf(userFilter, theMessage.Author) > 0;

                return result;
            }

            // subreddit filter only
            if ((subFilter != null && subFilter.Length > 0) && userFilter == null)
            {
                result = Array.IndexOf(subFilter, theMessage.Subreddit) > 0;

                return result;
            }

            result = (Array.IndexOf(userFilter, theMessage.Author) > 0) &&
                         (Array.IndexOf(subFilter, theMessage.Subreddit) > 0);

            // filter on both subreddit and user
            return result;
        }
    }
}
