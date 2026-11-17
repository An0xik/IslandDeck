using System;
using System.Windows.Threading;

namespace IslandDeck
{
    /// <summary>
    /// Sklenený / priehľadný panel úloh. Windows ho občas prekreslí, preto sa efekt obnovuje časovačom.
    /// </summary>
    public class TaskbarGlass
    {
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(700) };

        // Farba vo formáte AABBGGRR – jemný tmavý nádych so sklom.
        public uint Tint { get; set; } = 0x66201A14;
        public bool Acrylic { get; set; } = true;

        public TaskbarGlass()
        {
            _timer.Tick += (_, _) => Apply();
        }

        public void Start()
        {
            Apply();
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
            ForEachTaskbar(h => Native.SetAccent(h, Native.ACCENT_DISABLED, 0));
        }

        private void Apply()
        {
            int state = Acrylic ? Native.ACCENT_ENABLE_ACRYLICBLURBEHIND : Native.ACCENT_ENABLE_BLURBEHIND;
            ForEachTaskbar(h => Native.SetAccent(h, state, Tint));
        }

        private static void ForEachTaskbar(Action<IntPtr> action)
        {
            IntPtr main = Native.FindWindow("Shell_TrayWnd", null);
            if (main != IntPtr.Zero) action(main);

            IntPtr secondary = IntPtr.Zero;
            while ((secondary = Native.FindWindowEx(IntPtr.Zero, secondary, "Shell_SecondaryTrayWnd", null)) != IntPtr.Zero)
                action(secondary);
        }
    }
}
