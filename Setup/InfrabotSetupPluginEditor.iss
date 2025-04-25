#include "CodeDependencies.iss"

[Setup]
AppName=infrabot Plugin Editor
AppVersion=3.0.0.0
WizardStyle=modern
DefaultDirName={autopf}\infrabot\3_0_0_0\
DefaultGroupName=InfraBot
UninstallDisplayIcon={app}\PluginEditor\infrabot.PluginEditor.exe
Compression=lzma2
SolidCompression=yes
OutputDir=".\"
OutputBaseFilename="infrabot_3_0_0_0_PluginEditor_install"
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin

[Files]
Source: ".\files\PluginEditor\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\InfraBot Plugin Editor"; Filename: "{app}\PluginEditor\infrabot.PluginEditor.exe"
Name: "{userdesktop}\InfraBot Plugin Editor"; Filename: "{app}\PluginEditor\infrabot.PluginEditor.exe"; IconFilename: "{app}\PluginEditor\infrabot.PluginEditor.exe"

[Code]

function InitializeSetup(): Boolean;
begin
    Dependency_AddDotNet80Desktop;
    Result := True;
end;
