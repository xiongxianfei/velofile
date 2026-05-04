using VeloFile.Windows.Foundation;

namespace VeloFile.Windows.Tests;

[TestClass]
public sealed class WindowsBoundarySmokeTests
{
    [TestMethod]
    public void Windows_integration_boundary_is_explicit()
    {
        Assert.AreEqual("VeloFile.Windows", WindowsIntegrationBoundary.AssemblyName);
        Assert.IsTrue(WindowsIntegrationBoundary.IsWindowsOnly);
        CollectionAssert.Contains(WindowsIntegrationBoundary.AdapterCategories.ToArray(), "Shell/Win32/WinRT interop");
    }
}
