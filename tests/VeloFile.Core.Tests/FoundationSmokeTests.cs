using VeloFile.Core.Foundation;

namespace VeloFile.Core.Tests;

[TestClass]
public sealed class FoundationSmokeTests
{
    [TestMethod]
    public void Product_identity_matches_v1_contract()
    {
        Assert.AreEqual("VeloFile", ProductIdentity.Name);
        Assert.AreEqual("fast, lightweight open-source file explorer", ProductIdentity.Tagline);
        CollectionAssert.AreEqual(new[] { "Windows 10", "Windows 11" }, ProductIdentity.SupportedWindowsVersions.ToArray());
    }

    [TestMethod]
    public void App_bootstrapper_exposes_minimal_launch_state()
    {
        var bootstrapper = new AppBootstrapper();

        var state = bootstrapper.CreateInitialState();

        Assert.AreEqual(ProductIdentity.Name, state.WindowTitle);
        Assert.IsTrue(state.AcceptsInput);
        Assert.IsFalse(state.RestoresExplorerReplacementMode);
    }
}
