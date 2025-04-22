#include "CodeDependencies.iss"

[Setup]
AppName=infrabot
AppVersion=3.0.0.0
WizardStyle=modern
DefaultDirName={autopf}\infrabot\3_0_0_0\
DefaultGroupName=InfraBot
UninstallDisplayIcon={app}\PluginEditor\infrabot.PluginEditor.exe
Compression=lzma2
SolidCompression=yes
OutputDir=".\"
OutputBaseFilename="infrabot_3_0_0_0_full_install"
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin

[Files]
Source: ".\files\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\InfraBot Plugin Editor"; Filename: "{app}\PluginEditor\infrabot.PluginEditor.exe"
Name: "{userdesktop}\InfraBot Plugin Editor"; Filename: "{app}\PluginEditor\infrabot.PluginEditor.exe"; IconFilename: "{app}\PluginEditor\infrabot.PluginEditor.exe"

[Run]
Filename: {sys}\sc.exe; Parameters: "delete InfrabotWebUI" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "create InfrabotWebUI start= auto binPath= ""{app}\WebUI\Infrabot.WebUI.exe""" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "description InfrabotWebUI ""WebUI Service for infrabot""" ; Flags: runhidden

Filename: {sys}\sc.exe; Parameters: "delete InfrabotTelegramService" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "create InfrabotTelegramService start= auto binPath= ""{app}\TelegramService\Infrabot.TelegramService.exe""" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "description InfrabotTelegramService ""Telegram Service Service for infrabot""" ; Flags: runhidden

Filename: {sys}\sc.exe; Parameters: "delete InfrabotWorkerService" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "create InfrabotWorkerService start= auto binPath= ""{app}\WorkerService\Infrabot.WorkerService.exe""" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "description InfrabotWorkerService ""Telegram Service Service for infrabot""" ; Flags: runhidden


[UninstallRun]
Filename: {sys}\sc.exe; Parameters: "stop InfrabotWebUI" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "delete InfrabotWebUI" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "stop InfrabotTelegramService" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "delete InfrabotTelegramService" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "stop InfrabotWorkerService" ; Flags: runhidden
Filename: {sys}\sc.exe; Parameters: "delete InfrabotWorkerService" ; Flags: runhidden

[Code]

function InitializeSetup(): Boolean;
begin
    Dependency_AddDotNet80;
    Result := True;
end;
