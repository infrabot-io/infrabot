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

**Infrabot** is a powerful **on-premise automation platform** designed for DevOps, SREs, sysadmins, and infrastructure engineers who want instant, secure command execution **directly from Telegram**.

Build your own modular **commandlets**, extend functionality with **plugins**, and manage your infrastructure with just a message. All without exposing your systems to the cloud.

> ⚡️ Your infrastructure, your rules — now in your pocket.

---

## 👀 Live Demo

<div align="center">
  <img src="assets/demo.gif" alt="infrabot-demo" width="420px" />
</div>

---

## 📦 Features

- ✅ **Runs as a secure Windows service**
- 📁 **Fully modular architecture** — add/remove features with plugins
- 📲 **Command & control through Telegram**
- 🔐 **Encrypted configuration and storage**
- 🧩 **Easy-to-create plugins using C#**
- 🕹️ **Built-in plugin manager, service control, and configuration tooling**
- 🧰 **No third-party cloud required**

---

## 🛠️ Installation Guide

1. Install the [.NET 8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
2. Download the latest release from the [Releases Page](https://github.com/infrabot-io/infrabot/releases)
3. Unpack, configure, and launch the service

💡 _Need help setting it up? Scroll down to the docs section!_

---

## 📚 Documentation

Everything you need to get up and running with Infrabot:

📘 **Essentials**
- [Getting Started](https://infrabot-io.github.io/documentation/gettingstarted.html)
- [Create a Telegram Bot](https://infrabot-io.github.io/documentation/createbot.html)
- [Configuration Overview](https://infrabot-io.github.io/documentation/configoverview.html)

🧠 **Deep Dives**
- [Plugin System](https://infrabot-io.github.io/documentation/pluginoverview.html)
- [Service Behavior](https://infrabot-io.github.io/documentation/infrabotservice.html)
- [Example Scripts](https://infrabot-io.github.io/documentation/examplescripts.html)

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

- Deleting the `.plug` file removes plugin metadata and disables it  
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

- **Report bugs** and open issues
- **Add new functionality**
- **Develop new plugins**
- **Improve the documentation**

Every PR is appreciated. Let's build something epic together.

<div align="center">
  <img src="assets/footer_fixed.svg"/>
</div>
