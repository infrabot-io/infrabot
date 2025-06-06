<img src="assets/header.svg"/>

<div align="center">

# <img src="assets/infrabot.png" height="48" width="48"/> **Infrabot** – Control Your Infrastructure From Telegram

[![Infrabot Badge](https://img.shields.io/badge/infrabot-orange)](https://github.com/infrabot-io/infrabot)
![License](https://img.shields.io/github/license/infrabot-io/infrabot)
![Lines of Code](https://img.shields.io/tokei/lines/github/infrabot-io/infrabot)
![Downloads](https://img.shields.io/github/downloads/infrabot-io/infrabot/total)
![Stars](https://img.shields.io/github/stars/infrabot-io/infrabot?style=social)
![Contributors](https://img.shields.io/github/contributors/infrabot-io/infrabot)

</div>

---

## 🧠 What is Infrabot?

**Infrabot** is a powerful **on-premise automation platform** designed those who want instant and secure command execution **directly from Telegram**.

Build your own modular **commandlets**, extend functionality with **plugins**, and manage your infrastructure with just a message.

---

## 👀 Live Demo

<div align="center">
  <img src="assets/demo.gif" alt="infrabot-demo" width="420px" />
</div>

<img src="assets/1.PNG" alt="infrabot-demo" />
<img src="assets/2.PNG" alt="infrabot-demo" />
<img src="assets/3.PNG" alt="infrabot-demo" />
<img src="assets/4.PNG" alt="infrabot-demo" />
<img src="assets/5.PNG" alt="infrabot-demo" />
<img src="assets/6.PNG" alt="infrabot-demo" />
<img src="assets/7.PNG" alt="infrabot-demo" />

---

## 📦 Features

- Runs as a secure Windows service
- Fully modular architecture — add/remove features with plugins
- Command & control through Telegram
- Encrypted configuration and storage
- Easy-to-create plugins using C#
- Built-in plugin manager, service control, and configuration tooling
- No third-party cloud required
- Comes with examples

---

## 🧰 Technical Requirements

Before installing or running Infrabot, ensure your environment meets the following requirements:

### Runtime & Platform
- **Operating System:** Windows Server 2012 and above or 8/10/11 or Linux (Ubuntu, RHEL, CentOS, Fedora, Alpine)
- **.NET Runtime:** [.NET 8.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (ASP.NET Core Hosting Bundle + .NET Desktop Runtime)
- **Architecture:** x64 (recommended)

### Hardware Requirements
- **CPU:** Dual-core (2 GHz or higher), 64-bit
- **RAM:** Minimum 4 GB (8 GB recommended for plugin-heavy usage)
- **Disk:** At least 400 MB of free space (more if storing large plugin resources or logs)

### Permissions
- If Windows (not\and AD joined) **Service Account Access:**
  - Must have **read/write** access to the Infrabot installation directory
  - Must have permission to execute **PowerShell, Python, Shell**
  - Can be a **non-administrative user** or an **Active Directory Managed Service Account**
- If Linux:
  - Must have **read/write** access to the Infrabot installation directory
  - Must have permission to execute **PowerShell, Python, Shell**
  - Can be a **non-administrative user**
- **For Admin-Level Commands:**  
  Scripts or executables must handle elevation internally if the bot is not running with admin rights

### Telegram Integration
- A registered **Telegram Bot Token** from [@BotFather](https://t.me/BotFather)
- Your Telegram ID must be added in the **Telegram Users** section to receive permissions

### Files & Execution
- The following paths must be correctly set (environment variables supported):
  - **PowerShell Path**
  - **Shell Path**
  - **Python Path**
- Plugins must be deployed as `.plug` files and placed into the `/plugins` directory

### Security Considerations
- **ExecutionPolicy:** Recommended to use `AllSigned` for PowerShell
- Plugin execution files are **hash-verified** to prevent tampering
- Permissions can be granted per user or via user groups

---

## 🛠️ Installation Guide

**Automatic**
1. Download the latest [release](https://github.com/infrabot-io/infrabot/releases) and follow the installation process to set up infrabot on your system
2. Open [https://localhost:8443](https://localhost:8443) and login into the system. Default credentials:
```
Login : admin
Password : password
```
3. Review Getting Started page which can be accessed via left side menu

**Manual**
1. Install the latest [ASP.NET Core Runtime 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
1. Install the latest [.NET Desktop Runtime 8](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
2. Download the latest ZIP release from the [Releases Page](https://github.com/infrabot-io/infrabot/releases)
3. Unpack files into the folder
4. Open CMD and configure services using NSSM which comes with the ZIP archive
```
cd "C:\your\folder\infrabot\nssm-2.24\win64"

nssm.exe install InfrabotWebUI "C:\your\folder\infrabot\WebUI\Infrabot.WebUI.exe"
nssm.exe set InfrabotWebUI AppDirectory "C:\your\folder\infrabot\WebUI"

nssm.exe install InfrabotTelegramService "C:\your\folder\infrabot\TelegramService\Infrabot.TelegramService.exe"
nssm.exe set InfrabotTelegramService AppDirectory "C:\your\folder\infrabot\TelegramService"

nssm.exe install InfrabotWorkerService "C:\your\folder\infrabot\WorkerService\Infrabot.WorkerService.exe"
nssm.exe set InfrabotWorkerService AppDirectory "C:\your\folder\infrabot\WorkerService"
```
5. Launch the ***InfrabotWebUI*** and ***InfrabotWorkerService*** services, and open [https://localhost:8443](https://localhost:8443) to login into the system using default credentials:
```
Login : admin
Password : password
```
6. Configure ***Telegram Bot Token*** on the Configuration page. If you do not know how to get Telegram Bot Token, review Getting Started page on the left side menu. 
7. Launch ***InfrabotTelegramService*** service

---

## 📏 Scheme

Scheme of infrabot components is specified below.

```mermaid
stateDiagram-v2
    state "Web Admin Service <br>
    - Web UI
    - User/Group Mgmt
    - Plugin Access Mgmt" as s1
    state "Telegram Service <br>
    - Telegram Client
    - Plugin Manager 
    - Command Manager" as s2
    state "Worker Service  <br>
    - Health Checker
    - Health Data Cleaner
    - Message Cleaner" as s3
    state "SQLite Database <br>
    - Users
    - Groups
    - Plugins
    - Permissions
    - Health
    - Logs
    - Audit events" as s4
    s1 --> s4
    s2 --> s4
    s3 --> s4
```

---

## 🔌 Plugin System

Infrabot is built to be extended.

🧠 Want to automate server reboots? Query databases? Deploy services? Just write a plugin.

> 📂 `.plug` files are compiled, serialized via Protocol Buffers, and live independently of the core app.
 
Each plugin defines the commandlets Infrabot can execute — and you can include multiple commands in one plugin.

📎 Check out the [Example Plugins](https://infrabot-io.github.io/documentation/examplescripts.html) to get started.

<details>
<summary><strong>📦 Plugin Basics</strong></summary>

- Format: Only `.plug` files are recognized
- Unique GUID & Plugin ID assigned at creation
- Commands with the same name across plugins are supported — just use the plugin ID to specify which one to run
- Created/modified using the **Plugin Editor**
- Contains metadata + scripts/apps needed for execution
- Each plugin can define **multiple commandlets**

</details>

<details>
<summary><strong>🚀 Installing Plugins</strong></summary>

- Copy the `.plug` file to `/plugins` in the Infrabot Telegram Service directory  
- Infrabot auto-detects and extracts contents into `/plugins/{plugin-GUID}`  
- If a newer version exists, it will **replace the old one**  
- Plugin appears automatically on the **Plugins** web page  
- Optionally use `/reloadplugins` to force immediate plugin reload  

</details>

<details>
<summary><strong>🗑️ Plugin Removal</strong></summary>

- Deleting the `.plug` file removes plugin
- Extracted plugin folder remains unless deleted manually  
- If redeployed, the folder is **replaced and re-extracted** automatically  

</details>

<details>
<summary><strong>🔐 Integrity & Execution</strong></summary>

- Executables can reside in subdirectories within the plugin folder  
- Use correct relative paths in the plugin configuration  
- File hashes are checked before each execution  
- If a mismatch is found, execution is **blocked** to prevent tampering  

</details>

<details>
<summary><strong>🔄 Command Updates & Conflicts</strong></summary>

- Telegram command menu updates within **3–5 minutes**  
- Use plugin IDs to disambiguate overlapping command names  
- Duplicate entries?  
  → Delete the `.plug` file → wait for cleanup → redeploy cleanly  

</details>

---

## 🧱 Building from Source

Want to customize or contribute?

1. Install **Visual Studio 2022**
2. Open the solution file
3. Build the project (Debug or Release mode)

That's it — you're ready to roll.

---

## 🤝 Contribute

We love community contributions! Here's how you can help:

- Report bugs and open issues
- Add new functionality
- Develop new plugins
- Improve the documentation

Every PR is appreciated. Let's build something epic together.

<div align="center">
  <img src="assets/footer_fixed.svg"/>
</div>
