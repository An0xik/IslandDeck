# IslandDeck

IslandDeck is a small desktop customization app for Windows 10 and 11. It runs in the background
and is controlled from a tray icon.

## Features

- **Island** – a compact black pill at the top center of the screen. Hover over it to expand it:
  now-playing track (Spotify, browsers and other players) with previous / play / next buttons,
  clock and battery status.
- **Glass taskbar** – a translucent, blurred taskbar.
- **Live wallpaper** – play any video (mp4, wmv, avi, mov, mkv) behind your desktop icons.
- **Clock widget** – a large clock and date on the desktop that you can drag around.
- **Built-in updater** – checks GitHub Releases and updates the app with one click.

> The app's own menus and labels are currently in Slovak.

## Install

1. Open the **Releases** page of this repository.
2. Download **IslandDeck-Setup.exe** from the latest release and run it.
3. Choose whether to start IslandDeck with Windows and whether to create a desktop shortcut.

The installer installs for the current user only and does not need administrator rights.
Nothing else has to be installed first.

Windows SmartScreen may warn that the app is unsigned the first time you run the installer.
Click **More info**, then **Run anyway**.

## Using the app

Right-click the IslandDeck icon in the notification area (next to the clock; it may be behind the `^` arrow).

| Menu item | What it does |
| --- | --- |
| Island | Show or hide the island |
| Glass taskbar | Turn the translucent taskbar on or off |
| Clock widget | Show or hide the desktop clock |
| Choose wallpaper video… | Pick a video to use as the live wallpaper |
| Stop live wallpaper | Remove the live wallpaper |
| Check for updates | Look for a newer version and install it |
| Update repository… | Set the GitHub repository to update from, as `owner/repo` |
| Check on startup | Check for updates every time the app starts |
| Exit | Quit the app and restore the taskbar |

Settings are saved in `%AppData%\IslandDeck\settings.json`.

## Updates

The updater downloads the newest release from the GitHub repository set under **Update repository…**
and replaces the installed files. The app restarts by itself afterwards.

Every push to the `main` branch of the repository builds a new release automatically
(see `.github/workflows/release.yml`). Each release contains:

- `IslandDeck-Setup.exe` – the installer
- `IslandDeck.zip` – the files used by the in-app updater

## Uninstall

Open **Settings → Apps → Installed apps**, find **IslandDeck** and uninstall it.
Optionally delete `%AppData%\IslandDeck` to remove saved settings.

## Notes and troubleshooting

- If the taskbar stays translucent after quitting, restart Windows Explorer from Task Manager.
- For the smoothest live wallpaper, use an H.264 mp4. Very heavy 4K videos can load the system.
- The island reads the system media session, so it works with most players that report what they play.
