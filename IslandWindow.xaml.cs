using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Windows.Media.Control;

namespace IslandDeck
{
    public partial class IslandWindow : Window
    {
        private const double CompactW = 150, CompactH = 36;
        private const double ExpandedW = 380, ExpandedH = 112;

        private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
        private GlobalSystemMediaTransportControlsSessionManager? _mgr;
        private GlobalSystemMediaTransportControlsSession? _session;
        private string _track = "";
        private bool _playing;

        public IslandWindow()
        {
            InitializeComponent();
            SourceInitialized += OnSourceInitialized;
            Loaded += (_, _) =>
            {
                Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
                Top = 0;
                _timer.Tick += (_, _) => UpdateClock();
                _timer.Start();
                UpdateClock();
                _ = InitMediaAsync();
            };
        }

        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            // Nezískava fokus, nezobrazí sa v Alt+Tab.
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            int ex = Native.GetWindowLong(hwnd, Native.GWL_EXSTYLE);
            Native.SetWindowLong(hwnd, Native.GWL_EXSTYLE, ex | Native.WS_EX_TOOLWINDOW | Native.WS_EX_NOACTIVATE);
        }

        // ---------- Animácie ----------
        private void Animate(double w, double h, double contentOpacity)
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            var dur = TimeSpan.FromMilliseconds(260);
            Pill.BeginAnimation(WidthProperty, new DoubleAnimation(w, dur) { EasingFunction = ease });
            Pill.BeginAnimation(HeightProperty, new DoubleAnimation(h, dur) { EasingFunction = ease });
            Expanded.BeginAnimation(OpacityProperty, new DoubleAnimation(contentOpacity, TimeSpan.FromMilliseconds(180)));
            CompactText.BeginAnimation(OpacityProperty, new DoubleAnimation(1 - contentOpacity, TimeSpan.FromMilliseconds(120)));
        }

        private void Pill_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Expanded.IsHitTestVisible = true;
            Animate(ExpandedW, ExpandedH, 1);
        }

        private void Pill_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Expanded.IsHitTestVisible = false;
            Animate(CompactW, CompactH, 0);
        }

        // ---------- Hodiny, batéria ----------
        private void UpdateClock()
        {
            ClockText.Text = DateTime.Now.ToString("HH:mm:ss");
            var ps = System.Windows.Forms.SystemInformation.PowerStatus;
            int pct = (int)Math.Round(ps.BatteryLifePercent * 100);
            bool charging = ps.PowerLineStatus == System.Windows.Forms.PowerLineStatus.Online;
            BatteryText.Text = ps.BatteryChargeStatus.HasFlag(System.Windows.Forms.BatteryChargeStatus.NoSystemBattery)
                ? "Napájanie zo siete"
                : $"{(charging ? "⚡ " : "🔋 ")}{pct} %";
            CompactText.Text = _playing && _track.Length > 0 ? "♪ " + _track : DateTime.Now.ToString("HH:mm");
        }

        // ---------- Médiá (Spotify, prehliadač, ...) ----------
        private async Task InitMediaAsync()
        {
            try
            {
                _mgr = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                _mgr.CurrentSessionChanged += (_, _) => Dispatcher.Invoke(HookSession);
                HookSession();
            }
            catch { /* médiá nie sú dostupné */ }
        }

        private void HookSession()
        {
            if (_session != null)
            {
                _session.MediaPropertiesChanged -= OnMediaChanged;
                _session.PlaybackInfoChanged -= OnMediaChanged;
            }
            _session = _mgr?.GetCurrentSession();
            if (_session != null)
            {
                _session.MediaPropertiesChanged += OnMediaChanged;
                _session.PlaybackInfoChanged += OnMediaChanged;
            }
            _ = RefreshMediaAsync();
        }

        private void OnMediaChanged(GlobalSystemMediaTransportControlsSession s, object args)
            => _ = RefreshMediaAsync();

        private async Task RefreshMediaAsync()
        {
            string title = "Nič nehrá", artist = "";
            bool playing = false;
            try
            {
                if (_session != null)
                {
                    var props = await _session.TryGetMediaPropertiesAsync();
                    title = string.IsNullOrWhiteSpace(props.Title) ? "Neznáma skladba" : props.Title;
                    artist = props.Artist ?? "";
                    playing = _session.GetPlaybackInfo().PlaybackStatus ==
                              GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
                }
            }
            catch { }

            Dispatcher.Invoke(() =>
            {
                TitleText.Text = title;
                ArtistText.Text = artist;
                PlayBtn.Content = playing ? "⏸" : "⏵";
                _track = _session != null ? title : "";
                _playing = playing;
                UpdateClock();
            });
        }

        private async void Prev_Click(object s, RoutedEventArgs e) { if (_session != null) await _session.TrySkipPreviousAsync(); }
        private async void Play_Click(object s, RoutedEventArgs e) { if (_session != null) await _session.TryTogglePlayPauseAsync(); }
        private async void Next_Click(object s, RoutedEventArgs e) { if (_session != null) await _session.TrySkipNextAsync(); }
    }
}
