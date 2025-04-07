<img src="assets/header.svg"/>

<div align="center">

# ⚙️ **Infrabot** – Automate Your Infrastructure From Telegram

[![Infrabot Badge](https://img.shields.io/badge/infrabot-orange)](https://github.com/infrabot-io/infrabot)
![License](https://img.shields.io/github/license/infrabot-io/infrabot)
![Lines of Code](https://img.shields.io/tokei/lines/github/infrabot-io/infrabot)
![Downloads](https://img.shields.io/github/downloads/infrabot-io/infrabot/total)
![Stars](https://img.shields.io/github/stars/infrabot-io/infrabot?style=social)
![Contributors](https://img.shields.io/github/contributors/infrabot-io/infrabot)

</div>

---

## 🧠 What is Infrabot?

🚀 **Infrabot** is a powerful **on-premise automation platform** designed for DevOps, SREs, sysadmins, and infrastructure engineers who want instant, secure command execution **directly from Telegram**.

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

1. Install the [.NET 6 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
2. Download the latest release from the [Releases Page](https://github.com/infrabot-io/infrabot/releases)
3. Unpack, configure, and launch the service

💡 _Need help setting it up? Scroll down to the docs section!_

---

## 📚 Documentation

Your roadmap to becoming an Infrabot pro:

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

Infrabot is built to be extended. Each plugin represents a **single commandlet**, and you can write your own in C# using the plugin template system.

🧠 Want to automate server reboots? Query databases? Deploy services? Just write a plugin.

> 📂 `.plug` files are compiled, serialized via Protocol Buffers, and live independently of the core app.

📎 See [Example Plugins](https://infrabot-io.github.io/documentation/examplescripts.html) to get started!

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

- 🐛 **Report bugs** and open issues
- ✨ **Add new functionality**
- 🔌 **Develop new plugins**
- 🧾 **Improve the documentation**

Every PR is appreciated. Let's build something epic together.

---

<div align="center">
  <img src="assets/footer_fixed.svg"/>
</div>
