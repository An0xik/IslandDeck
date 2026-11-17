using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace IslandDeck
{
    /// <summary>
    /// Aktualizácie z GitHub Releases: nájde najnovšie vydanie, stiahne ZIP, po ukončení aplikácie
    /// prepíše súbory a aplikáciu znova spustí.
    /// </summary>
    public static class Updater
    {
        private static readonly HttpClient Http = CreateClient();

        private static HttpClient CreateClient()
        {
            var c = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            c.DefaultRequestHeaders.UserAgent.ParseAdd("IslandDeck-Updater");
            return c;
        }

        public static Version Current
        {
            get
            {
                var v = typeof(Updater).Assembly.GetName().Version ?? new Version(1, 0, 0);
                return new Version(v.Major, v.Minor, Math.Max(v.Build, 0));
            }
        }

        /// <summary>Vráti true, ak sa spustila aktualizácia a aplikácia sa má ukončiť.</summary>
        public static async Task<bool> CheckAsync(string repo, bool silent)
        {
            if (string.IsNullOrWhiteSpace(repo) || !repo.Contains('/'))
            {
                if (!silent)
                    MessageBox.Show("Najprv nastav GitHub repozitár (ikona pri hodinách, „Repozitár aktualizácií…“).",
                        "IslandDeck", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            try
            {
                string json = await Http.GetStringAsync($"https://api.github.com/repos/{repo.Trim()}/releases/latest");
                using var doc = JsonDocument.Parse(json);

                string tag = doc.RootElement.GetProperty("tag_name").GetString() ?? "";
                if (!Version.TryParse(tag.TrimStart('v', 'V'), out var latest))
                    throw new Exception($"Neplatné číslo verzie: {tag}");
                latest = new Version(latest.Major, latest.Minor, Math.Max(latest.Build, 0));

                if (latest <= Current)
                {
                    if (!silent)
                        MessageBox.Show($"Máš najnovšiu verziu ({Current}).", "IslandDeck",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                string? url = null;
                foreach (var asset in doc.RootElement.GetProperty("assets").EnumerateArray())
                {
                    string name = asset.GetProperty("name").GetString() ?? "";
                    if (name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        url = asset.GetProperty("browser_download_url").GetString();
                        break;
                    }
                }
                if (url == null)
                    throw new Exception("Vydanie neobsahuje súbor ZIP.");

                var answer = MessageBox.Show(
                    $"Je dostupná verzia {latest} (máš {Current}).\nAktualizovať teraz? Aplikácia sa na chvíľu reštartuje.",
                    "IslandDeck", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (answer != MessageBoxResult.Yes)
                    return false;

                await InstallAsync(url);
                return true;
            }
            catch (Exception ex)
            {
                if (!silent)
                    MessageBox.Show("Kontrola aktualizácií zlyhala:\n" + ex.Message, "IslandDeck",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        private static async Task InstallAsync(string url)
        {
            string work = Path.Combine(Path.GetTempPath(), "IslandDeckUpdate_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(work);

            string zip = Path.Combine(work, "update.zip");
            using (var stream = await Http.GetStreamAsync(url))
            using (var file = File.Create(zip))
                await stream.CopyToAsync(file);

            string extract = Path.Combine(work, "new");
            ZipFile.ExtractToDirectory(zip, extract);

            string? exe = Directory.GetFiles(extract, "IslandDeck.exe", SearchOption.AllDirectories).FirstOrDefault();
            if (exe == null)
                throw new Exception("V archíve chýba IslandDeck.exe.");

            string srcDir = Path.GetDirectoryName(exe)!;
            string target = AppContext.BaseDirectory.TrimEnd('\\');
            int pid = Environment.ProcessId;

            // Skript počká na ukončenie aplikácie, prepíše súbory a spustí novú verziu.
            string bat = Path.Combine(work, "update.bat");
            File.WriteAllText(bat,
                "@echo off\r\n" +
                ":wait\r\n" +
                $"tasklist /FI \"PID eq {pid}\" | find \"{pid}\" >nul\r\n" +
                "if not errorlevel 1 (\r\n  timeout /t 1 /nobreak >nul\r\n  goto wait\r\n)\r\n" +
                $"xcopy \"{srcDir}\\*\" \"{target}\\\" /E /Y /I /Q >nul\r\n" +
                $"start \"\" \"{target}\\IslandDeck.exe\"\r\n");

            Process.Start(new ProcessStartInfo("cmd.exe", $"/c \"{bat}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }
    }
}
