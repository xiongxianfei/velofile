using VeloFile.Core.Listing;
using VeloFile.Core.Visibility;

#pragma warning disable MSTEST0037

namespace VeloFile.Core.Tests.Visibility;

[TestClass]
[TestCategory("Visibility")]
public sealed class VisibilityTests
{
    [TestMethod]
    public void Visibility_projection_uses_safe_v1_defaults()
    {
        var entries = new[]
        {
            Entry("normal.txt", FileAttributes.Archive),
            Entry("hidden.txt", FileAttributes.Hidden),
            Entry("system.ini", FileAttributes.Hidden | FileAttributes.System)
        };

        var projected = entries
            .Select(entry => FileVisibilityProjector.Project(entry, VisibilitySettings.Default))
            .Where(item => item is not null)
            .ToArray();

        Assert.AreEqual(1, projected.Length);
        Assert.AreEqual("normal.txt", projected[0]!.DisplayName);
        Assert.IsFalse(projected[0]!.IsVisuallyDimmed);
    }

    [TestMethod]
    public void Visibility_projection_keeps_protected_files_hidden_until_explicitly_enabled()
    {
        var hidden = Entry("hidden.txt", FileAttributes.Hidden);
        var protectedSystem = Entry("system.ini", FileAttributes.Hidden | FileAttributes.System);
        var settings = VisibilitySettings.Default with { ShowHiddenFiles = true };

        var hiddenProjected = FileVisibilityProjector.Project(hidden, settings);
        var protectedProjected = FileVisibilityProjector.Project(protectedSystem, settings);

        Assert.IsNotNull(hiddenProjected);
        Assert.IsTrue(hiddenProjected!.IsHidden);
        Assert.IsTrue(hiddenProjected.IsVisuallyDimmed);
        Assert.IsNull(protectedProjected);
    }

    [TestMethod]
    public void Visibility_projection_can_hide_known_file_extensions_without_changing_unknown_extensions()
    {
        var known = Entry("report.txt", FileAttributes.Archive);
        var unknown = Entry("private-report.foo", FileAttributes.Archive);
        var directory = Entry("src.code", FileAttributes.Directory, FileSystemEntryKind.Directory);
        var settings = VisibilitySettings.Default with { ShowFileExtensions = false };

        var knownProjected = FileVisibilityProjector.Project(known, settings);
        var unknownProjected = FileVisibilityProjector.Project(unknown, settings);
        var directoryProjected = FileVisibilityProjector.Project(directory, settings);

        Assert.AreEqual("report", knownProjected!.DisplayName);
        Assert.AreEqual("private-report.foo", unknownProjected!.DisplayName);
        Assert.AreEqual("src.code", directoryProjected!.DisplayName);
    }

    [TestMethod]
    public void Protected_files_are_dimmed_when_explicitly_visible()
    {
        var protectedSystem = Entry("system.ini", FileAttributes.Hidden | FileAttributes.System);
        var settings = VisibilitySettings.Default with
        {
            ShowHiddenFiles = true,
            ShowProtectedOperatingSystemFiles = true
        };

        var projected = FileVisibilityProjector.Project(protectedSystem, settings);

        Assert.IsNotNull(projected);
        Assert.IsTrue(projected!.IsProtectedOperatingSystemFile);
        Assert.IsTrue(projected.IsVisuallyDimmed);
    }

    private static FileSystemEntrySnapshot Entry(
        string name,
        FileAttributes attributes,
        FileSystemEntryKind kind = FileSystemEntryKind.File)
    {
        return new FileSystemEntrySnapshot(
            FullPath: Path.Combine(@"D:\folder", name),
            Name: name,
            Kind: kind,
            Length: kind is FileSystemEntryKind.File ? 100 : null,
            LastWriteTimeUtc: DateTimeOffset.Parse("2026-05-04T00:00:00Z"),
            Attributes: attributes);
    }
}
