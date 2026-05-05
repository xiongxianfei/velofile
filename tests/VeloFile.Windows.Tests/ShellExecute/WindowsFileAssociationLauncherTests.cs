using VeloFile.Core.FileAssociations;
using VeloFile.Windows.Processes;
using VeloFile.Windows.ShellExecute;

#pragma warning disable MSTEST0037

namespace VeloFile.Windows.Tests.FileAssociations;

[TestClass]
[TestCategory("FileAssociations")]
public sealed class WindowsFileAssociationsLauncherTests
{
    [TestMethod]
    public async Task Open_uses_shell_execute_default_association_without_mutating_associations()
    {
        var starter = new RecordingWindowsProcessStarter();
        var launcher = new WindowsFileAssociationLauncher(starter);
        var path = @"D:\scratch\report.txt";

        var result = await launcher.LaunchAsync(new FileAssociationLaunchRequest(
            FileAssociationLaunchKind.Open,
            path,
            ModifySystemAssociations: false));

        Assert.AreEqual(FileAssociationLaunchStatus.Succeeded, result.Status);
        var request = starter.Requests.Single();
        Assert.AreEqual(path, request.FileName);
        Assert.IsTrue(request.UseShellExecute);
        Assert.IsTrue(string.IsNullOrWhiteSpace(request.Verb));
        Assert.IsFalse(request.ModifySystemAssociations);
    }

    [TestMethod]
    public async Task Open_with_uses_openas_shell_verb_without_mutating_associations()
    {
        var starter = new RecordingWindowsProcessStarter();
        var launcher = new WindowsFileAssociationLauncher(starter);
        var path = @"D:\scratch\report.txt";

        var result = await launcher.LaunchAsync(new FileAssociationLaunchRequest(
            FileAssociationLaunchKind.OpenWith,
            path,
            ModifySystemAssociations: false));

        Assert.AreEqual(FileAssociationLaunchStatus.Succeeded, result.Status);
        var request = starter.Requests.Single();
        Assert.AreEqual(path, request.FileName);
        Assert.IsTrue(request.UseShellExecute);
        Assert.AreEqual("openas", request.Verb);
        Assert.IsFalse(request.ModifySystemAssociations);
    }

    [TestMethod]
    public async Task Shell_execute_failure_returns_recoverable_association_failure()
    {
        var starter = new RecordingWindowsProcessStarter { Exception = new InvalidOperationException("boom") };
        var launcher = new WindowsFileAssociationLauncher(starter);

        var result = await launcher.LaunchAsync(new FileAssociationLaunchRequest(
            FileAssociationLaunchKind.Open,
            @"D:\scratch\broken.xyz",
            ModifySystemAssociations: false));

        Assert.AreEqual(FileAssociationLaunchStatus.Failed, result.Status);
        Assert.AreEqual("association-launch-failed", result.ReasonCode);
    }

    private sealed class RecordingWindowsProcessStarter : IWindowsProcessStarter
    {
        public List<WindowsProcessStartRequest> Requests { get; } = [];

        public Exception? Exception { get; set; }

        public void Start(WindowsProcessStartRequest request)
        {
            if (Exception is not null)
            {
                throw Exception;
            }

            Requests.Add(request);
        }
    }
}
