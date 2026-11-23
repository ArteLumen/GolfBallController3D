# GolfBallController3D
# Unity 3D Golf

A lightweight 3D golf prototype built with C# and Unity featuring slingshot aiming, camera orbit and follow, water hazard respawns, and hole capture detection.

## Features
- **Slingshot Aiming & Shot Preview:** Drag back from the ball to aim and preview shot direction and power with a `LineRenderer`.
- **Camera Follow & Orbit:** Smoothly tracks the ball while supporting horizontal camera rotation via right-mouse drag.
- **Water Hazard Respawn:** Returns the ball to its last safe position with a `+1` stroke penalty when entering `Water` trigger zones.
- **Hole Capture & Reset:** Detects when the ball enters the hole within speed limits and supports instant hole resets with `R`.

## Controls
- **Left Click + Drag:** Aim and shoot the ball.
- **Right Click + Drag:** Orbit the camera around the ball.
- **R:** Reset the current hole.

## Project Structure
```text
unity-3d-golf/
├── .gitignore
├── README.md
├── LICENSE
└── Assets/
    └── Scripts/
        ├── GolfBallController3D.cs
        ├── CameraFollow3D.cs
        └── GameManager3D.cs
```

## Unity Setup
1. **Ground Layer:** Create a `Ground` layer, assign it to playable terrain or floor objects, and select it in **Ground Layer** on `GolfBallController3D`.
2. **Golf Ball:** Add a `Sphere` to the scene and attach a `Rigidbody`, `LineRenderer`, and `GolfBallController3D`.
3. **Hole Center:** Place an empty `GameObject` (`Transform`) at the center of the hole and assign it to **Hole Center**.
4. **Water Hazards & UI (Optional):** Add trigger colliders tagged `Water` for water hazards and assign UI `Text` references to `GameManager3D`.

## License
This project is licensed under the [APACHE 2.0 License](LICENSE).
