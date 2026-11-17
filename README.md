# IslandDeck

Vlastná aplikácia pre Windows 10/11 inšpirovaná videom o NotchPixel. Obsahuje:

- **Ostrov** (pilulka hore uprostred obrazovky) – po nabehnutí myšou sa rozbalí: skladba z Spotify/prehliadača
  s tlačidlami prev / play / next, hodiny a batéria.
- **Sklenený panel úloh** – priehľadný / acrylic panel úloh (efekt sa obnovuje časovačom).
- **Živá tapeta** – ľubovoľné video (mp4, wmv…) beží za ikonami plochy.
- **Widget** – veľké denné hodiny s dátumom, dá sa presúvať myšou.
- **Updater** – aplikácia si sama stiahne novú verziu z GitHub Releases.
- Ikona v oznamovacej oblasti (tray) s ovládaním všetkého; nastavenia sa ukladajú do `%AppData%\IslandDeck\settings.json`.

## Inštalátor a aktualizácie cez GitHub (jednorazové nastavenie)

Inštalátor `IslandDeck-Setup.exe` zostavuje GitHub automaticky. Na tvojom počítači nemusíš mať nainštalované nič ďalšie.

1. Vytvor si bezplatný účet na github.com a **verejný** repozitár (napr. `IslandDeck`).
2. Nahraj doň **obsah** priečinka `IslandDeck` vrátane skrytého priečinka `.github`
   (Add file → Upload files, potom označ všetko a pretiahni). Súbory musia byť priamo v koreni repozitára.
3. Otvor kartu **Actions**. Zostavenie trvá niekoľko minút. Po skončení sa vpravo na hlavnej stránke objaví
   **Releases** s verziou `1.0.<číslo behu>`.
4. Stiahni z Releases **IslandDeck-Setup.exe** a spusti ho. Inštaluje sa len pre tvoj účet, bez správcu.
   Inštalátor ponúkne spúšťanie so systémom Windows a odkaz na ploche.
5. V aplikácii: ikona pri hodinách → **Repozitár aktualizácií…** → napíš `tvoje-meno/IslandDeck`.
6. Ikona → **Skontrolovať aktualizácie**. Aplikácia sa po potvrdení sama aktualizuje a reštartuje.
   Ak je zapnuté „Kontrolovať pri spustení“, kontroluje aj pri každom štarte.

Každá ďalšia zmena nahraná do repozitára (vetva `main`) vytvorí novú verziu a updater ju ponúkne.

## Lokálne zostavenie (voliteľné, pre vývojárov)

Potrebuješ [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0):

```
dotnet publish -c Release -r win-x64 --self-contained true -o publish
```

Updater funguje pri nainštalovanej aplikácii, nie cez `dotnet run`.

## Poznámky

- Panel úloh sa pri ukončení aplikácie vráti do normálu; ak by nie, reštartuj Prieskumníka.
- Živá tapeta používa MediaElement (WPF). Pre najlepší výkon použi H.264 mp4.
- Prehrávanie médií číta systémové Windows media sessions, takže funguje s väčšinou prehrávačov.
- Windows SmartScreen môže pri prvom spustení inštalátora upozorniť, že aplikácia nie je podpísaná.
  Vyber „Ďalšie informácie“ a „Aj tak spustiť“.
- Ide o pôvodný kód; názov, vzhľad ani ikony nekopírujú NotchPixel.
