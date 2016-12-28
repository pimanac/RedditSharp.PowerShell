using System;
using System.Management.Automation;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="description">Return a RedditUser by name.</para>
    /// <example>
    ///    <code>Get-RedditUser -Name example</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "RedditUser")]
    [OutputType(typeof(RedditUser))]
    public class GetRedditUser : Cmdlet
    {
        /// <summary>
        /// Reddit username.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Reddit username")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

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
                WriteObject(Session.Reddit.GetUser(CleanName()));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CantGetUser", ErrorCategory.InvalidOperation, Session.Reddit));
                throw;
            }
        }

        private string CleanName()
        {
            if (Name.StartsWith("/u/"))
                return Name.Remove(0, 3);

            if (Name.StartsWith("u/"))
                return Name.Remove(0, 2);

            if (Name.StartsWith("user/"))
                return Name.Remove(0, 5);

            if (Name.StartsWith("/user/"))
                return Name.Remove(0, 6);

            return Name;
        }
    }
}
