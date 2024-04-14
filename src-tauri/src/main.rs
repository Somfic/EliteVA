// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use std::{fs::File, io::Write, process::Command};

use downloader::download_new_version;
use downloader::query_latest_release;
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

#[derive(Clone, serde::Serialize)]
struct NewVersionPayload {
    title: String,
    version: String,
    changelog: String,
    url: String,
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
}

fn set_error(window: &Window, error: String, backtrace: String) {
    window
        .emit_all(
            "error",
            ErrorPayload {
                message: format!("Error: {}", error.to_string()),
                error_message: backtrace,
            },
        )
        .unwrap();
}

#[tauri::command]
async fn get_new_version(window: Window) {
    println!("Checking for new version");
    let release = query_latest_release("Somfic", "EliteVA").await;

    match release {
        Ok(e) => {
            window
                .emit_all(
                    "new_version",
                    NewVersionPayload {
                        title: e.clone().name.unwrap_or(e.clone().tag_name),
                        version: e.tag_name.clone(),
                        changelog: e.body.unwrap_or("".to_string()),
                        url: e.html_url.to_string(),
                    },
                )
                .unwrap();
        }
        Err(e) => {
            println!("{:?}", e);
            set_error(&window, e.to_string(), format!("{:?}", e));
        }
    }
}

#[tauri::command]
async fn update_now(window: Window) {
    match download_new_version(&window).await {
        Ok(e) => set_finished(
            &window,
            format!("EliteVA has been updated to v{}!", e.tag_name),
        ),
        Err(e) => {
            println!("{:?}", e);
            set_error(&window, e.to_string(), format!("{:?}", e));
        }
    }
}

#[tauri::command]
fn show_window(window: Window) {
    window.show().unwrap();
}

#[tauri::command]
async fn update_later(window: Window) -> Result<(), ()> {
    close(&window);

    Ok(())
}

fn close(window: &Window) {
    window.close().unwrap();
    std::process::exit(0);
}

fn main() {
    tauri::Builder::default()
        .invoke_handler(tauri::generate_handler![
            show_window,
            get_new_version,
            update_now,
            update_later
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
