# Ludo

A Ludo game project built with C#/.NET and React. The solution includes shared game logic, two frontend implementations (Blazor and React), a console application, and an ASP.NET Core Web API.

> **Project status:** Local pass-and-play gameplay through the React frontend and API has been tested. Step-by-step piece animation is in progress. Online multiplayer authentication and real-time synchronization are not complete.

## Project Structure

```text
Ludo/
├── Ludo.sln
├── .gitignore
└── src/
    ├── Ludo.Core/       # Shared Ludo domain and game rules
    ├── Ludo.Web/        # Blazor frontend
    ├── Ludo.Console/    # Console application
    ├── Ludo.Api/        # ASP.NET Core Web API
    └── Ludo.React/      # React frontend (TypeScript + Vite)
```

## Technology Stack

- C# and .NET 8
- ASP.NET Core Web API
- Blazor
- React
- TypeScript
- Vite
- Tailwind CSS / DaisyUI for frontend styling

## Requirements

- **.NET SDK 8.0** — required to build and run the .NET projects
- Node.js and npm — required for the React frontend
- Git

Check your installed versions:

```bash
dotnet --list-sdks
node --version
npm --version
```

Make sure a .NET 8 SDK is installed. If the solution targets `net8.0`, use the .NET 8 SDK to build it.

## Frontend Implementations

This repository includes two frontend implementations to provide experience with both Blazor and React:

### Blazor frontend — `Ludo.Web`

The Blazor application is the .NET-based frontend. It uses Razor components and C# to build the user interface and interact with the shared game logic.

**Screenshot placeholder**

> Add a screenshot of the Blazor application here.

<!-- Replace the placeholder above with an image, for example:
![Blazor frontend screenshot](docs/images/blazor-frontend.png)
-->

### React frontend — `Ludo.React`

The React application is a separate frontend built with React, TypeScript, and Vite. It communicates with the ASP.NET Core API for game operations.

**Screenshot placeholder**

> Add a screenshot of the React Ludo board here.

<!-- Replace the placeholder above with an image, for example:
![React frontend screenshot](docs/images/react-frontend.png)
-->
<img width="1390" height="991" alt="image" src="https://github.com/user-attachments/assets/2b3ee6df-d492-4a21-b01e-31fa9344b0bf" />


The Blazor and React applications are **two alternative frontends** for the project, not one frontend embedded inside the other. Both demonstrate frontend development using different approaches.

## Run the React Frontend and API

The following steps describe the current local development setup. Run commands from the solution root unless stated otherwise.

### 1. Start the API

```powershell
dotnet run --project src/Ludo.Api
```

The configured local URLs have been:

- HTTP API: `http://localhost:5158`
- Swagger UI: `http://localhost:5158/swagger/index.html`

If your launch profile also enables HTTPS, it may expose `https://localhost:7044`. Confirm the actual URLs in the startup output or `src/Ludo.Api/Properties/launchSettings.json`.

### 2. Start the React frontend

Open a second terminal:

```powershell
cd src/Ludo.React
npm install
npm run dev
```

Open the URL printed by Vite. The usual development URL is:

`http://localhost:5173`

The frontend uses `/api` requests. If requests fail, verify that the API is running and that `vite.config.ts` proxies requests to the API's actual local address.

### 3. Play

1. Open the React frontend.
2. Select **Create Game**.
3. Roll the dice.
4. Select a highlighted legal piece when a choice is required.
5. Continue taking turns with the players in the same browser.

The frontend may restore the most recently saved game ID from browser `localStorage`.

## Run the Blazor Frontend

From the solution root:

```powershell
dotnet run --project src/Ludo.Web
```

Open the local URL printed by ASP.NET Core.

**Screenshot placeholder**

> Add a screenshot of the Blazor game screen here if you want to document this frontend separately.

## Run the Console Application

From the solution root:

```powershell
dotnet run --project src/Ludo.Console
```

Follow the instructions printed by the console application.

**Console output placeholder**

Add a screenshot or text capture of a representative console session here.

<!-- Example:
```text
Paste representative Ludo.Console output here.
```
-->
```Enter player 1 name: Alice
Alice chose Red.
Enter player 2 name: Bob
Bob chose Blue.
Enter player 3 name (press Enter to finish): 
▶ Alice's turn (Turn 1, consecutive sixes: 0)
╭─LUDO BOARD──────────────────────────────────────────────────────────────────╮
│                           ·   ·   ·                                         │
│                           ·   ·   S                                         │
│           1   2           ✦   ·   ·           ○   ○                         │
│           3   4           ·   ·   ·           ○   ○                         │
│                           ·   ·   ·                                         │
│                           ·   ·   ·                                         │
│   ·   S   ·   ·   ·   ·       ◆       ·   ·   ·   ✦   ·   ·                 │
│   ·   ·   ·   ·   ·   ·   ◆       ◆   ·   ·   ·   ·   ·   ·                 │
│   ·   ·   ✦   ·   ·   ·       ◆       ·   ·   ·   ·   S   ·                 │
│                           ·   ·   ·                                         │
│                           ·   ·   ·                                         │
│           5   6           ·   ·   ·           ○   ○                         │
│           7   8           ·   ·   ✦           ○   ○                         │
│                           S   ·   ·                                         │
│                           ·   ·   ·                                         │
╰─────────────────────────────────────────────────────────────────────────────╯
╭───────────────────────────────────────────────────────────────────────────────╮
│ Legend: ● Red  ● Green  ● Yellow  ● Blue  S Start  ✦ Safe  ○ Base  ◆ Finish   │
│ ×N Stacked pieces                                                             │
╰───────────────────────────────────────────────────────────────────────────────╯
───────────────────────────────── Alice's turn ──────────────────────────────────
Player: Alice
Press Enter to roll the dice...```

## API Overview

The API currently supports game operations such as:

- `POST /api/games` — create a game
- `GET /api/games/{gameId}` — retrieve a game state
- `POST /api/games/{gameId}/roll` — roll the dice for a player
- `POST /api/games/{gameId}/move` — move a selected piece

Request and response contracts are defined in the API project's contracts/DTOs. Run the API and open Swagger UI to inspect the current API schema.

## Build and Verify

From the solution root:

```powershell
dotnet build Ludo.sln
```

Build the React frontend:

```powershell
cd src/Ludo.React
npm run build
```

## Development Notes

- `Ludo.Core` owns the shared game rules and domain behavior.
- `Ludo.Api` exposes game operations to the React client.
- `Ludo.Web` is the Blazor frontend.
- `Ludo.React` is an alternative React frontend that communicates with the API.
- `Ludo.Console` provides a console-based way to interact with the game.
- Keep generated `.NET` `bin/` and `obj/` directories and Node `node_modules/` and `dist/` out of Git using the root `.gitignore`.
- Keep credentials and local secrets out of source control.

### Multiplayer and Security

The current React workflow is intended for local, same-browser pass-and-play testing. Player-token validation was removed during local testing. **Do not expose this API publicly or treat it as secure for online multiplayer** until player authentication/authorization and appropriate real-time synchronization are implemented and tested.

### Piece Animation

Step-by-step piece animation is in progress. The helper `src/Ludo.React/src/helpers/pieceAnimation.ts` has been started, but the animation has not yet been connected to the board component.

## Git

After reviewing the files included in the repository:

```powershell
git status
git add .
git commit -m "Describe your changes"
```

If generated files were staged before adding `.gitignore`, review `git status` carefully and remove only unwanted files from Git's index before committing.

## Roadmap

- [x] Local React + API game creation, dice rolling, and piece movement
- [x] Root `.gitignore`
- [ ] Add screenshots for the React and Blazor frontends
- [ ] Add representative console output
- [ ] Connect step-by-step piece animation to the board
- [ ] Polish board rendering and interaction
- [ ] Restore player authentication before online multiplayer
- [ ] Implement and test real-time multiplayer synchronization
