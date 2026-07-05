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
        public int ConfirmImpact { get; set; }
        public string? DefaultParameterSetName { get; set; }
        public string? HelpUri { get; set; }
        public int RemotingCapability { get; set; }
        public bool SupportsPaging { get; set; }
        public bool SupportsShouldProcess { get; set; }
        public bool SupportsTransactions { get; set; }
    }
}
