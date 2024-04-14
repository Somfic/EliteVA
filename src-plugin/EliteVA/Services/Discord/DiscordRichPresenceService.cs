using EliteAPI.Discord;
using EliteVA.Proxy;
using EliteVA.Proxy.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EliteVA.Services.Discord;

public class DiscordRichPresenceService : VoiceAttackService
{
	private readonly ILogger<DiscordRichPresenceService> _log;
	private readonly IConfiguration _config;
	private readonly EliteDangerousApiDiscordRichPresence _discord;

	public DiscordRichPresenceService(ILogger<DiscordRichPresenceService> log,  IConfiguration config, EliteDangerousApiDiscordRichPresence discord)
	{
		_log = log;
		_config = config;
		_discord = discord;
	}
	
	public override async Task OnStart(IVoiceAttackProxy proxy)
	{
		var rpcEnabled = _config.GetSection("EliteAPI").GetValue("DiscordRichPresence", true);

		if (rpcEnabled)
		{
			_log.LogDebug("Starting Discord Rich Presence");
			await _discord.StartAsync();
		}
		else
		{
			_log.LogDebug("Discord Rich Presence is disabled, skipping");
		}
	}
}