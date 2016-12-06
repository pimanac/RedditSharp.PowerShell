using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Get,"Post")]
    [OutputType(typeof(Post))]
    public class GetPost : PSCmdlet
    {
        [Parameter(Mandatory=true)]
        [ValidateNotNullOrEmpty]
        public string Uri { get; set; }

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
                WriteObject(Session.Reddit.GetPost(new Uri(Uri)));
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CantGetPost", ErrorCategory.InvalidOperation, Uri));
            }
        }


    }
}
