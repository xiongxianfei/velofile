using VeloFile.Core.Persistence;

namespace VeloFile.Core.Visibility;

public enum VisibilityChangeStatus
{
    Applied,
    ConfirmationRequired
}

public sealed class VisibilitySettingsService
{
    private VisibilitySettingsService(VisibilitySettings settings)
    {
        Settings = settings;
    }

    public VisibilitySettings Settings { get; private set; }

    public string? PreferredTerminalTargetId { get; private set; }

    public static VisibilitySettingsService FromPayload(SettingsStatePayload payload)
    {
        return new VisibilitySettingsService(new VisibilitySettings(
            payload.ShowHiddenFiles,
            payload.ShowProtectedOperatingSystemFiles,
            payload.ShowFileExtensions))
        {
            PreferredTerminalTargetId = payload.PreferredTerminalTargetId
        };
    }

    public void SetShowHiddenFiles(bool show)
    {
        Settings = Settings with { ShowHiddenFiles = show };
    }

    public void SetShowFileExtensions(bool show)
    {
        Settings = Settings with { ShowFileExtensions = show };
    }

    public VisibilityChangeStatus SetShowProtectedOperatingSystemFiles(bool show, bool confirmed)
    {
        if (show && !Settings.ShowProtectedOperatingSystemFiles && !confirmed)
        {
            return VisibilityChangeStatus.ConfirmationRequired;
        }

        Settings = Settings with { ShowProtectedOperatingSystemFiles = show };
        return VisibilityChangeStatus.Applied;
    }

    public void SetPreferredTerminalTargetId(string? targetId)
    {
        PreferredTerminalTargetId = string.IsNullOrWhiteSpace(targetId) ? null : targetId;
    }

    public SettingsStatePayload ToPayload()
    {
        return new SettingsStatePayload(
            Settings.ShowHiddenFiles,
            Settings.ShowProtectedOperatingSystemFiles,
            Settings.ShowFileExtensions,
            PreferredTerminalTargetId);
    }
}
