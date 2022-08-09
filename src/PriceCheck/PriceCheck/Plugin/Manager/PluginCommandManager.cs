using Dalamud.Game.Command;

namespace PriceCheck
{
    /// <summary>
    /// Manage plugin commands.
    /// </summary>
    public class PluginCommandManager
    {
        private readonly PriceCheckPlugin plugin;

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginCommandManager"/> class.
        /// </summary>
        /// <param name="plugin">plugin.</param>
        public PluginCommandManager(PriceCheckPlugin plugin)
        {
            this.plugin = plugin;
            PriceCheckPlugin.CommandManager.AddHandler("/garlandconfig", new CommandInfo(this.ToggleConfig)
            {
                HelpMessage = "Show Search on Garland config.",
                ShowInHelp = true,
            });
            PriceCheckPlugin.CommandManager.AddHandler("/sogconfig", new CommandInfo(this.ToggleConfig)
            {
                ShowInHelp = false,
            });
        }

        /// <summary>
        /// Dispose command manager.
        /// </summary>
        public static void Dispose()
        {
            PriceCheckPlugin.CommandManager.RemoveHandler("/sogconfig");
            PriceCheckPlugin.CommandManager.RemoveHandler("/garlandconfig");
        }

        private void ToggleConfig(string command, string args)
        {
            this.plugin.WindowManager.ConfigWindow!.Toggle();
        }
    }
}
