#nullable enable
namespace Jborean.PwshAsync
{
    [global::Microsoft.CodeAnalysis.EmbeddedAttribute]
    internal class PSAsyncThrowTerminatingException : global::System.Exception
    {
        public PSAsyncThrowTerminatingException(global::System.Management.Automation.ErrorRecord errorRecord)
            : base()
        {
            ErrorRecord = errorRecord;
        }

        public global::System.Management.Automation.ErrorRecord ErrorRecord { get; }
    }
}
