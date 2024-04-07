// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use std::{fs::File, io::Write, process::Command};

use futures_util::StreamExt;
use tauri::{Manager, Window};
use window_vibrancy::{apply_acrylic, apply_mica, apply_vibrancy, NSVisualEffectMaterial};

#[derive(Clone, serde::Serialize)]
struct MessagePayload {
    message: String,
    progress: u8,
    is_fetching: bool,
}

#[derive(Clone, serde::Serialize)]
struct FinishedPayload {
    success: bool,
    message: String,
}

fn set_progess(window: &Window, message: impl Into<String>, progress: u8) {
    window
        .emit_all(
            "message",
            MessagePayload {
                message: message.into(),
                progress,
                is_fetching: false,
            },
        )
        .unwrap();
}

fn set_fetching(window: &Window, message: impl Into<String>) {
    window
        .emit_all(
            "message",
            MessagePayload {
                message: message.into(),
                progress: 0,
                is_fetching: true,
            },
        )
        .unwrap();
}

fn set_finished(window: &Window, success: bool, message: impl Into<String>) {
    window
        .emit_all(
            "finished",
            FinishedPayload {
                success,
                message: message.into(),
            },
        )
        .unwrap();

    // Wait for 5 seconds before closing the window
    std::thread::sleep(std::time::Duration::from_secs(5));
    window.close().unwrap();
    // Exit the application
    std::process::exit(0);
}

#[tauri::command]
async fn update_eliteva(window: Window) {
    // Stop VoiceAttack.exe
    set_fetching(&window, "Stopping VoiceAttack");

    // Stop the VoiceAttack process
    Command::new("taskkill")
        .args(&["/IM", "VoiceAttack.exe", "/F"])
        .output()
        .unwrap();

    let voiceattack_dir = match winreg::RegKey::predef(winreg::enums::HKEY_CURRENT_USER)
        .open_subkey(r"Software\VoiceAttack.com\VoiceAttack")
    {
        Ok(key) => key.get_value::<String, _>("installpath").unwrap(),
        Err(_) => {
            set_finished(&window, false, "Could not find EliteVA directory");
            return;
        }
    };

    let eliteva_dir = format!("{}/Apps/EliteVA", voiceattack_dir);
    let eliteva_dir = std::path::Path::new(&eliteva_dir);

    if !std::path::Path::new(&eliteva_dir).exists() {
        std::fs::create_dir_all(&eliteva_dir).unwrap();
    }

    set_fetching(&window, "Fetching latest version of EliteVA");
    let latest_release = octocrab::instance()
        .repos("Somfic", "EliteVA")
        .releases()
        .get_latest()
        .await;

    if latest_release.is_err() {
        set_finished(&window, false, latest_release.unwrap_err().to_string());
        return;
    }

    let latest_release = latest_release.unwrap();
    let latest_version = latest_release.tag_name;
    // Find the first asset that is a zip file
    let download_url = &latest_release
        .assets
        .iter()
        .find(|asset| asset.name.ends_with(".zip"))
        .unwrap()
        .browser_download_url;

    set_progess(
        &window,
        format!("Downloading EliteVA v{}", latest_version),
        0,
    );

    let client = reqwest::Client::new();

    let download_response = client.get(download_url.clone()).send().await;
    if download_response.is_err() {
        set_finished(&window, false, download_response.unwrap_err().to_string());
        return;
    }
    let download_response = download_response.unwrap();

    let download_size = download_response.content_length().unwrap_or(0);

    // Download the file to the EliteVA directory
    let path = &eliteva_dir.join(format!("eliteva_v{}.zip", latest_version));

    let mut file = File::create(path).unwrap();
    let mut downloaded: u64 = 0;
    let mut stream = download_response.bytes_stream();

    while let Some(chunk) = stream.next().await {
        let chunk = chunk.unwrap();
        downloaded += chunk.len() as u64;
        let progress = ((downloaded as f64 / download_size as f64) * 100.0) as u8;
        set_progess(
            &window,
            format!("Downloading EliteVA v{}", latest_version),
            progress,
        );
        file.write_all(&chunk).unwrap();
    }

    // Unzip the downloaded file
    set_fetching(&window, "Extracting EliteVA");

    let file = std::fs::File::open(path.clone()).unwrap();
    let mut archive = zip::ZipArchive::new(file).unwrap();

    for i in 0..archive.len() {
        let mut file = archive.by_index(i).unwrap();
        let outpath = eliteva_dir.join(file.sanitized_name());

        set_fetching(
            &window,
            format!(
                "Extracting EliteVA v{}: {}",
                latest_version,
                outpath.display()
            ),
        );

        if file.is_dir() {
            std::fs::create_dir_all(&outpath).unwrap();
        } else {
            if let Some(p) = outpath.parent() {
                if !p.exists() {
                    std::fs::create_dir_all(&p).unwrap();
                }
            }
            let mut outfile = std::fs::File::create(&outpath).unwrap();
            std::io::copy(&mut file, &mut outfile).unwrap();
        }
    }

    // Remove the downloaded zip file
    std::fs::remove_file(path).unwrap();

    set_fetching(&window, "Starting VoiceAttack");

    let voiceattack_exe = format!("{}/VoiceAttack.exe", voiceattack_dir);
    let _voiceattack = std::process::Command::new(voiceattack_exe).spawn().unwrap();

    std::thread::sleep(std::time::Duration::from_secs(1));

    set_finished(
        &window,
        true,
        format!(
            "EliteVA has been updated to v{} successfully",
            latest_version
        ),
    );

    window.close().unwrap();
    std::process::exit(0);
}

fn main() {
    tauri::Builder::default()
        .invoke_handler(tauri::generate_handler![update_eliteva])
        .setup(|app| {
            let window = app.get_window("main").unwrap();

            #[cfg(target_os = "windows")]
            apply_acrylic(&window, Some((0, 0, 0, 255)))
                .expect("Unsupported platform! 'apply_mica' is only supported on Windows");

            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
