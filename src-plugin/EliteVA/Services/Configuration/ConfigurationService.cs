using EliteVA.Proxy;
using EliteVA.Proxy.Abstractions;

namespace EliteVA.Services.Configuration;

public class ConfigurationService : VoiceAttackService
{
	private static readonly string ConfigPath = Path.Combine(VoiceAttackPlugin.Proxy.Paths.Plugins.FullName, "EliteVA", "EliteAPI.ini");

	private const string DefaultConfig = """
	                                      [EliteAPI]
	                                      # This file is used to configure the EliteAPI. Changes will take effect after restarting VoiceAttack.
	                                      # Remove the semicolon to change a setting

	                                      # Whether or not to automatically update the plugin when a new version is available.
	                                      #  true (default) will automatically update the plugin
	                                      #  false will disable automatic updates
	                                      AutoUpdate = true

	                                      # Whether or not to enable the Elite Dangerous Data Network (EDDN) bridge. This is used to send event data to EDDN.
	                                      #  true (default) will enable the EDDN bridge
	                                      #  false will disable the EDDN bridge
	                                      Eddn = true
	                                      
	                                      # Whether or not to enable Discord Rich Presence. This is used to show your current in-game status on Discord.
	                                      #  true (default) will enable Discord Rich Presence
	                                      #  false will disable Discord Rich Presence
	                                      DiscordRichPresence = true
	                                      
	                                      # The amount of time in milliseconds to wait between each checking for new event data.
	                                      #  500 (default) is the recommended value.
	                                      #  Setting this to an arbitrarily low value may cause increased CPU usage, while setting it to a high value may cause delays in events.
	                                      UpdateDelay = 500

	                                      # Path to the Elite Dangerous journals folder. This is used to get event data.
	                                      #  Leave this blank to use the default path.
	                                      ; JournalsPath = "C:\Users\Commander\Saved Games\Frontier Developments\Elite Dangerous"

	                                      # Path to the Elite Dangerous options folder. This is used to get keybindings and other settings.
	                                      #  Leave this blank to use the default path.
	                                      ; OptionsPath = "C:\Users\Commander\AppData\Local\Frontier Developments\Elite Dangerous\Options"

	                                      # Amount of Journals to scrape for record generation.
	                                      #  1 (default) will only scrape the latest journal
	                                      ; AmountJournalsToScrape = 1

	                                      # Whether or not to log events to a file. This is useful for debugging and reporting issues with the plugin.
	                                      #  true (default) will log to file
	                                      #  false will disable logging
	                                      FileLogging = true
	                                      """; // Default configuration

	public override Task OnStart(IVoiceAttackProxy proxy)
	{
		if (!File.Exists(ConfigPath))
		{
			File.WriteAllText(ConfigPath, DefaultConfig);
		}

		return Task.CompletedTask;
	}
}