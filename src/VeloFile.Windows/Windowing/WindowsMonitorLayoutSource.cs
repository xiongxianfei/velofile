using System.Runtime.InteropServices;
using VeloFile.Core.Session;

namespace VeloFile.Windows.Windowing;

public sealed class WindowsMonitorLayoutSource : IMonitorLayoutSource
{
    private const int MonitorInfoPrimary = 1;

    public IReadOnlyList<MonitorWorkArea> GetCurrentWorkAreas()
    {
        var monitors = new List<MonitorWorkArea>();

        MonitorEnumProc callback = (
            IntPtr monitorHandle,
            IntPtr monitorDeviceContext,
            ref Rect monitorBounds,
            IntPtr data) =>
            {
                _ = monitorDeviceContext;
                _ = monitorBounds;
                _ = data;

                var monitorInfo = new MonitorInfoEx
                {
                    Size = Marshal.SizeOf<MonitorInfoEx>(),
                    DeviceName = string.Empty
                };

                if (GetMonitorInfo(monitorHandle, ref monitorInfo))
                {
                    var workArea = monitorInfo.WorkArea;
                    var width = workArea.Right - workArea.Left;
                    var height = workArea.Bottom - workArea.Top;
                    if (width > 0 && height > 0)
                    {
                        monitors.Add(new MonitorWorkArea(
                            monitorInfo.DeviceName,
                            workArea.Left,
                            workArea.Top,
                            width,
                            height,
                            (monitorInfo.Flags & MonitorInfoPrimary) == MonitorInfoPrimary,
                            GetMonitorRasterizationScale(monitorHandle)));
                    }
                }

                return true;
            };

        EnumDisplayMonitors(
            IntPtr.Zero,
            IntPtr.Zero,
            callback,
            IntPtr.Zero);

        return monitors;
    }

    private static double? GetMonitorRasterizationScale(IntPtr monitorHandle)
    {
        try
        {
            var result = GetDpiForMonitor(monitorHandle, MonitorDpiType.Effective, out var dpiX, out _);
            return result == 0 && dpiX > 0
                ? dpiX / 96.0
                : null;
        }
        catch
        {
            return null;
        }
    }

    private delegate bool MonitorEnumProc(
        IntPtr monitorHandle,
        IntPtr monitorDeviceContext,
        ref Rect monitorBounds,
        IntPtr data);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(
        IntPtr deviceContext,
        IntPtr clippingRect,
        MonitorEnumProc callback,
        IntPtr data);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetMonitorInfo(
        IntPtr monitorHandle,
        ref MonitorInfoEx monitorInfo);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(
        IntPtr monitorHandle,
        MonitorDpiType dpiType,
        out uint dpiX,
        out uint dpiY);

    private enum MonitorDpiType
    {
        Effective = 0
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct MonitorInfoEx
    {
        public int Size;
        public Rect MonitorArea;
        public Rect WorkArea;
        public int Flags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
    }
}
