using VeloFile.Core.Commands;
using VeloFile.Windows.Clipboard;

#pragma warning disable MSTEST0037

namespace VeloFile.Windows.Tests.Clipboard;

[TestClass]
[TestCategory("Commands")]
public sealed class WindowsClipboardTextWriterTests
{
    [TestMethod]
    public void Windows_clipboard_writer_implements_core_clipboard_boundary()
    {
        IClipboardTextWriter writer = new WindowsClipboardTextWriter();

        Assert.IsNotNull(writer);
    }
}
