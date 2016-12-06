using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "RedditUser")]
    [OutputType(typeof(RedditUser))]
    public class GetRedditUser : Cmdlet
    {
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
