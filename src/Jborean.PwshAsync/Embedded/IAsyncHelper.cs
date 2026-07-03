#nullable enable

namespace NamespaceReplaceMe
{
    internal interface IAsyncHelper
    {
        global::System.Management.Automation.PSCmdlet Cmdlet { get; }
        global::System.Collections.Concurrent.BlockingCollection<global::System.Action?> Pipeline { get; }
        bool InAsyncBlock { get; set; }
    }
}