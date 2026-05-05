using VeloFile.Core.Listing;

namespace VeloFile.Core.DragDrop;

[Flags]
public enum DragDropKeyModifiers
{
    None = 0,
    Control = 1,
    Shift = 2,
    Alt = 4
}

public enum DropVolumeRelationship
{
    SameVolume,
    CrossVolume,
    Unknown
}

public enum DropAction
{
    None,
    Copy,
    Move,
    Shortcut
}

public sealed record DropItem(
    string FullPath,
    string Name,
    FileSystemEntryKind Kind)
{
    public static DropItem FromListedItem(ListedFileItem item)
    {
        return new DropItem(item.FullPath, item.Name, item.Kind);
    }
}

public sealed record DragDropRequest(
    IReadOnlyList<DropItem> Items,
    string TargetDirectory,
    DropVolumeRelationship VolumeRelationship,
    DragDropKeyModifiers Modifiers,
    bool SupportsShortcut = true);

public sealed record DropActionResolution(
    DropAction Action,
    string IndicatorText,
    bool CanDrop,
    string? ReasonCode)
{
    public static DropActionResolution None(string reasonCode)
    {
        return new DropActionResolution(DropAction.None, "Drop unavailable", CanDrop: false, reasonCode);
    }
}

public sealed class DragDropActionResolver
{
    public DropActionResolution Resolve(DragDropRequest request)
    {
        if (request.Items.Count == 0)
        {
            return DropActionResolution.None("drop-no-items");
        }

        if (string.IsNullOrWhiteSpace(request.TargetDirectory))
        {
            return DropActionResolution.None("drop-no-target");
        }

        var targetDirectory = request.TargetDirectory.Trim();
        var action = ResolveAction(request);
        if (action is DropAction.Shortcut && !request.SupportsShortcut)
        {
            return DropActionResolution.None("drop-shortcut-unsupported");
        }

        var verb = action switch
        {
            DropAction.Copy => "Copy to",
            DropAction.Move => "Move to",
            DropAction.Shortcut => "Create shortcut in",
            _ => "Drop on"
        };

        return new DropActionResolution(
            action,
            $"{verb} {targetDirectory}",
            CanDrop: true,
            ReasonCode: null);
    }

    private static DropAction ResolveAction(DragDropRequest request)
    {
        var modifiers = request.Modifiers;
        if ((modifiers & (DragDropKeyModifiers.Control | DragDropKeyModifiers.Shift))
            == (DragDropKeyModifiers.Control | DragDropKeyModifiers.Shift))
        {
            return DropAction.Shortcut;
        }

        if ((modifiers & DragDropKeyModifiers.Alt) == DragDropKeyModifiers.Alt)
        {
            return DropAction.Shortcut;
        }

        if ((modifiers & DragDropKeyModifiers.Control) == DragDropKeyModifiers.Control)
        {
            return DropAction.Copy;
        }

        if ((modifiers & DragDropKeyModifiers.Shift) == DragDropKeyModifiers.Shift)
        {
            return DropAction.Move;
        }

        return request.VolumeRelationship is DropVolumeRelationship.SameVolume
            ? DropAction.Move
            : DropAction.Copy;
    }
}

public static class DropVolumeRelationshipClassifier
{
    public static DropVolumeRelationship Classify(IReadOnlyList<DropItem> items, string targetDirectory)
    {
        if (items.Count == 0 || string.IsNullOrWhiteSpace(targetDirectory))
        {
            return DropVolumeRelationship.Unknown;
        }

        var targetRoot = SafeRoot(targetDirectory);
        if (string.IsNullOrWhiteSpace(targetRoot))
        {
            return DropVolumeRelationship.Unknown;
        }

        var sawUnknown = false;
        foreach (var item in items)
        {
            var itemRoot = SafeRoot(item.FullPath);
            if (string.IsNullOrWhiteSpace(itemRoot))
            {
                sawUnknown = true;
                continue;
            }

            if (!string.Equals(itemRoot, targetRoot, StringComparison.OrdinalIgnoreCase))
            {
                return DropVolumeRelationship.CrossVolume;
            }
        }

        return sawUnknown ? DropVolumeRelationship.Unknown : DropVolumeRelationship.SameVolume;
    }

    private static string? SafeRoot(string path)
    {
        try
        {
            return Path.GetPathRoot(path);
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }
}
