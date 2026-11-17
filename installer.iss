; Skript pre Inno Setup. Inštalátor zostavuje GitHub Actions (pozri .github/workflows/release.yml).
#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

[Setup]
AppId={{B3D6F1A2-5C1E-4B7A-9E0D-1F2A3C4D5E6F}
AppName=IslandDeck
AppVersion={#AppVersion}
AppPublisher=IslandDeck
DefaultDirName={localappdata}\Programs\IslandDeck
DefaultGroupName=IslandDeck
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=output
OutputBaseFilename=IslandDeck-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\IslandDeck.exe
CloseApplications=yes

[Tasks]
Name: "autostart"; Description: "Spúšťať IslandDeck so systémom Windows"
Name: "desktopicon"; Description: "Vytvoriť odkaz na ploche"; Flags: unchecked

[Files]
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\IslandDeck"; Filename: "{app}\IslandDeck.exe"
Name: "{autodesktop}\IslandDeck"; Filename: "{app}\IslandDeck.exe"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "IslandDeck"; ValueData: """{app}\IslandDeck.exe"""; Tasks: autostart; Flags: uninsdeletevalue

[Run]
Filename: "{app}\IslandDeck.exe"; Description: "Spustiť IslandDeck"; Flags: nowait postinstall skipifsilent
