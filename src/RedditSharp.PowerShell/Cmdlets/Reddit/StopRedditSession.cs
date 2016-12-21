using System.Management.Automation;
using log4net;

namespace RedditSharp.PowerShell.Cmdlets
{
    /// <summary>
    /// <para type="description">Disconnect from reddit and clear your session.</para>
    /// <example>
    ///    <code>Stop-RedditSession</code>
    /// </example>
    /// </summary>
    [Cmdlet(VerbsLifecycle.Stop, "RedditSession")]
    [OutputType(typeof(RedditSharp.Reddit))]
    public class StopRedditSession : Cmdlet
    {
        protected override void ProcessRecord()
        {
            Session.Cache = null;
            Session.WebAgent = null;
            Session.Reddit = null;
        }
    }
}
