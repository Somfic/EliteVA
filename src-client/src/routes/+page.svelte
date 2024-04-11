<script lang="ts">
	import Logo from '$lib/Logo.svelte';

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

	onMount(() => {
		progress = 'fetching';
		text = 'Starting updater ... ';

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
			let message = event.payload as Error;
			progress = 'none';
			text = message.message;
			error_message = message.error_message;
		});

		invoke('update_eliteva');
	});

	let progress: number | 'fetching' | 'none' = 'none';
	let text: string = '';
	let error_message = '';

	function copy() {
		// Copy error message to clipboard
		navigator.clipboard.writeText(error_message);
	}
</script>

<main>
	<Logo />
	<h3>VoiceAttack plugin for Elite: Dangerous</h3>
	<div class="progress" class:none={progress == 'none'}>
		{#if progress === 'none'}
			<div class="progress-bar" style="width: 0%"></div>
		{:else if progress === 'fetching'}
			<div class="progress-bar fetching" style="width: 100%"></div>
		{:else}
			<div class="progress-bar number" style="width: {progress}%"></div>
		{/if}
	</div>
	<h2>{text}</h2>
	{#if error_message}
		<button on:click={() => copy()}>Copy error code</button>
	{/if}
</main>

<style lang="scss">
	@import '../styles/global.scss';

	main {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		margin-top: 80px;
		height: 100vh;
		padding: 0 5rem;
	}

	h2 {
		text-align: center;
		margin-top: 2rem;
		font-size: 1.25rem;
		font-weight: 600;
		color: rgba(255, 255, 255, 0.9);
	}

	h3 {
		text-align: center;
		margin-top: 0;
		font-size: 1rem;
		font-weight: 500;
		color: rgba(255, 255, 255, 0.85);
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

	button {
		font-size: 0.8em;
		opacity: 0.85;
		cursor: pointer;
		background-color: rgba(255, 255, 255, 0.1);
		border-radius: 10px;
		border: 2px solid rgba(255, 255, 255, 0.25);
		padding: 5px 10px;
		transition: 200ms ease;

		&:hover {
			background-color: rgba(255, 255, 255, 0.25);
		}

		&:active {
			transform: scale(0.95);
		}
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
