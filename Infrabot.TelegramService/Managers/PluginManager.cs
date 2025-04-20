using Infrabot.PluginSystem;
using Infrabot.PluginSystem.Utils;
using Infrabot.Common.Contexts;
using Infrabot.TelegramService.Core;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using Infrabot.PluginSystem.Enums;

namespace Infrabot.TelegramService.Managers
{
    public class PluginManager : BackgroundService, IPluginRegistry
    {
        private readonly ILogger<PluginManager> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private readonly List<Plugin> _plugins = new();
        private readonly object _lock = new();
        private string _pluginDirectory;
        private readonly ConcurrentDictionary<Guid, (DateTime LastModified, int Version)> _loadedPluginMeta = new();
        private readonly TimeSpan _period;

        public PluginManager(ILogger<PluginManager> logger, IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _pluginDirectory = NormalizePluginPath(configuration["Plugins:PluginsDirectory"] ?? "plugins");

            _logger.LogInformation("Init: Plugin Manager");

            int period = Convert.ToInt32(_configuration["Services:PluginManagerRefreshIntervalSeconds"]);
            _logger.LogInformation($" Plugin Manager interval is set to {period} seconds.");
            _period = TimeSpan.FromSeconds(period);
        }

        public IReadOnlyList<Plugin> Plugins
        {
            get
            {
                lock (_lock)
                {
                    return _plugins.ToList();
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting plugin manager background service...");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    RefreshPlugins();
                    await SyncPluginsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while refreshing plugins.");
                }

                await Task.Delay(_period, stoppingToken);
            }

            _logger.LogInformation("Plugin manager background service stopped.");
        }

        public async void RefreshPlugins()
        {
            if (!Directory.Exists(_pluginDirectory))
            {
                Directory.CreateDirectory(_pluginDirectory);
                return;
            }

            var discoveredPlugins = new Dictionary<Guid, (Plugin Plugin, DateTime Timestamp, int Version)>();

            foreach (var file in Directory.GetFiles(_pluginDirectory, "*.plug"))
            {
                try
                {
                    var lastModified = File.GetLastWriteTimeUtc(file);
                    var plugin = await PluginUtility.GetPlugin(file);

                    // Validate plugin
                    if (string.IsNullOrWhiteSpace(plugin.Id) || plugin.Guid == Guid.Empty || plugin.PluginExecutions.Count == 0)
                    {
                        _logger.LogWarning("Plugin '{File}' failed validation. Skipping.", Path.GetFileName(file));
                        continue;
                    }

                    // Get plugin folder path
                    var folderPath = Path.Combine(_pluginDirectory, plugin.Guid.ToString());

                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                        await PluginUtility.ExtractPluginFiles(plugin, folderPath);
                    }

                    // Track whether it's new, updated, or same
                    if (_loadedPluginMeta.TryGetValue(plugin.Guid, out var loadedMeta))
                    {
                        if (loadedMeta.Version >= plugin.Version && loadedMeta.LastModified >= lastModified)
                        {
                            // not updated, but still valid - keep it in memory
                            _logger.LogInformation("Plugin '{Name}' unchanged", plugin.Name);
                            plugin.PluginFiles?.ForEach(file => file.FileData = Array.Empty<byte>());
                            discoveredPlugins[plugin.Guid] = (plugin, loadedMeta.LastModified, loadedMeta.Version);
                            continue;
                        }

                        if (loadedMeta.Version < plugin.Version)
                        {
                            _logger.LogInformation("Plugin '{Name}' upgraded to version {Version}", plugin.Name, plugin.Version);
                        }
                        else if (loadedMeta.Version == plugin.Version && loadedMeta.LastModified < lastModified)
                        {
                            _logger.LogInformation("Plugin '{Name}' reloaded due to file update", plugin.Name);
                        }

                        if (Directory.Exists(folderPath))
                        {
                            Directory.Delete(folderPath, true);
                            WaitForDeletion(folderPath);
                        }

                        Directory.CreateDirectory(folderPath);
                        await PluginUtility.ExtractPluginFiles(plugin, folderPath);
                    }
                    else
                    {
                        _logger.LogInformation("Loaded plugin '{Name}' (ID: {Id})", plugin.Name, plugin.Id);
                    }

                    plugin.PluginFiles?.ForEach(file => file.FileData = Array.Empty<byte>());

                    _loadedPluginMeta[plugin.Guid] = (lastModified, plugin.Version);
                    discoveredPlugins[plugin.Guid] = (plugin, lastModified, plugin.Version);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load plugin file: {File}", file);
                }
            }

            lock (_lock)
            {
                _plugins.Clear();
                _plugins.AddRange(discoveredPlugins.Values.Select(v => v.Plugin));
            }

            _logger.LogInformation("Loaded {Count} unique plugins", _plugins.Count);
        }

        private void WaitForDeletion(string directoryName)
        {
            int attempts = 0;
            while (Directory.Exists(directoryName) && attempts < 6)
            {
                Thread.Sleep(150);
                attempts++;
            }
            if (Directory.Exists(directoryName))
            {
                _logger.LogWarning("Plugin directory '{Dir}' still exists after multiple delete attempts.", directoryName);
            }
        }

        public async Task SyncPluginsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<InfrabotContext>();

            // Retrieve the existing database plugins
            var existingDbPlugins = await _context.Plugins.ToListAsync();

            // Map plugins using the common Guid field
            var sourcePluginGuids = Plugins.Select(p => p.Guid).ToHashSet();
            var dbPluginGuids = existingDbPlugins.Select(p => p.Guid).ToHashSet();

            // Find plugins to add (in source but not in DB)
            var pluginsToAdd = Plugins
                .Where(sp => !dbPluginGuids.Contains(sp.Guid))
                .Select(sp => new Common.Models.Plugin
                {
                    Guid = sp.Guid,
                    Name = sp.Name,
                    PluginType = Enum.GetName(typeof(PluginType), sp.PluginType)
                })
                .ToList();

            // Find plugins to update (in both source and DB)
            var pluginsToUpdate = existingDbPlugins
                .Where(dp => sourcePluginGuids.Contains(dp.Guid))
                .ToList();

            foreach (var dbPlugin in pluginsToUpdate)
            {
                var sourcePlugin = Plugins.First(sp => sp.Guid == dbPlugin.Guid);
                dbPlugin.Name = sourcePlugin.Name;
                dbPlugin.Guid = sourcePlugin.Guid;
            }

            // Find plugins to remove (in DB but not in source)
            var pluginsToRemove = existingDbPlugins
                .Where(dp => !sourcePluginGuids.Contains(dp.Guid))
                .ToList();

            // Perform database operations
            if (pluginsToAdd.Any())
            {
                foreach (var item in pluginsToAdd)
                {
                    _context.EventLogs.Add(new Common.Models.EventLog { EventType = Common.Enums.EventLogType.Info, CreatedDate = DateTime.Now, Description = $"New plugin {item.Name} with Guid {item.Guid} installed." });
                }

                await _context.Plugins.AddRangeAsync(pluginsToAdd);
            }

            if (pluginsToRemove.Any())
            {
                foreach (var item in pluginsToRemove)
                {
                    _context.EventLogs.Add(new Common.Models.EventLog { EventType = Common.Enums.EventLogType.Info, CreatedDate = DateTime.Now, Description = $"Plugin {item.Name} with Guid {item.Guid} removed." });
                }

                _context.Plugins.RemoveRange(pluginsToRemove);
            }

            // Save changes to the database
            await _context.SaveChangesAsync();
        }

        private string NormalizePluginPath(string path)
        {
            return Path.IsPathRooted(path)
                ? path
                : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, path));
        }

        public string GetPluginDirectory()
        {
            return _pluginDirectory;
        }
    }
}
