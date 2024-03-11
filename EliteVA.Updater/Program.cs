using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using Spectre.Console;

namespace EliteVA.Updater;

public static class Program
{
	public static async Task Main(string[] args)
	{
		AnsiConsole.Write(
			new FigletText("EliteVA")
				.Centered()
				.Color(Color.Pink1));
		
		AnsiConsole.MarkupLine("[bold]EliteVA Updater[/]");
		
		var baseFolder = (string)Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("VoiceAttack.com")?.OpenSubKey("VoiceAttack")?.GetValue("installpath");
		
		var http = new HttpClient();
        
		await AnsiConsole.Progress()
			.StartAsync(async ctx =>
			{
				// Force quit VoiceAttack
				var quitVoiceAttackTask = ctx.AddTask("[green]Quitting VoiceAttack[/]");
				quitVoiceAttackTask.IsIndeterminate = true;
				
				var voiceAttackProcesses = System.Diagnostics.Process.GetProcessesByName("VoiceAttack");
				foreach (var process in voiceAttackProcesses)
				{
					process.Kill();
				}

				while (voiceAttackProcesses.Any(x => !x.HasExited))
				{
					await Task.Delay(100);
				}
				
				// Search for releases
				var searchTask = ctx.AddTask("[green]Searching for releases[/]");
				searchTask.IsIndeterminate = true;
				
				http.DefaultRequestHeaders.Add("User-Agent", "EliteVA");
				var message = await http.GetAsync("https://api.github.com/repos/Somfic/EliteVA/releases/latest");
				
				await Task.Delay(1000);
				searchTask.Value = 100;
				searchTask.StopTask();
				
				// Parse release information
				var parseTask = ctx.AddTask("[green]Parsing release information[/]");
				parseTask.IsIndeterminate = true;
				
				var release = JsonConvert.DeserializeObject<ReleaseResponse>(await message.Content.ReadAsStringAsync());
				var asset = release.Assets.First(x => x.Name.EndsWith(".zip"));
				
				await Task.Delay(1000);
				parseTask.Value = 100;
				parseTask.StopTask();
				
				// Download release
				var downloadTask = ctx.AddTask("[green]Downloading release[/]");
				downloadTask.IsIndeterminate = false;
				
				if(File.Exists("EliteVA.zip"))
					File.Delete("EliteVA.zip");
                
				var file = File.OpenWrite("EliteVA.zip");
                
				http.DefaultRequestHeaders.Add("User-Agent", "EliteVA");
				await http.DownloadAsync(asset.BrowserDownloadUrl.ToString(), file, new Progress<float>(p => downloadTask.Value = p * 100));

				await file.FlushAsync();
				file.Close();
				
				await Task.Delay(1000);
				downloadTask.Value = 100;
				downloadTask.StopTask();
				
				// Extract release
				var extractTask = ctx.AddTask("[green]Extracting release[/]");
				extractTask.IsIndeterminate = true;
				
				ExtractZipFile(File.ReadAllBytes("EliteVA.zip"),  Path.Combine(baseFolder, "Apps", "EliteVA"));
				
				await Task.Delay(1000);
				extractTask.Value = 100;
				extractTask.StopTask();
				
				// Clean up
				var cleanupTask = ctx.AddTask("[green]Cleaning up[/]");
				cleanupTask.Description = "[green]Cleaning up[/]";
				cleanupTask.IsIndeterminate = true;
				
				File.Delete("EliteVA.zip");
				
				await Task.Delay(1000);
				cleanupTask.Value = 100;
				cleanupTask.StopTask();
				
				var startVoiceAttackTask = ctx.AddTask("[green]Starting VoiceAttack[/]");
				startVoiceAttackTask.IsIndeterminate = true;
				
				var voiceAttackPath = Path.Combine(baseFolder, "VoiceAttack.exe");
				if (File.Exists(voiceAttackPath))
				{
					System.Diagnostics.Process.Start(voiceAttackPath);
				}
				
				// Wait for VoiceAttack to start
				while (System.Diagnostics.Process.GetProcessesByName("VoiceAttack").Length == 0)
				{
					await Task.Delay(100);
				}
				
				await Task.Delay(1000);
				startVoiceAttackTask.Value = 100;
				startVoiceAttackTask.StopTask();
				
				await Task.Delay(3000);
			});
	}

	private static void ExtractZipFile(byte[] zipFileData, string targetDirectory, int bufferSize = 256 * 1024)
	{
		Directory.CreateDirectory(targetDirectory);

		using var fileStream = new MemoryStream();
		fileStream.Write(zipFileData, 0, zipFileData.Length);
		fileStream.Flush();
		fileStream.Seek(0, SeekOrigin.Begin);

		var zipFile = new ZipFile(fileStream);

		foreach (ZipEntry entry in zipFile)
		{
			var targetFile = Path.Combine(targetDirectory, entry.Name);
				
			var fileInfo = new FileInfo(targetFile);
				
			Directory.CreateDirectory(fileInfo.Directory.FullName);

			// Skip ini files if they already exist
			if (fileInfo.Extension == ".ini" && fileInfo.Exists)
				continue;
			
			using var outputFile = File.Create(targetFile);
			
			if (entry.Size <= 0) continue;
			
			var zippedStream = zipFile.GetInputStream(entry);
			var dataBuffer = new byte[bufferSize];

			int readBytes;
			do
			{
				readBytes = zippedStream.Read(dataBuffer, 0, bufferSize);
				outputFile.Write(dataBuffer, 0, readBytes);
				outputFile.Flush();
			} while (readBytes > 0);
		}
	}
}

public static class HttpClientExtensions
{
	public static async Task DownloadAsync(this HttpClient client, string requestUri, Stream destination, IProgress<float> progress = null, CancellationToken cancellationToken = default) {
		// Get the http headers first to examine the content length
		using (var response = await client.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead)) {
			var contentLength = response.Content.Headers.ContentLength;

			using (var download = await response.Content.ReadAsStreamAsync()) {

				// Ignore progress reporting when no progress reporter was 
				// passed or when the content length is unknown
				if (progress == null || !contentLength.HasValue) {
					await download.CopyToAsync(destination);
					return;
				}

				// Convert absolute progress (bytes downloaded) into relative progress (0% - 100%)
				var relativeProgress = new Progress<long>(totalBytes => progress.Report((float)totalBytes / contentLength.Value));
				// Use extension method to report progress while downloading
				await download.CopyToAsync(destination, 81920, relativeProgress, cancellationToken);
				progress.Report(1);
			}
		}
	}
}

public static class StreamExtensions
{
	public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long> progress = null, CancellationToken cancellationToken = default) {
		if (source == null)
			throw new ArgumentNullException(nameof(source));
		if (!source.CanRead)
			throw new ArgumentException("Has to be readable", nameof(source));
		if (destination == null)
			throw new ArgumentNullException(nameof(destination));
		if (!destination.CanWrite)
			throw new ArgumentException("Has to be writable", nameof(destination));
		if (bufferSize < 0)
			throw new ArgumentOutOfRangeException(nameof(bufferSize));

		var buffer = new byte[bufferSize];
		long totalBytesRead = 0;
		int bytesRead;
		while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0) {
			await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
			totalBytesRead += bytesRead;
			progress?.Report(totalBytesRead);
		}
	}
}

