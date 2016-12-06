using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using RedditSharp.Things;

namespace RedditSharp.PowerShell.Cmdlets.Posts
{
    /// <summary>
    /// <para type="description">Edit the text of a self post.  Will not work on link posts.</para>
    /// </summary>
    [Cmdlet(VerbsData.Edit,"Post")]
    [OutputType(typeof(Post))]
    public class EditPost : Cmdlet
    {
        [Parameter(Mandatory=true,Position = 0,HelpMessage = "Self post to edit")]
        public Post Target { get; set; }

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
                Target.EditText(Body);
                Target.SelfText = Body;
                WriteObject(Target);
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "CantUpdateSelfText", ErrorCategory.InvalidOperation, Target));
            }
        }
    }
}
