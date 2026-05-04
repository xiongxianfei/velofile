using VeloFile.Core.Filtering;
using VeloFile.Core.Listing;

namespace VeloFile.Core.Tests.Filtering;

[TestClass]
[TestCategory("Filtering")]
public sealed class CurrentFolderFilterTests
{
    [TestMethod]
    public void Filter_matches_visible_items_by_case_insensitive_name_substring()
    {
        var items = new[]
        {
            Item(@"D:\project\README.md", "README.md"),
            Item(@"D:\project\report.pdf", "report.pdf"),
            Item(@"D:\project\src", "src", FileSystemEntryKind.Directory),
            Item(@"D:\project\Readme.Backup", "Readme.Backup")
        };
        var service = new CurrentFolderFilterService();

        var filtered = service.Apply(items, "read");

        CollectionAssert.AreEqual(
            new[] { "README.md", "Readme.Backup" },
            filtered.Select(item => item.Name).ToArray());
    }

    [TestMethod]
    public void Clearing_filter_restores_the_unfiltered_visible_order()
    {
        var items = new[]
        {
            Item(@"D:\project\c.txt", "c.txt"),
            Item(@"D:\project\a.txt", "a.txt"),
            Item(@"D:\project\b.txt", "b.txt")
        };
        var service = new CurrentFolderFilterService();

        var filtered = service.Apply(items, "a");
        var cleared = service.Apply(items, "");

        CollectionAssert.AreEqual(new[] { "a.txt" }, filtered.Select(item => item.Name).ToArray());
        CollectionAssert.AreEqual(new[] { "c.txt", "a.txt", "b.txt" }, cleared.Select(item => item.Name).ToArray());
    }

    [TestMethod]
    public void Filter_treats_search_or_shell_metacharacters_as_literal_text()
    {
        var items = new[]
        {
            Item(@"D:\project\literal-[abc].txt", "literal-[abc].txt"),
            Item(@"D:\project\literal-a.txt", "literal-a.txt"),
            Item(@"D:\project\invoice-2026.pdf", "invoice-2026.pdf"),
            Item(@"D:\project\star-*.txt", "star-*.txt")
        };
        var service = new CurrentFolderFilterService();

        var bracket = service.Apply(items, "[abc]");
        var star = service.Apply(items, "*.txt");

        CollectionAssert.AreEqual(new[] { "literal-[abc].txt" }, bracket.Select(item => item.Name).ToArray());
        CollectionAssert.AreEqual(new[] { "star-*.txt" }, star.Select(item => item.Name).ToArray());
    }

    private static ListedFileItem Item(
        string fullPath,
        string name,
        FileSystemEntryKind kind = FileSystemEntryKind.File)
    {
        return new ListedFileItem(
            FullPath: fullPath,
            Name: name,
            DisplayName: name,
            Kind: kind,
            Length: kind is FileSystemEntryKind.File ? 10 : null,
            LastWriteTimeUtc: DateTimeOffset.Parse("2026-05-05T00:00:00Z"),
            Attributes: kind is FileSystemEntryKind.Directory ? FileAttributes.Directory : FileAttributes.Archive,
            IsHidden: false,
            IsProtectedOperatingSystemFile: false,
            IsVisuallyDimmed: false);
    }
}
