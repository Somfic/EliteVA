// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use std::{fs::File, io::Write, process::Command};

use downloader::download_new_version;
use futures_util::StreamExt;
use tauri::{Manager, Window};
use window_vibrancy::{apply_acrylic, apply_mica, apply_vibrancy, NSVisualEffectMaterial};

pub mod downloader;

#[derive(Clone, serde::Serialize)]
struct MessagePayload {
    message: String,
    progress: u8,
    is_fetching: bool,
}

#[derive(Clone, serde::Serialize)]
struct FinishedPayload {
    message: String,
}

#[derive(Clone, serde::Serialize)]
struct ErrorPayload {
    message: String,
    error_message: String,
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

fn set_finished(window: &Window, message: impl Into<String>) {
    window
        .emit_all(
            "finished",
            FinishedPayload {
                message: message.into(),
            },
        )
        .unwrap();

    // Wait for 5 seconds before closing the window
    std::thread::sleep(std::time::Duration::from_secs(5));
    window.close().unwrap();
    std::process::exit(0);
}

fn set_error(window: &Window, error: String) {
    window
        .emit_all(
            "error",
            ErrorPayload {
                message: "An error occurred!".to_string(),
                error_message: error,
            },
        )
        .unwrap();
}

#[tauri::command]
async fn update_eliteva(window: Window) {
    match download_new_version(&window).await {
        Ok(e) => set_finished(
            &window,
            format!("EliteVA has been updated to v{}!", e.tag_name),
        ),
        Err(e) => {
            println!("{:?}", e);
            set_error(&window, format!("{:?}", e))
        }
    }
}

fn main() {
    tauri::Builder::default()
        .invoke_handler(tauri::generate_handler![update_eliteva])
        .setup(|app| {
            let window = app.get_window("main").unwrap();

            #[cfg(target_os = "windows")]
            apply_acrylic(&window, Some((0, 0, 0, 0)))
                .expect("Unsupported platform! 'apply_mica' is only supported on Windows");

            Ok(())
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
