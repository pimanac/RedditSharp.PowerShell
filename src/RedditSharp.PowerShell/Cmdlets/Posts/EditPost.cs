using System;
using System.Management.Automation;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets.Posts
{
    /// <summary>
    /// <para type="description">Edit the text of a self post.  Will not work on link posts.</para>
    /// <example>
    ///    <code>Get-Post -</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsData.Edit,"Post")]
    [OutputType(typeof(Post))]
    public class EditPost : Cmdlet
    {
        /// <summary>
        /// Self post to edit.
        /// </summary>
        [Parameter(Mandatory=true,Position = 0,HelpMessage = "Self post to edit")]
        public Post InputObject { get; set; }

        /// <summary>
        /// Replacement body text.  Supports markdown.
        /// </summary>
        [Parameter(Mandatory = true,Position = 1,HelpMessage = "New body text/markdown.")]
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
                InputObject.SelfText = Body;
                WriteObject(InputObject);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CantUpdateSelfText", ErrorCategory.InvalidOperation, InputObject));
            }
        }
    }
}
