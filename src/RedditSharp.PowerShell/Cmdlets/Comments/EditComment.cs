using System;
using System.Management.Automation;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets.Comments
{
    [Cmdlet(VerbsData.Edit,"Comment")]
    [OutputType(typeof(Comment))]
    public class EditComment : Cmdlet
    {
        [Parameter(ParameterSetName = "ByInputObject", Mandatory = true, Position = 0, ValueFromPipeline = true,
             HelpMessage = "Comment to edit")]
        [ValidateNotNullOrEmpty]
        public Comment InputObject { get; set; }

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
                InputObject.EditText(Body);
                InputObject.Body = Body;
                Session.Log.Debug($"Edited {InputObject.FullName}");
                WriteObject(InputObject);
            }
            catch (Exception ex)
            {
                Session.Log.Error($"Could not edit {InputObject.FullName}");
                WriteError(new ErrorRecord(ex, "CantEditComment",
                    ErrorCategory.InvalidOperation, InputObject));
            }
        }
    }
}
