# Dead Island 2 Trainer

A simple Windows Forms application (C#, .NET Framework 4.7.2) for enabling cheats in **Dead Island 2** (64-bit version). This trainer allows you to toggle various in-game features such as God Mode, infinite weapon durability, stamina, and money by directly patching the game's memory.

## Features

- **God Mode**: Prevents the player from taking damage.
- **Infinite Weapon Durability**: Weapons never break.
- **Infinite Stamina**: Unlimited stamina for continuous actions.
- **Infinite Money**: Sets money to a very high value.
- *(Placeholder for future features, e.g., Infinite EXP)*

## Usage

1. **Build** the project in Visual Studio 2022 (targeting .NET Framework 4.7.2).
2. **Run** Dead Island 2 (64-bit) and load into the game.
3. **Start** the trainer application.
4. The trainer will attempt to detect the game process (`DeadIsland-Win64-Shipping.exe`).
5. Use the provided buttons to toggle cheats ON/OFF.

> **Note:** The trainer must be run with appropriate permissions to access and modify another process's memory.

## How It Works

- The trainer locates the running game process and calculates specific memory addresses using the process's base address.
- When a cheat is toggled, the trainer writes custom byte sequences (patches) to the game's memory to enable or disable the desired effect.
- The application uses Windows API functions via P/Invoke for process and memory manipulation.

## Disclaimer

- This tool is for educational purposes only.
- Use at your own risk. Modifying game memory may violate the game's terms of service and could result in bans or other consequences.
- The author is not responsible for any damage or issues caused by using this trainer.

## Requirements

- Windows 10/11 (64-bit)
- .NET Framework 4.7.2
- Visual Studio 2022 (for building from source)
- Dead Island 2 (64-bit version)

## License

This project is provided as-is for educational use. No warranty is provided.
