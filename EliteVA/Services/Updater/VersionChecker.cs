using System.Net.Http;
using System.Reflection;
using EliteVA.Proxy;
using EliteVA.Proxy.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EliteVA.Services.Updater;

public class VersionChecker : VoiceAttackService
{
	private readonly ILogger<VersionChecker> _log;
	private readonly IConfiguration _configuration;
	private readonly HttpClient _http;

	public VersionChecker(ILogger<VersionChecker> log, IHttpClientFactory httpClientFactory, IConfiguration configuration)
	{
		_log = log;
		_configuration = configuration;
		_http = httpClientFactory.CreateClient();
	}
	public override async Task OnStart(IVoiceAttackProxy proxy)
	{
		var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
		
		_log.LogDebug("Checking for EliteVA updates. Current version: v{CurrentVersion}", currentVersion);
		
		var (release, version) = await GetLatestRelease();
		
		_log.LogDebug("Latest version is v{LatestVersion}", version);
		
		if (version > currentVersion)
		{
			_log.LogWarning("An update is available: v{LatestVersion}", version);
			
			// Download setup.exe
			var asset = release.Assets.FirstOrDefault(x => x.Name.EndsWith(".bat"));
			
			if (asset == null)
			{
				_log.LogDebug("Could not find a setup file for the latest version. Continuing with the current version");
				return;
			}

			if (_configuration.GetSection("EliteAPI").GetValue("AutoUpdate", true))
			{
				_log.LogDebug("Auto update is enabled");
				await DownloadAndRunFile(asset, release.TagName);
			}
		}
	}
	
	 private async Task<(ReleaseResponse release, Version version)> GetLatestRelease()
	 {
		 try
		 {
			 _http.DefaultRequestHeaders.Add("User-Agent", "EliteVA");
			 var response =
				 await _http.GetAsync(
					 "https://api.github.com/repos/Somfic/EliteVA/releases/latest");

			 var release = JsonConvert.DeserializeObject<ReleaseResponse>(await response.Content.ReadAsStringAsync());

			 var versionTxt = release.TagName;
			 
			 if(versionTxt.StartsWith("v"))
				 versionTxt = versionTxt.Substring(1);
			 
			 // Add a revision number to the version to avoid a comparison error
			 if (versionTxt.Split('.').Length == 3)
				 versionTxt += ".0";

			 var version = new Version(versionTxt);
			 
			 _log.LogDebug("Latest EliteVA version is v{Version}", version);
			 
			 return (release, version);
		 }
		 catch (Exception ex)
		 {
			 _log.LogDebug(ex, "Could not query latest EliteVA version");
			 return default;
		 }
	 }

	 private async Task DownloadAndRunFile(Asset asset, string hash)
	 {
		 // Make sure we're not in a update loop
		 if (File.Exists("last-update"))
		 {
			 var lastHash = File.ReadAllText("last-update");
			 if (lastHash == hash)
			 {
				 _log.LogDebug("Already downloaded the latest version. Skipping update");
				 return;
			 }
		 }
		 
		 File.WriteAllText("last-update", hash);
		 
		 _log.LogDebug("Downloading setup file from {Url}", asset.BrowserDownloadUrl);
		 
		 _http.DefaultRequestHeaders.Add("User-Agent", "EliteVA");
		 var response = await _http.GetAsync(asset.BrowserDownloadUrl);
		 var setup = await response.Content.ReadAsByteArrayAsync();
		  
		 File.WriteAllBytes(asset.Name, setup);
		 
		 _log.LogDebug("Running setup ... This process will exit soon ... ");
		 
		 // Start setup
		 System.Diagnostics.Process.Start(asset.Name);
	 }
}