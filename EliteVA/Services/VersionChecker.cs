using System.Net.Http;
using System.Reflection;
using EliteVA.Proxy;
using EliteVA.Proxy.Abstractions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace EliteVA.Services;

public class VersionChecker : VoiceAttackService
{
	private readonly ILogger<VersionChecker> _log;
	private readonly HttpClient _http;

	public VersionChecker(ILogger<VersionChecker> log, IHttpClientFactory httpClientFactory)
	{
		_log = log;
		_http = httpClientFactory.CreateClient();
	}
	public override async Task OnStart(IVoiceAttackProxy proxy)
	{
		var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
		
		_log.LogDebug("Checking for EliteVA updates. Current version: v{CurrentVersion}", currentVersion);
		
		var latestVersion = await GetLatestVersion();
		
		if (latestVersion > currentVersion)
		{
			_log.LogWarning("An update is available: v{LatestVersion}", latestVersion);
		}
		else if (latestVersion == currentVersion)
		{
			_log.LogDebug("EliteVA is up to date");
		}
		else
		{
			_log.LogWarning("Running pre-release version v{CurrentVersion}", currentVersion);
		}
	}
	
	 private async Task<Version> GetLatestVersion()
	 {
		 try
		 {
			 _http.DefaultRequestHeaders.Add("User-Agent", "EliteVA");
			 var response =
				 await _http.GetAsync(
					 "https://api.github.com/repos/Somfic/EliteVA/releases/latest");

			 var json = await response.Content.ReadAsStringAsync();
			 var jObject = JObject.Parse(json);
			 
			 var versionTxt = jObject["tag_name"].ToString();
			 
			 if(versionTxt.StartsWith("v"))
				 versionTxt = versionTxt.Substring(1);
			 
			 // Add a revision number to the version to avoid a comparison error
			 if (versionTxt.Split('.').Length == 3)
				 versionTxt += ".0";

			 var version = new Version(versionTxt);
			 
			 _log.LogDebug("Latest EliteVA version is v{Version}", version);
			 
			 return version;
		 }
		 catch (Exception ex)
		 {
			 _log.LogDebug(ex, "Could not query latest EliteVA version");
			 return new Version(0, 0, 0, 0);
		 }
	 }
}