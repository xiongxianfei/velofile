using VeloFile.Core.Persistence;
using VeloFile.Core.Visibility;

namespace VeloFile.Core.Tests.Visibility;

[TestClass]
[TestCategory("Visibility")]
[TestCategory("Sidebar")]
public sealed class VisibilitySettingsServiceTests
{
    [TestMethod]
    public void Visibility_settings_round_trip_through_persistent_settings_payload()
    {
        var service = VisibilitySettingsService.FromPayload(new SettingsStatePayload(
            ShowHiddenFiles: true,
            ShowProtectedOperatingSystemFiles: false,
            ShowFileExtensions: false));

        Assert.AreEqual(new VisibilitySettings(true, false, false), service.Settings);

        service.SetShowHiddenFiles(false);
        service.SetShowFileExtensions(true);

        var payload = service.ToPayload();

        Assert.IsFalse(payload.ShowHiddenFiles);
        Assert.IsFalse(payload.ShowProtectedOperatingSystemFiles);
        Assert.IsTrue(payload.ShowFileExtensions);
    }

    [TestMethod]
    public void Protected_operating_system_files_require_first_use_confirmation()
    {
        var service = VisibilitySettingsService.FromPayload(SettingsStatePayload.Default);

        var refused = service.SetShowProtectedOperatingSystemFiles(show: true, confirmed: false);

        Assert.AreEqual(VisibilityChangeStatus.ConfirmationRequired, refused);
        Assert.IsFalse(service.Settings.ShowProtectedOperatingSystemFiles);

        var accepted = service.SetShowProtectedOperatingSystemFiles(show: true, confirmed: true);

        Assert.AreEqual(VisibilityChangeStatus.Applied, accepted);
        Assert.IsTrue(service.Settings.ShowProtectedOperatingSystemFiles);
    }
}
