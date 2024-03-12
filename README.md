 <img src="https://repository-images.githubusercontent.com/529349259/227c444b-984e-48b3-8a09-5ad9282b36a5"/>

<div style="text-align: center;">
  <h1 align="center">EliteVA</h1>

  <p align="center"><i>An intelligent VoiceAttack plugin for Elite: Dangerous, powered by <a href="https://www.github.com/EliteAPI/EliteAPI">EliteAPI</a></i></p>

  <p align="center">
       <a href="https://www.discord.gg/jwpFUPZ">
            <img alt="Discord" src="https://img.shields.io/discord/498422961297031168?color=%23f2a529&label=DISCORD&style=for-the-badge">
       </a>
       <a href="https://github.com/EliteAPI/EliteVA/releases">
          <img alt="GitHub release" src="https://img.shields.io/github/v/release/Somfic/EliteVA?color=%23f2a529&label=VERSION&style=for-the-badge">
       </a>
       <a href="https://github.com/EliteAPI/EliteVA/blob/master/LICENSE">
           <img alt="GitHub" src="https://img.shields.io/github/license/Somfic/EliteVA?color=%23f2a529&label=LICENSE&style=for-the-badge">
       </a>
  </p>
  
  <p>EliteVA is a VoiceAttack plugin specifically designed for Elite: Dangerous, enhancing your macros with events, variables, and keybindings support. By leveraging EliteAPI, it empowers you to create a truly intelligent voice assistant for your game, using VoiceAttack.</p>
</div>

## Why EliteVA?
Consider the scenario of retracting your landing gear using a VoiceAttack macro.

### Without EliteVA
Traditionally, the macro would be quite simple:

```
Press the G key
```

However, this approach lacks intelligence. What if the landing gear is already retracted or your commander is currently in supercruise? And what if the landing gear key is not actually the `G` key? In these situations, the macro would fail. 

### With EliteVA
By using EliteVA, you can significantly improve the intelligence of your macro. Here's how:

1. Check if the landing gear is not already retracted:

```
Boolean compare: EliteAPI.Gear equals True
```

2. Verify if the commander is in normal non-supercruise space:

```
Boolean compare: EliteAPI.Supercruise equals False
```

3. Trigger the appropriate landing gear key:

```
Press variable key: [EliteAPI.LandingGearToggle]
```

By incorporating these checks into your profile, you can create a much smarter and more reliable voice assistant for your game.

## Getting Started
Let's help you get started, commander.

## Installation
EliteVA is distributed through GitHub, making it the recommended method for installation. Alternatively, you can compile the plugin to retrieve the plugin file.

To install, follow these steps:
1. Download the [EliteVA-setup.bat](https://github.com/Somfic/EliteVA/releases/latest) file and run it. The setup will automatically detect your VoiceAttack installation.
2. Ensure that **Plugin Support** is enabled in VoiceAttack.
3. After restarting VoiceAttack, the EliteVA plugin will be ready to use.

## Events
EliteVA converts numerous in-game events into macro commands. For example, retracting your gear will trigger the `((EliteAPI.Ship.Gear))` command, while cracking an asteroid will trigger the `((EliteAPI.AsteroidCracked))` macro.

## Variables
EliteVA provides access to various variables synced with the game. For instance, `{BOOL:EliteAPI.Gear}` holds the value of the ship's landing gear status, and `{BOOL:MassLocked}` indicates whether you're currently mass-locked. EliteVA outputs set variables in the Variables folder, which is generated when running the plugin.

## Bindings

EliteVA makes all in-game keybindings available and keeps them updated whenever changes are made to the keybindings preset in-game. Instead of blindly pressing the `G` key to retract the landing gear, your macro can use the `{TXT:EliteAPI.LandingGearToggle}` variable to press the actual key assigned to the gear.

A list of all supported keybindings can be found in the `Bindings.txt` file within the generated Variables folder.

**Disclaimer**
```
Please note that VoiceAttack does not support external Joystick or Hotas triggers.
The plugin will only expose keyboard keybindings.
Make sure that either the primary or secondary keybinding is set to a keyboard key.
```

