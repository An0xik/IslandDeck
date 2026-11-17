using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace IslandDeck
{
    /// <summary>
    /// Živá tapeta: okno s videom vložené za ikony pracovnej plochy (trik s Progman/WorkerW).
    /// </summary>
    public class WallpaperWindow : Window
    {
        private readonly MediaElement _media;

        public WallpaperWindow(string videoPath)
        {
            WindowStyle = WindowStyle.None;
            ShowInTaskbar = false;
            ResizeMode = ResizeMode.NoResize;
            Background = Brushes.Black;

            _media = new MediaElement
            {
                LoadedBehavior = MediaState.Manual,
                UnloadedBehavior = MediaState.Manual,
                Stretch = Stretch.UniformToFill,
                Volume = 0,
                IsMuted = true,
                Source = new Uri(videoPath)
            };
            _media.MediaEnded += (_, _) =>
            {
                _media.Position = TimeSpan.Zero;
                _media.Play();
            };
            Content = _media;

            Loaded += (_, _) =>
            {
                AttachToDesktop();
                _media.Play();
            };
        }

        private void AttachToDesktop()
        {
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            IntPtr progman = Native.FindWindow("Progman", null);
            Native.SendMessageTimeout(progman, 0x052C, IntPtr.Zero, IntPtr.Zero, 0, 1000, out _);

            // Windows 10 / starší Windows 11: WorkerW za SHELLDLL_DefView
            IntPtr workerW = IntPtr.Zero;
            Native.EnumWindows((h, _) =>
            {
                IntPtr defView = Native.FindWindowEx(h, IntPtr.Zero, "SHELLDLL_DefView", null);
                if (defView != IntPtr.Zero)
                    workerW = Native.FindWindowEx(IntPtr.Zero, h, "WorkerW", null);
                return true;
            }, IntPtr.Zero);

            int style = Native.GetWindowLong(hwnd, Native.GWL_STYLE);
            style = (style & ~Native.WS_POPUP) | Native.WS_CHILD;
            Native.SetWindowLong(hwnd, Native.GWL_STYLE, style);

            // Windows 11 24H2+: WorkerW chýba, rodičom je priamo Progman
            IntPtr parent = workerW != IntPtr.Zero ? workerW : progman;
            Native.SetParent(hwnd, parent);

            int w = Native.GetSystemMetrics(0); // SM_CXSCREEN
            int h2 = Native.GetSystemMetrics(1); // SM_CYSCREEN
            Native.SetWindowPos(hwnd, Native.HWND_BOTTOM, 0, 0, w, h2,
                Native.SWP_NOACTIVATE | Native.SWP_SHOWWINDOW);
        }

        public void Stop()
        {
            _media.Stop();
            Close();
        }
    }
}
