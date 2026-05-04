using System.Runtime.InteropServices;
using System.Text;
using VeloFile.Core.Commands;

namespace VeloFile.Windows.Clipboard;

public sealed class WindowsClipboardTextWriter : IClipboardTextWriter
{
    private const uint CfUnicodeText = 13;
    private const uint GmemMoveable = 0x0002;

    public void SetText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (!OpenClipboard(IntPtr.Zero))
        {
            throw new InvalidOperationException("Clipboard is unavailable.");
        }

        try
        {
            if (!EmptyClipboard())
            {
                throw new InvalidOperationException("Clipboard could not be cleared.");
            }

            var bytes = Encoding.Unicode.GetBytes(text + '\0');
            var memory = GlobalAlloc(GmemMoveable, (nuint)bytes.Length);
            if (memory == IntPtr.Zero)
            {
                throw new OutOfMemoryException("Could not allocate clipboard memory.");
            }

            var locked = GlobalLock(memory);
            if (locked == IntPtr.Zero)
            {
                GlobalFree(memory);
                throw new InvalidOperationException("Clipboard memory could not be locked.");
            }

            try
            {
                Marshal.Copy(bytes, 0, locked, bytes.Length);
            }
            finally
            {
                GlobalUnlock(memory);
            }

            if (SetClipboardData(CfUnicodeText, memory) == IntPtr.Zero)
            {
                GlobalFree(memory);
                throw new InvalidOperationException("Clipboard text could not be set.");
            }
        }
        finally
        {
            CloseClipboard();
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool CloseClipboard();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalAlloc(uint uFlags, nuint dwBytes);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalFree(IntPtr hMem);
}
