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
		success: boolean;
		message: string;
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

		invoke('update_eliteva');
	});

	let progress: number | 'fetching' | 'none' = 'none';
	let text: string = '';
</script>

<main>
	<Logo />
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
</main>

<style lang="scss">
	@import '../styles/global.scss';

	main {
		display: flex;
		flex-direction: column;
		align-items: center;
		height: 100vh;
		padding: 0 5rem;
		padding-top: 25vh;
		border: 10px solid rgba(255, 255, 255, 0.1);
	}

	h2 {
		text-align: center;
		margin-top: 2rem;
		font-size: 1.25rem;
		font-weight: 600;
		color: rgba(255, 255, 255, 0.9);
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

	@keyframes move {
		0% {
			background-position: 0 0;
		}
		100% {
			background-position: 20px 20px;
		}
	}
</style>
