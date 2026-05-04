namespace VeloFile.Core.Commands;

public enum VeloFileCommandId
{
    Open,
    OpenWith,
    Cut,
    Copy,
    Paste,
    Rename,
    Delete,
    PermanentDelete,
    Properties,
    CopyPath,
    CopyName,
    OpenTerminalHere,
    Refresh,
    ParentFolder
}

public enum CommandProviderKind
{
    BuiltIn
}
