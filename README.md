## Programming 1 Course Project

This repository contains my personal project for the Programming 1 course at the University of Jyväskylä. 
This project provided hands-on experience with game development concepts and practices, while also reinforcing fundamental programming skills in C#. 
The goal of this project was to apply the programming skills learned during the course using the C# language and to create a simple game or a small program.
The graphics are created using my daughter's drawings.

Game Objectives:

    - To fly the game character (kite) collecting collectibles (stars) avoiding hitting any other objects.
    - The game ends, if the character hits any other game objects (trees, storm clouds ground), than collectibles.
    - The speed of the game increases as the game advances.
    - The game is finished, when the character reaches finishing line.

Tools and Technologies Used:

    - Programming Language: C#
    - Development Environment: Visual Studio
    - Version Control: Git Bash and GitLab (this GitHub repository is created later for preserving purposes)
    - Libraries: JyPeli

Project Structure:

    - Leija.cs: The main content of the game. Manages the game functions.
    - Ohjelma.cs: The main entry point for the application.
    - Content: Contains all graphics and sound effects used in the game.

Features:

    - Physics-based game mechanics using FarseerPhysics engine.
    - Player control of a kite (Leija) character using keyboard input.
    - Collision detection and handling between game objects.
    - Score tracking and display.
    - Dynamic camera movement following the player.
    - Randomly generated collectible objects (Tähti).
    - Obstacle avoidance (trees and storm clouds).
    - Sound effect for various game events.
    - Custom graphics and images for game objects.
    - Possibility to start a new game after game over or reaching the finish line.
    - Keeps record of high scores.

Objectives:
Through this project, the aim was to strengthen the following programming skills:

    - Object-oriented programming in C# using classes and inheritance.
    - Game development using the Jypeli game framework.
    - Implementing physics-based gameplay mechanics.
    - Handling user input and controls.
    - Managing game state and object interactions.
    - Working with 2D graphics and animations.
    - Utilizing random number generation for game elements.
    - Applying basic game design principles (scoring, obstacles, collectibles).

1. **Download the repository**:
   - You can clone this repository using Git:
     ```bash
     git clone https://github.com/mmkarkko/Leijapeli
     ```
   - Alternatively, download it as a ZIP file from [here](https://github.com/mmkarkko/Leijapeli/blob/master/Leijapeli.zip) and extract it.

2. **Open the project**:
   - Launch **Visual Studio** and open the solution file (`Leija.sln`) found in the downloaded folder.

3. **Install required libraries**:
   - Ensure you have the **JyPeli** library installed. You can add it via NuGet Package Manager in Visual Studio:
     - Right-click on the project in Solution Explorer and select **Manage NuGet Packages**.
     - Search for "JyPeli" and install the latest version.
     - Additional info and instructions about Jypeli can be found [here](https://tim.jyu.fi/view/kurssit/jypeli/wiki#gUEja7HYbZtV).

4. **Build the project**:
   - After ensuring all dependencies are met, build the project by selecting **Build > Build Solution** from the menu.

5. **Run the game**:
   - Start the game by pressing `F5` or clicking the **Start** button in Visual Studio.

## How to Play

- Use the keyboard to control the kite.
- Collect stars while avoiding trees and storm clouds.

## Notes

This project is a personal learning tool and is not intended to be shared or used by others.
