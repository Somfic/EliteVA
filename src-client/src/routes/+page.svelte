<script lang="ts">
	import SvelteMarkdown from 'svelte-markdown';
	import { invoke } from '@tauri-apps/api/tauri';
	import { listen } from '@tauri-apps/api/event';
	import { onMount } from 'svelte';

	interface Message {
		message: string;
		progress: number;
		is_fetching: boolean;
	}

	interface Finished {
		message: string;
	}

	interface Error {
		message: string;
		error_message: string;
	}

	interface NewVersion {
		title: string;
		version: string;
		changelog: string;
		url: string;
	}

	onMount(() => {
		progress = 'fetching';
		text = 'Starting updater ... ';

		listen('new_version', (event) => {
			let message = event.payload as NewVersion;
			title = message.title;
			version = message.version;
			changelog = message.changelog;
			url = message.url;

			invoke('show_window');
		});

		listen('message', (event) => {
			let message = event.payload as Message;
			progress = message.is_fetching ? 'fetching' : message.progress;
			text = message.message;
		});

		listen('finished', (event) => {
			let message = event.payload as Finished;
			progress = 'none';
			text = message.message;
		});

		listen('error', (event) => {
			isPrompting = false;
			let message = event.payload as Error;
			progress = 'none';
			text = message.message;
			error_message = message.error_message;
		});

		invoke('get_new_version');
	});

	let isPrompting = true;

	let title: string;
	let version: string;
	let changelog: string;
	let url: string;

	let progress: number | 'fetching' | 'none' = 'none';
	let text: string = '';
	let error_message = '';

	function copy() {
		// Copy error message to clipboard
		navigator.clipboard.writeText(error_message);
	}

	function updateNow() {
		isPrompting = false;
		invoke('update_now');
	}

	function updateLater() {
		isPrompting = false;
		invoke('update_later');
	}

	function skipUpdate() {
		isPrompting = false;
		invoke('skip_update');
	}

	function close() {
		invoke('close_window');
	}
</script>

<main class:prompt={isPrompting}>
	{#if isPrompting}
		<div class="header">
			<h3>A new EliteVA update is available!</h3>
			<h1>{title}</h1>
			<h2 class="pill">EliteVA {version}</h2>
		</div>
		<div class="content">
			<p><b>Changelog</b></p>
			<SvelteMarkdown source={changelog} />
			<!-- <a href={url} target="_blank">See GitHub release</a> -->
		</div>
		<div class="footer">
			<div class="buttons">
				<button class="main" on:click={() => updateNow()}>Update now</button>
				<button on:click={() => updateLater()}>Skip this version</button>
				<!-- <button on:click={() => skipUpdate()}>Skip this version</button> -->
			</div>
		</div>
	{:else}
		<div class="header">
			<img src="/icon.png" />
			<h3>VoiceAttack plugin for Elite: Dangerous</h3>
		</div>
		<div class="content">
			<div class="progress" class:none={progress == 'none'}>
				{#if progress === 'none'}
					<div class="progress-bar" style="width: 0%"></div>
				{:else if progress === 'fetching'}
					<div class="progress-bar fetching" style="width: 100%"></div>
				{:else}
					<div class="progress-bar number" style="width: {progress}%"></div>
				{/if}
			</div>
		</div>
		<div class="footer">
			<h3>{text}</h3>
			<div class="buttons">
				{#if error_message}
					<button on:click={() => copy()}>Copy error code</button>
				{:else}
					<button on:click={() => close()}>Close</button>
				{/if}
			</div>
		</div>
	{/if}
</main>

<style lang="scss">
	@import '../styles/global.scss';

	main {
		display: flex;
		flex-direction: column;
		align-items: center;
		height: 100vh;
		padding: 2rem 4rem;

		.header {
			text-align: center;

			h3 {
				opacity: 0.5;
				font-weight: 500;
				font-size: 1rem;
			}
		}

		img {
			max-width: 100%;
		}

		.content {
			flex-grow: 1;
			margin-top: 4rem;
		}

		.footer {
		}
	}

	.title {
		display: flex;
		flex-direction: column;
		gap: 1rem;

		h1,
		h2,
		h3 {
			margin: 0;
		}

		h1 {
			font-size: 2rem;
		}

		h2 {
			font-size: 1.5rem;
			opacity: 0.5;
		}
	}

	.progress {
		margin-top: 10rem;
		width: 100%;
		height: 10px;
		background-color: $background;
		border-radius: 10px;
		border: $border;
		transition: all 250ms ease-in-out;

		.progress-bar {
			height: 100%;
			background-color: rgba(255, 255, 255, 0.5);
			border-radius: 10px;
			//filter: drop-shadow(0 0 5px rgba(255, 255, 255, 0.5));
			transition: all 250ms ease-in-out;

			// Diagonal stripes that move
			&.fetching {
				background-image: linear-gradient(
					-45deg,
					rgba(0, 0, 0, 0.5) 25%,
					transparent 25%,
					transparent 50%,
					rgba(0, 0, 0, 0.5) 50%,
					rgba(0, 0, 0, 0.5) 75%,
					transparent 75%,
					transparent
				);
				background-size: 20px 20px;
				animation: move 1s linear infinite; // 2s duration, linear timing, infinite loop
			}
		}

		&.none {
			opacity: 0;
		}
	}

	.buttons {
		display: flex;
		flex-grow: 1;
		align-items: center;
		justify-content: center;
	}

	.pill,
	button {
		font-size: 0.9em;
		opacity: 0.85;
		margin: 0 0.2rem;

		background-color: rgba(255, 255, 255, 0.1);
		border-radius: 10px;
		border: 2px solid rgba(255, 255, 255, 0.25);
		padding: 5px 12px;
		transition: 200ms ease;
	}

	button {
		cursor: pointer;

		&.main {
			background-color: $accent;
			border: 2px solid rgba(255, 255, 255, 0.5);
			font-weight: bold;

			&:hover {
				background-color: lighten($accent, 0.6);
			}
		}

		&:hover {
			background-color: rgba(255, 255, 255, 0.25);
		}

		&:active {
			transform: scale(0.95);
		}
	}

	a {
		color: $accent;
		cursor: pointer;
		text-decoration: underline;
	}

	@keyframes move {
		0% {
			background-position: 0 0;
		}
		100% {
			background-position: 20px 20px;
		}
	}
</style>
