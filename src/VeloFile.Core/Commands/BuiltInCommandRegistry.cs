namespace VeloFile.Core.Commands;

public sealed record BuiltInCommandDefinition(
    VeloFileCommandId CommandId,
    string Text,
    CommandProviderKind Provider,
    bool ShowInContextMenu,
    Func<CommandContext, bool> Availability)
{
    public bool IsAvailable(CommandContext context)
    {
        return Availability(context);
    }
}

public sealed record BuiltInContextMenuItem(
    VeloFileCommandId CommandId,
    string Text,
    CommandProviderKind Provider);

public sealed class BuiltInCommandRegistry
{
    private readonly IReadOnlyDictionary<VeloFileCommandId, BuiltInCommandDefinition> _commands;

    private BuiltInCommandRegistry(IEnumerable<BuiltInCommandDefinition> commands)
    {
        _commands = commands.ToDictionary(command => command.CommandId);
    }

    public bool EnumeratesShellExtensions => false;

    public static BuiltInCommandRegistry CreateDefault()
    {
        return new BuiltInCommandRegistry([
            SelectionCommand(VeloFileCommandId.Open, "Open"),
            SelectionCommand(VeloFileCommandId.OpenWith, "Open with"),
            SelectionCommand(VeloFileCommandId.Cut, "Cut"),
            SelectionCommand(VeloFileCommandId.Copy, "Copy"),
            new BuiltInCommandDefinition(
                VeloFileCommandId.Paste,
                "Paste",
                CommandProviderKind.BuiltIn,
                ShowInContextMenu: true,
                Availability: context => context.CanPaste),
            SelectionCommand(VeloFileCommandId.Rename, "Rename"),
            SelectionCommand(VeloFileCommandId.Delete, "Delete"),
            SelectionCommand(VeloFileCommandId.Properties, "Properties"),
            SelectionCommand(VeloFileCommandId.CopyPath, "Copy path"),
            SelectionCommand(VeloFileCommandId.CopyName, "Copy name"),
            new BuiltInCommandDefinition(
                VeloFileCommandId.OpenTerminalHere,
                "Open terminal here",
                CommandProviderKind.BuiltIn,
                ShowInContextMenu: true,
                Availability: context => !string.IsNullOrWhiteSpace(context.CurrentFolderPath)),
            new BuiltInCommandDefinition(
                VeloFileCommandId.PermanentDelete,
                "Permanently delete",
                CommandProviderKind.BuiltIn,
                ShowInContextMenu: false,
                Availability: context => context.SelectionCount > 0),
            new BuiltInCommandDefinition(
                VeloFileCommandId.Refresh,
                "Refresh",
                CommandProviderKind.BuiltIn,
                ShowInContextMenu: false,
                Availability: _ => true),
            new BuiltInCommandDefinition(
                VeloFileCommandId.ParentFolder,
                "Parent folder",
                CommandProviderKind.BuiltIn,
                ShowInContextMenu: false,
                Availability: _ => true)
        ]);
    }

    public BuiltInCommandDefinition GetCommand(VeloFileCommandId commandId)
    {
        return _commands[commandId];
    }

    public IReadOnlyList<BuiltInContextMenuItem> BuildContextMenu(CommandContext context)
    {
        return _commands.Values
            .Where(command => command.ShowInContextMenu && command.IsAvailable(context))
            .Select(command => new BuiltInContextMenuItem(command.CommandId, command.Text, command.Provider))
            .ToArray();
    }

    private static BuiltInCommandDefinition SelectionCommand(VeloFileCommandId commandId, string text)
    {
        return new BuiltInCommandDefinition(
            commandId,
            text,
            CommandProviderKind.BuiltIn,
            ShowInContextMenu: true,
            Availability: context => context.SelectionCount > 0);
    }
}
