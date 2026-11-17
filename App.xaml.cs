using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Forms = System.Windows.Forms;

namespace IslandDeck
{
    public partial class App : Application
    {
        private Settings _settings = new();
        private IslandWindow? _island;
        private WallpaperWindow? _wallpaper;
        private WidgetWindow? _widget;
        private readonly TaskbarGlass _glass = new();
        private Forms.NotifyIcon? _tray;
        private Forms.ToolStripMenuItem? _islandItem, _glassItem, _widgetItem, _autoUpdateItem;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _settings = Settings.Load();
            BuildTray();

            if (_settings.IslandEnabled) ShowIsland();
            if (_settings.GlassTaskbar) _glass.Start();
            if (_settings.ShowWidgets) ShowWidget();
            if (!string.IsNullOrEmpty(_settings.WallpaperVideo) && System.IO.File.Exists(_settings.WallpaperVideo))
                StartWallpaper(_settings.WallpaperVideo);

            if (_settings.AutoCheckUpdates && !string.IsNullOrWhiteSpace(_settings.UpdateRepo))
                _ = CheckUpdatesAsync(silent: true);
        }

        // ---------- Tray menu ----------
        private void BuildTray()
        {
            var menu = new Forms.ContextMenuStrip();

            _islandItem = new Forms.ToolStripMenuItem("Ostrov") { Checked = _settings.IslandEnabled, CheckOnClick = true };
            _islandItem.CheckedChanged += (_, _) =>
            {
                _settings.IslandEnabled = _islandItem.Checked;
                if (_islandItem.Checked) ShowIsland(); else { _island?.Close(); _island = null; }
                _settings.Save();
            };

            _glassItem = new Forms.ToolStripMenuItem("Sklenený panel úloh") { Checked = _settings.GlassTaskbar, CheckOnClick = true };
            _glassItem.CheckedChanged += (_, _) =>
            {
                _settings.GlassTaskbar = _glassItem.Checked;
                if (_glassItem.Checked) _glass.Start(); else _glass.Stop();
                _settings.Save();
            };

            _widgetItem = new Forms.ToolStripMenuItem("Widget s hodinami") { Checked = _settings.ShowWidgets, CheckOnClick = true };
            _widgetItem.CheckedChanged += (_, _) =>
            {
                _settings.ShowWidgets = _widgetItem.Checked;
                if (_widgetItem.Checked) ShowWidget(); else { _widget?.Close(); _widget = null; }
                _settings.Save();
            };

            var pick = new Forms.ToolStripMenuItem("Vybrať video ako tapetu…");
            pick.Click += (_, _) => PickWallpaper();

            var stop = new Forms.ToolStripMenuItem("Zastaviť živú tapetu");
            stop.Click += (_, _) =>
            {
                _wallpaper?.Stop();
                _wallpaper = null;
                _settings.WallpaperVideo = null;
                _settings.Save();
            };

            var checkUpdates = new Forms.ToolStripMenuItem("Skontrolovať aktualizácie");
            checkUpdates.Click += async (_, _) => await CheckUpdatesAsync(silent: false);

            var repoItem = new Forms.ToolStripMenuItem("Repozitár aktualizácií…");
            repoItem.Click += (_, _) => ChooseRepo();

            _autoUpdateItem = new Forms.ToolStripMenuItem("Kontrolovať pri spustení")
            {
                Checked = _settings.AutoCheckUpdates,
                CheckOnClick = true
            };
            _autoUpdateItem.CheckedChanged += (_, _) =>
            {
                _settings.AutoCheckUpdates = _autoUpdateItem.Checked;
                _settings.Save();
            };

            var version = new Forms.ToolStripMenuItem($"Verzia {Updater.Current}") { Enabled = false };

            var exit = new Forms.ToolStripMenuItem("Ukončiť");
            exit.Click += (_, _) => Quit();

            menu.Items.AddRange(new Forms.ToolStripItem[]
            {
                _islandItem, _glassItem, _widgetItem, new Forms.ToolStripSeparator(), pick, stop,
                new Forms.ToolStripSeparator(), checkUpdates, repoItem, _autoUpdateItem, version,
                new Forms.ToolStripSeparator(), exit
            });

            _tray = new Forms.NotifyIcon
            {
                Icon = SystemIcons.Application,
                Text = "IslandDeck " + Updater.Current,
                Visible = true,
                ContextMenuStrip = menu
            };
        }

        // ---------- Aktualizácie ----------
        private async Task CheckUpdatesAsync(bool silent)
        {
            if (!silent && string.IsNullOrWhiteSpace(_settings.UpdateRepo))
            {
                ChooseRepo();
                if (string.IsNullOrWhiteSpace(_settings.UpdateRepo)) return;
            }

            if (await Updater.CheckAsync(_settings.UpdateRepo, silent))
                Quit();
        }

        private void ChooseRepo()
        {
            var box = new TextBox { Text = _settings.UpdateRepo, Width = 320, Margin = new Thickness(0, 8, 0, 12) };
            var ok = new Button { Content = "Uložiť", IsDefault = true, Width = 80, HorizontalAlignment = HorizontalAlignment.Right };
            var panel = new StackPanel { Margin = new Thickness(16) };
            panel.Children.Add(new TextBlock { Text = "GitHub repozitár (napr. meno/IslandDeck):" });
            panel.Children.Add(box);
            panel.Children.Add(ok);

            var win = new Window
            {
                Title = "Repozitár aktualizácií",
                Content = panel,
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Topmost = true
            };
            ok.Click += (_, _) => win.DialogResult = true;

            if (win.ShowDialog() == true)
            {
                _settings.UpdateRepo = box.Text.Trim();
                _settings.Save();
            }
        }

        // ---------- Akcie ----------
        private void ShowIsland()
        {
            _island = new IslandWindow();
            _island.Show();
        }

        private void ShowWidget()
        {
            _widget = new WidgetWindow();
            _widget.Show();
        }

        private void PickWallpaper()
        {
            var dlg = new Forms.OpenFileDialog
            {
                Title = "Vyber video pre živú tapetu",
                Filter = "Video|*.mp4;*.wmv;*.avi;*.mov;*.mkv|Všetky súbory|*.*"
            };
            if (dlg.ShowDialog() == Forms.DialogResult.OK)
            {
                StartWallpaper(dlg.FileName);
                _settings.WallpaperVideo = dlg.FileName;
                _settings.Save();
            }
        }

        private void StartWallpaper(string path)
        {
            _wallpaper?.Stop();
            _wallpaper = new WallpaperWindow(path);
            _wallpaper.Show();
        }

        private void Quit()
        {
            _glass.Stop();
            _wallpaper?.Stop();
            if (_tray != null)
            {
                _tray.Visible = false;
                _tray.Dispose();
            }
            Shutdown();
        }
    }
}
