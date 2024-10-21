using System.Runtime.InteropServices;

namespace OpenClicker
{
    public class Win32
    {
        public const uint MOUSEEVENTF_LEFTDOWN = 0x02;
        public const uint MOUSEEVENTF_LEFTUP = 0x04;

        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        [DllImport("User32.Dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("User32.DLL", SetLastError = true)]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int X, int Y)
            {
                this.X = X;
                this.Y = Y;
            }
        }
    }
}
