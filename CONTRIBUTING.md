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
	```
	
2. Set up configuration:
- Copy appsettings.Development.json to all services and update as needed.
- Ensure SQLite or your database of choice is accessible.
- Telegram bot token and other secrets should be stored in environment variables or dotnet user-secrets.

3. Run the app:

- Launch from Visual Studio or run each service via CLI:
	```bash
	dotnet run --project Infrabot.WebUI
	dotnet run --project Infrabot.TelegramService
	```

## ✏️ How to Contribute

🐛 Bug Fixes
- Open an issue describing the bug.
- Submit a pull request with a fix and a short explanation in the PR description.

🧩 Plugin Contributions
- Use the PluginEditor tool to create and test plugins.
- Make sure each plugin has a valid checksum and .plug extension.
- Attach documentation in Markdown or XML format if the plugin is complex.

🔧 Feature Suggestions
- Propose new features via issues before implementation.
- Features should:
	- Follow modular design.
	- Be clearly scoped (avoid bloat).
	- Include UI support if applicable.
	
## ✅ CCode Guidelines
- Use async/await for all I/O and DB calls.
- Stick to C# 10+ conventions (nullability, var, scoped namespaces).
- Avoid magic strings — use centralized constants in /Constants/.
- Always log critical actions via _auditLogService.AddAuditLog(...).
- Test all services and utilities using NUnit, xUnit, or MSTest.
- UI logic should fallback gracefully if data is missing or invalid.

## 📦 Submitting Pull Requests
1. Fork the repo and create a new branch:
	```bash
	git checkout -b feature/my-new-feature
	```
	
2. Follow commit message guidelines:
	```bash
	feat(plugin): added dynamic firewall plugin
	fix(webui): correct pagination on user view
	```
	
3. Ensure your code:
	- Builds successfully
	- Passes tests (if applicable)
	- Has no obvious side effects

4. Open a pull request:
	- Describe what you changed
	- Explain why it’s necessary
	- Mention how it was tested

##  🔐 Security Reporting
If you discover a vulnerability or security-related issue, please do not open a public issue. Instead, contact with me directly.

##  👀 Code of Conduct
We expect all contributors to follow respectful, inclusive, and professional behavior in line with our CODE_OF_CONDUCT.md.
Harassment, abuse, or discrimination of any kind will not be tolerated.

## 💬 Questions?
- Start a GitHub Discussion if you want to brainstorm a feature or improvement.
- Open an issue if you spot a bug or missing behavior.
