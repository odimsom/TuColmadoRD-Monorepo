#ifndef AppVersion
  #define AppVersion GetEnv("APP_VERSION")
#endif
#if AppVersion == ""
  #define AppVersion "0.1.0-test"
#endif

#ifndef AppVersionNumeric
  #define AppVersionNumeric GetEnv("APP_VERSION_NUMERIC")
#endif
#if AppVersionNumeric == ""
  #define AppVersionNumeric "0.1.0.0"
#endif

#define AssetsDir "assets"
#define WizardImagePath AddBackslash(SourcePath) + AssetsDir + "\\wizard.bmp"
#define WizardSmallImagePath AddBackslash(SourcePath) + AssetsDir + "\\wizard-small.bmp"
#define SetupIconPath AddBackslash(SourcePath) + AssetsDir + "\\app.ico"

[Setup]
AppId={{E8A6E2E4-3C2A-49D2-BB48-7A02489A4472}
AppName=TuColmadoRD
AppVerName=TuColmadoRD version {#AppVersion}
AppVersion={#AppVersion}
AppPublisher=Synset Solutions
AppPublisherURL=https://tucolmadord.com
AppSupportURL=https://wa.me/18296932458
AppUpdatesURL=https://tucolmadord.com
AppCopyright=Copyright (c) 2026 Synset Solutions
VersionInfoCompany=Synset Solutions
VersionInfoProductName=TuColmadoRD
VersionInfoProductVersion={#AppVersionNumeric}
VersionInfoTextVersion={#AppVersion}
DefaultDirName={autopf}\TuColmadoRD
DefaultGroupName=TuColmadoRD
OutputDir=../publish/installer
OutputBaseFilename=TuColmadoRD-Setup-v{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UsePreviousAppDir=yes
UsePreviousTasks=yes
WizardStyle=modern
MinVersion=10.0
LicenseFile=terms-and-conditions.txt
SetupLogging=yes
UninstallDisplayIcon={app}\TuColmadoRD.Desktop.exe

#ifexist "{#SetupIconPath}"
SetupIconFile={#SetupIconPath}
#endif

#ifexist "{#WizardImagePath}"
WizardImageFile={#WizardImagePath}
#endif

#ifexist "{#WizardSmallImagePath}"
WizardSmallImageFile={#WizardSmallImagePath}
#endif

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear icono en el escritorio"
Name: "startupicon"; Description: "Iniciar TuColmadoRD con Windows"; Flags: unchecked
Name: "launchapp"; Description: "Abrir TuColmadoRD al finalizar la instalacion"

[Files]
Source: "../publish/desktop/*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "assets/app.ico"; DestDir: "{app}"; Flags: ignoreversion
; Pre-instalar WebView2 si es necesario (el setup.exe debería estar en assets/)
; Source: "../assets/MicrosoftEdgeWebview2Setup.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{group}\TuColmadoRD"; Filename: "{app}\TuColmadoRD.Desktop.exe"; WorkingDir: "{app}"; IconFilename: "{app}\app.ico"; Comment: "Sistema de gestion TuColmadoRD"
Name: "{commondesktop}\TuColmadoRD"; Filename: "{app}\TuColmadoRD.Desktop.exe"; WorkingDir: "{app}"; IconFilename: "{app}\app.ico"; Tasks: desktopicon
Name: "{userstartup}\TuColmadoRD"; Filename: "{app}\TuColmadoRD.Desktop.exe"; WorkingDir: "{app}"; IconFilename: "{app}\app.ico"; Tasks: startupicon

[Run]
Filename: "{app}\TuColmadoRD.Desktop.exe"; Description: "Abrir TuColmadoRD"; Flags: nowait postinstall skipifsilent; Tasks: launchapp
; Descargar e instalar WebView2 en modo silencioso si aplica
; Filename: "{tmp}\MicrosoftEdgeWebview2Setup.exe"; Parameters: "/silent /install"; StatusMsg: "Instalando componentes necesarios (WebView2)..."; Check: WebView2NotInstalled

[Code]
function WebView2NotInstalled: Boolean;
var version: String;
begin
  Result := not RegQueryStringValue(HKLM, 'SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate\Clients\{F3017226-FE2A-4295-8BDF-00C3A9A7E4C5}', 'pv', version);
end;
