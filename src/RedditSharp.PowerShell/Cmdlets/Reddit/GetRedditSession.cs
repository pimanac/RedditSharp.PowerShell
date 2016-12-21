using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace RedditSharp.PowerShell.Cmdlets.Reddit
{
    /// <summary>
    /// <para type="description">Get the current reddit session.</para>
    /// <example>
    ///    <code>Get-RedditSession</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsCommon.Get,"RedditSession")]
    [OutputType(typeof(RedditSharp.Reddit))]
    public class GetRedditSession : Cmdlet
    {
        protected override void ProcessRecord()
        {
            if (Session.Reddit == null)
                ThrowTerminatingError(new ErrorRecord(new InvalidOperationException("No reddit session has been started."),
                    "InvalidOperation", ErrorCategory.InvalidOperation, null));
            else
               WriteObject(Session.Reddit);
        }
    }
}
