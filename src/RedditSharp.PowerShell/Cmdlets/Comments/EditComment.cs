using System;
using System.Management.Automation;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets.Comments
{
    [Cmdlet(VerbsData.Edit,"Comment")]
    [OutputType(typeof(Comment))]
    public class EditComment : Cmdlet
    {
        [Parameter(ParameterSetName = "ByTarget", Mandatory = true, Position = 0, ValueFromPipeline = true,
             HelpMessage = "Comment to edit")]
        [ValidateNotNullOrEmpty]
        public Comment Target { get; set; }

        [Parameter(Mandatory = true, HelpMessage = "Comment text/markdown")]
        [ValidateNotNullOrEmpty]
        public string Body { get; set; }

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
                Target.EditText(Body);
                Target.Body = Body;
                Session.Log.Debug($"Edited {Target.FullName}");
                WriteObject(Target);
            }
            catch (Exception ex)
            {
                Session.Log.Error($"Could not edit {Target.FullName}");
                WriteError(new ErrorRecord(ex, "CantEditComment",
                    ErrorCategory.InvalidOperation, Target));
            }
        }
    }
}
