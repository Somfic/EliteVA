use std::{any, fs::File, io::Write, path::PathBuf, process::Command};

use anyhow::{Context, Result};
use futures_util::StreamExt;
use octocrab::models::repos::Release;
use tauri::Window;

use crate::{set_fetching, set_progess};

pub async fn download_new_version(window: &Window) -> Result<Release> {
    set_fetching(&window, "Stopping VoiceAttack");
    stop_voiceattack_process()
        .map_err(|e| anyhow::anyhow!(e))
        .context("Could not stop VoiceAttack before updating EliteVA")?;

    set_fetching(&window, "Finding VoiceAttack installation directory");
    let voiceattack_root = &get_voiceattack_installation_directory()
        .context("Could not find VoiceAttack installation directory")?;

    let eliteva_root = &get_eliteva_installation_directory(voiceattack_root)
        .context("Could not find EliteVA installation directory")?;

    set_fetching(&window, "Querying latest EliteVA release");
    let release = &query_latest_release("somfic", "EliteVA")
        .await
        .context("Could not query latest release of EliteVA")?;

    set_fetching(
        &window,
        format!("Downloading EliteVA v{}", release.tag_name),
    );
    let zip_file = download_file(&window, release, ".zip", eliteva_root)
        .await
        .context("Could not download EliteVA zip file")?;

    set_fetching(&window, "Extracting EliteVA");
    unzip_to_directory(&window, &zip_file, eliteva_root)
        .context("Could not unzip downloaded zip to the EliteVA directory")?;

    set_fetching(&window, "Starting VoiceAttack");
    start_voiceattack(voiceattack_root)
        .context("Could not start VoiceAttack after updating EliteVA")?;

    Ok(release.clone())
}

fn stop_voiceattack_process() -> Result<()> {
    Command::new("taskkill")
        .args(&["/IM", "VoiceAttack.exe", "/F"])
        .output()
        .context("Failed to stop VoiceAttack process")?;

    Ok(())
}

fn get_voiceattack_installation_directory() -> Result<PathBuf> {
    let def = winreg::RegKey::predef(winreg::enums::HKEY_CURRENT_USER);
    let key = def
        .open_subkey(r"Software\VoiceAttack.com\VoiceAttack")
        .context(
        "Could not find VoiceAttack installation directory in the registry. Subkey does not exist",
    )?;

    let path = key
        .get_value::<String, _>("installpath")
        .context( "Could not find VoiceAttack installation directory in the registry. Keyvalue 'installpath' does not exist")?;

    Ok(std::path::PathBuf::from(path))
}

fn get_eliteva_installation_directory(
    voiceattack_installation_directory: &PathBuf,
) -> Result<PathBuf> {
    let directory = voiceattack_installation_directory
        .join("Apps")
        .join("EliteVA");

    // Create the EliteVA directory if it does not exist
    if !directory.exists() {
        std::fs::create_dir_all(&directory).context(format!(
            "Could not create EliteVA directory '{}'",
            directory.display()
        ))?;
    }

    Ok(directory)
}

async fn query_latest_release(owner: &str, repo: &str) -> Result<Release> {
    let release = octocrab::instance()
        .repos(owner, repo)
        .releases()
        .get_latest()
        .await
        .context(format!(
            "Could not query latest release of {} from {}",
            repo, owner
        ))?;

    Ok(release)
}

async fn download_file(
    window: &Window,
    release: &Release,
    extension: &str,
    directory: &PathBuf,
) -> Result<PathBuf> {
    let download_url = &release
        .assets
        .iter()
        .find(|asset| asset.name.ends_with(extension))
        .map(|asset| &asset.browser_download_url);

    if download_url.is_none() {
        return Err(anyhow::anyhow!(
            "Could not find '{}' asset for EliteVA v{}",
            extension,
            release.tag_name
        ));
    }

    let download_url = &download_url.unwrap().clone();

    set_progess(
        &window,
        format!("Downloading EliteVA v{}", release.tag_name),
        0,
    );

    let client = reqwest::Client::new();

    let download_response = client
        .get(download_url.clone())
        .send()
        .await
        .context(format!(
            "Could not query download URL '{}'",
            download_url.clone()
        ))?;

    let download_size = download_response.content_length().unwrap_or(0);

    // Download the file to the EliteVA directory
    let path = &directory.join(format!("eliteva_v{}.zip", release.tag_name));

    let mut file =
        File::create(path).context(format!("Could not create file '{}'", path.display()))?;

    let mut downloaded: u64 = 0;
    let mut stream = download_response.bytes_stream();

    while let Some(chunk) = stream.next().await {
        let chunk = chunk.context(format!("Invalid chunk when downloading '{}'", download_url))?;
        downloaded += chunk.len() as u64;
        let progress = ((downloaded as f64 / download_size as f64) * 100.0) as u8;
        set_progess(
            &window,
            format!("Downloading EliteVA v{}", release.tag_name),
            progress,
        );
        file.write_all(&chunk).context(format!(
            "Could not write chunk to file '{}'",
            path.display()
        ))?;
    }

    Ok(path.clone())
}

fn unzip_to_directory(window: &Window, zip_path: &PathBuf, directory: &PathBuf) -> Result<()> {
    let file = std::fs::File::open(zip_path.clone())
        .context(format!("Could not open zip file '{}'", zip_path.display()))?;
    let mut archive = zip::ZipArchive::new(file).context(format!(
        "Could not load zip archive '{}'",
        zip_path.display()
    ))?;

    for i in 0..archive.len() {
        let mut file = archive.by_index(i).context({
            format!(
                "Could not get file at index {} from zip archive '{}'",
                i,
                zip_path.display()
            )
        })?;
        let outpath = directory.join(file.sanitized_name());

        set_fetching(&window, format!("Extracting {}", outpath.display()));

        if file.is_dir() {
            std::fs::create_dir_all(&outpath).context({
                format!(
                    "Could not create directory '{}' when extracting '{}'",
                    outpath.display(),
                    zip_path.display()
                )
            })?;
        } else {
            if let Some(p) = outpath.parent() {
                if !p.exists() {
                    std::fs::create_dir_all(&p).context({
                        format!(
                            "Could not create parent directory '{}' when extracting '{}'",
                            p.display(),
                            zip_path.display()
                        )
                    })?;
                }
            }
            let mut outfile = std::fs::File::create(&outpath).context({
                format!(
                    "Could not create file '{}' when extracting '{}'",
                    outpath.display(),
                    zip_path.display()
                )
            })?;
            std::io::copy(&mut file, &mut outfile).context({
                format!(
                    "Could not write to file '{}' when extracting '{}'",
                    outpath.display(),
                    zip_path.display()
                )
            })?;
        }
    }

    // Remove the downloaded zip file
    std::fs::remove_file(zip_path).context({
        format!(
            "Could not remove downloaded zip file '{}' after extraction",
            zip_path.display()
        )
    })?;

    Ok(())
}

fn start_voiceattack(voiceattack_installation_directory: &PathBuf) -> Result<()> {
    let voiceattack_exe = &voiceattack_installation_directory.join("VoiceAttack.exe");
    std::process::Command::new(voiceattack_exe)
        .spawn()
        .context({
            format!(
                "Could not start VoiceAttack at '{}'",
                voiceattack_exe.display()
            )
        })?;

    Ok(())
}
