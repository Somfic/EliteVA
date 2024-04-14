using EliteAPI.EDDN;
using EliteVA.Proxy;
using EliteVA.Proxy.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EliteVA.Services.Eddn;

public class DataNetworkService : VoiceAttackService
{
	private readonly ILogger<DataNetworkService> _log;
	private readonly IConfiguration _config;
	private readonly EliteDangerousApiEddnBridge _eddn;

	public DataNetworkService(ILogger<DataNetworkService> log, IConfiguration config, EliteDangerousApiEddnBridge eddn)
	{
		_log = log;
		_config = config;
		_eddn = eddn;
	}
	
	public override async Task OnStart(IVoiceAttackProxy proxy)
	{
		var eddnEnabled = _config.GetSection("EliteAPI").GetValue("Eddn", true);

		if (eddnEnabled)
		{
			_log.LogDebug("Bridging event data with EDDN");
			await _eddn.StartAsync("EliteVA", GetType().Assembly.GetName().Version.ToString());
		}
		else
		{
			_log.LogDebug("EDDN bridge is disabled, skipping");
		}
	}
}