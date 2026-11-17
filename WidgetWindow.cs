using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace IslandDeck
{
    /// <summary>
    /// Veľké denné hodiny s dátumom v rohu plochy (jednoduchý widget).
    /// </summary>
    public class WidgetWindow : Window
    {
        private readonly TextBlock _time = new()
        {
            FontSize = 72, FontWeight = FontWeights.Light, Foreground = Brushes.White
        };
        private readonly TextBlock _date = new()
        {
            FontSize = 20, Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255))
        };
        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };

        public WidgetWindow()
        {
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            ShowInTaskbar = false;
            ResizeMode = ResizeMode.NoResize;
            SizeToContent = SizeToContent.WidthAndHeight;
            Topmost = false;

            var panel = new StackPanel { Margin = new Thickness(24) };
            panel.Children.Add(_time);
            panel.Children.Add(_date);
            Content = panel;

            SourceInitialized += (_, _) =>
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                int ex = Native.GetWindowLong(hwnd, Native.GWL_EXSTYLE);
                Native.SetWindowLong(hwnd, Native.GWL_EXSTYLE, ex | Native.WS_EX_TOOLWINDOW | Native.WS_EX_NOACTIVATE);
            };
            Loaded += (_, _) =>
            {
                Left = 40;
                Top = SystemParameters.WorkArea.Height - ActualHeight - 40;
                _timer.Tick += (_, _) => Tick();
                _timer.Start();
                Tick();
            };
            MouseLeftButtonDown += (_, _) => DragMove();
        }

        private void Tick()
        {
            var now = DateTime.Now;
            _time.Text = now.ToString("HH:mm");
            _date.Text = now.ToString("dddd, d. MMMM");
        }
    }
}
