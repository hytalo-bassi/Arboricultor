# Arboricultor

A procedural plant generation system using L-systems (Lindenmayer systems) to create realistic growing plants and trees. Generate diverse plant structures with customizable parameters, stochastic variations, and real-time growth animations in Godot.

---

## Quick Start

1. Clone the repository: `git clone https://github.com/hytalo-bassi/Arboricultor/`
2. Open the project in [Godot Engine](https://godotengine.org/)
3. Press Play to run the application
4. Use keyboard controls to interact:
   - `Space`: Pause/unpause animation
   - `S`: Generate new seed
   - `LEFT/RIGHT arrow`: Switch between different L-systems

## Features

- Multiple L-system implementations (D0L, stochastic)
- Real-time interactive controls
- Seed-based procedural generation
- Parametric plant modeling
- Interactive visualization in Godot

## What's that about?

L-systems are parallel rewriting systems used to model the growth processes of plants. This project implements various L-system algorithms to generate realistic plant structures.

The used algorithms are usually explained better at [docs/](docs).

## How to Use

### Running the Project

- **In Godot Editor**: Open the project and press the Play button (or `F5`)
- **Built Application**: Export the project from Godot and run the executable

### Development

To work on this project:

1. Install [Godot Engine](https://godotengine.org/) (4.0+)
2. Clone the repository
3. Open in Godot and start editing scenes and scripts

## Contributing

Contributions are welcome! Please read below:

### Coding Conventions

We want our codebase to be readable, consistent, and maintainable. Please follow these guidelines:

#### Code Style

- Prioritize readability over clever tricks.
- Write easy-to-follow logic.
- Strive for optimized but clear code.
- Use English for all variable names, class names, and methods.
- Write comments only when necessary (the code should explain itself).

#### Git Conventions

Follow Conventional Commits:
- `feat`: – new feature
- `fix`: – bug fixes
- `docs`: – documentation changes
- `style`: – formatting changes (no code logic impact)
- `refactor`: – code refactoring
- `test`: – adding or updating tests
- `chore`: – maintenance tasks

Example:
```
feat: add support for interpreting parameters in L-system strings

The L-system interpreter can now parse and evaluate parameters embedded
in the production rules (e.g., F(10), A(theta)). This enables more
flexible modeling of growth patterns and parameterized plant structures.
```

Also remember to:
- Keep commits small and focused.
- Use clear commit messages that describe what and why, not just how.

## Roadmap

**In Development**

**Planned**

- Realistic trunk and branches random radius
- Random leaf shapes
- Realistic colors for wood and leaves
- Multiple plant types and variations

**Under Consideration**

- Implementation of context-sensitive L-Systems
- Generalization of tree types (monopodial, sympodial, pleionomorphic, etc.)
- Physics-based growth simulation

**Recently Completed**

- L-system interpreter
- Real-time visualization
- Seed generation
- Keyboard controls

## Troubleshooting

- **Project won't open in Godot**

  Ensure you have Godot 4.0+ installed. If the project.godot file is missing or corrupted, try re-importing the project.

- **Plants not rendering**

  Check that all scene files are properly linked in the main scene. Verify that shader resources (if any) are correctly imported.

---

**Happy growing!** 🌱
