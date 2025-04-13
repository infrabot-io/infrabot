# Contributing to Infrabot

Welcome, and thank you for considering contributing to Infrabot! This document outlines how to get started, coding standards, and what to keep in mind when submitting changes.

---

## 📦 Project Structure

- **Infrabot.WebUI** — ASP.NET Core MVC Web interface for managing users, plugins, and groups.
- **Infrabot.TelegramService** — Telegram bot service with plugin command execution.
- **Infrabot.PluginSystem** — Plugin definitions, execution models, and serialization (ProtoBuf).
- **Infrabot.PluginEditor** — Desktop WPF tool for managing `.plug` files.
- **Infrabot.WorkerService** — Background tasks for system health and data cleanup.
- **Infrabot.Common** — Shared models and Entity Framework context.

---

## 🚀 Getting Started

1. Clone the repo:
   ```bash
   git clone https://github.com/yourusername/infrabot.git
   cd infrabot
	````
	
2. Set up configuration:
- Copy appsettings.Development.json to all services and update as needed.
- Ensure SQLite or your database of choice is accessible.
- Telegram bot token and other secrets should be stored in environment variables or dotnet user-secrets.

3. Run the app:

- Launch from Visual Studio or run each service via CLI:
	- dotnet run --project Infrabot.WebUI
	- dotnet run --project Infrabot.TelegramService
	
