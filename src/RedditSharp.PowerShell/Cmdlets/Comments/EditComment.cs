using System;
using System.Management.Automation;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets.Comments
{
    /// <summary>
    /// <para type="description">Edit an existing comment.</para>
    /// </summary>
    [Cmdlet(VerbsData.Edit,"Comment")]
    [OutputType(typeof(Comment))]
    public class EditComment : Cmdlet
    {
        /// <summary>
        /// <para type="description">Comment to edit.</para>
        /// </summary>
        [Parameter(ParameterSetName = "ByInputObject", Mandatory = true, Position = 0, ValueFromPipeline = true,
             HelpMessage = "Comment to edit")]
        [ValidateNotNullOrEmpty]
        public Comment InputObject { get; set; }

        /// <summary>
        /// <para type="description">New comment body.  Supports markdown.</para>
        /// </summary>
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
                WriteVerbose($"Edited {InputObject.FullName}");
                WriteObject(InputObject);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CantEditComment",
                    ErrorCategory.InvalidOperation, InputObject));
            }
        }
    }
}
