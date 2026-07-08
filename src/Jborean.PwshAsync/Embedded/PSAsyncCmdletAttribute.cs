#nullable enable
namespace Jborean.PwshAsync
{
    [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
    [global::System.AttributeUsage(global::System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class PSAsyncCmdletAttribute : global::System.Attribute
    {
        public PSAsyncCmdletAttribute(string verb, string noun)
        {
            Verb = verb;
            Noun = noun;
        }

        public string Verb { get; }
        public string Noun { get; }

        // Properties matching System.Management.Automation.CmdletAttribute
        public global::System.Management.Automation.ConfirmImpact ConfirmImpact { get; set; }
        public string? DefaultParameterSetName { get; set; }
        public string? HelpUri { get; set; }
        public global::System.Management.Automation.RemotingCapability RemotingCapability { get; set; }
        public bool SupportsPaging { get; set; }
        public bool SupportsShouldProcess { get; set; }
        // PowerShell always sets this to false in 7.x so we don't support it here.
        // public bool SupportsTransactions { get; set; }
    }
}
