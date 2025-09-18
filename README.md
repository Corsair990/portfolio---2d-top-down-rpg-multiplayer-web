# A Godot 4 Multiplayer RPG Foundation in C#

A robust, server-authoritative foundation for a 2D top-down multiplayer RPG built in Godot 4 using C#. This project is the result of a deep dive into stable, modern networking patterns.


# About The Project #

This project was built from the ground up to solve common, complex problems in multiplayer game development. It provides a clean, stable, and optimized starting point for any developer looking to build a networked 2D RPG.

The architecture is centered around a **dedicated server** model, ensuring security and a single source of truth for the game state.

# Key Features #

* **Dedicated Server Architecture:** The server is the ultimate authority, preventing common cheats.
* **Client-Side Prediction & Server Reconciliation:** Player movement feels instantly responsive to the local player while being smoothly corrected by the server's authoritative state.
* **Networked State Machine:** A clean and extensible state machine pattern.
* **Animation Controller:** Handles all the animation logic.
* **Bandwidth Optimization:** Player input/facing is sent to the server using bitmasking, I will be minimizing network traffic using optimizations where I can.
* **Dynamic Player Spawning:** A robust system to be spawned into the world safely and reliably.
* **Movement Interpolation:** Smoothly renders remote player movement, eliminating network jitter.
* **Networked Inventory:** A properly networked and authoritative inventory system. -- TODO
* **Quest & Dialogue:** A questing and dialgue system. -- TODO

# Built With

* [Godot Engine 4.5](https://godotengine.org/)
* [C# 11](https://learn.microsoft.com/en-us/dotnet/csharp/)

# Getting Started #

To get a local copy up and running, follow these simple steps.

1.  Clone the repo:
2.  Open the project in the Godot 4.5 editor.
3.  Run the server instance from your command line or by creating a dedicated export preset.
4.  Run one or more client instances from the editor.

# License #

Distributed under the MIT License. See [LICENSE](https://github.com/Corsair990/portfolio---2d-top-down-rpg-multiplayer-web/blob/master/LICENSE.txt) for more information.
