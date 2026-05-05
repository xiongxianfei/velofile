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
    DragDropKeyModifiers Modifiers);

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
