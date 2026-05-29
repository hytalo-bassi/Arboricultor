# L-System Console Test Application

A standalone, non-production C# console application for testing and visualizing the L-system expansion engine in real-time, **without Godot**.

## Quick Start

```bash
cd /path/to/arboricultor
dotnet run --project LSystemConsoleApp.csproj
```

Or run the compiled binary directly:
```bash
dotnet bin/Release/net10.0/LSystemConsoleApp.dll
```

## Features

- **Interactive Presets**: Choose from pre-configured L-systems (Fractal Tree, Stochastic Bush, Simple Plant)
- **Custom Rules**: Define your own production rules and watch them expand
- **Real-time Iteration**: Expand one step at a time with progress tracking
- **Performance Metrics**: See expansion time and final L-word statistics
- **Reproducible Seeds**: Specify seeds for deterministic results or use random seeds
- **Symbol Inspection**: View detailed symbol information including parameters
- **No Godot Required**: Pure C# console app with zero Godot dependencies

## Example Usage

### 1. Run a Preset

```
Select preset (1-5): 1
🌱 Running: Classic Fractal Tree
Enter number of iterations (default 5): 4
Enter seed for reproducibility (press Enter for random): 
📌 Axiom: F (length: 1)
  Iteration 1:     11 symbols
  Iteration 2:     61 symbols
  Iteration 3:    311 symbols
  Iteration 4:   1561 symbols
✅ Expansion complete in 42ms
🔤 Final L-word length: 1561 symbols
📄 Output (first 100 chars): F[+F]F[-F]F[+F[+F]F[-F]F]F[+F]F[-F]F[-F[+F]F[-F]F]F[+F]F[-F]F[+F[+F]F[-F]F[+F[+F]F[-F]F]F[+F]F[-F]F[...
```

### 2. Create Custom Rules

```
Select preset (1-5): 4
🛠️  Custom L-System Builder

Enter axiom (e.g., 'F' or 'A+B'): X
Enter production rules (format: letter→successor, e.g., 'F→F+F')
Enter empty line to finish:
  Rule: X→F-[[X]+X]+F[+FX]-X
  Rule: F→FF
  Rule: (press Enter to finish)
```

### 3. Inspect Symbols

```
View full symbol details? (y/n): y

📊 Symbol Details:

Index | Letter | Parameters
------|--------|-------------------
    0 | F      | (none)
    1 | [      | (none)
    2 | +      | (none)
    3 | F      | (none)
    4 | ]      | (none)
```

## Available Presets

| Name | Description | Axiom | Complexity |
|------|-------------|-------|-----------|
| Classic Fractal Tree | Deterministic binary tree | `F` | Exponential growth |
| Stochastic Bush | Complex branching structure | `X` | Higher branching |
| Simple Plant | Fibonacci growth pattern | `A` | Linear growth |

## How It Works

1. **Parse Axiom**: Converts the initial string (e.g., "F") into internal L-symbols
2. **Build Rule Set**: Compiles production rules with stochastic weighting support
3. **Expand Iteratively**: Applies rules in parallel to all symbols at each iteration
4. **Display Results**: Shows growth metrics, final size, and optional symbol details

## Technical Details

- **Engine**: Uses the same core L-system engine as Godot version (`StringRewriter`, `RuleSet`, `LSymbol`)
- **Performance**: Zero-allocation hot path via `Span<T>` and `ArrayPool<T>`
- **Seeding**: Deterministic expansion with reproducible random seeds
- **Parameters**: Supports parametric L-systems (symbols can carry float values)

## Troubleshooting

**Issue**: `.NET 8.0 not found` → The project targets .NET 10.0. Install it or update your `LSystemConsoleApp.csproj`.

**Issue**: Large expansions take long → L-systems grow exponentially. For >6 iterations on complex rules, expect memory and time costs.

**Issue**: Custom rules not expanding → Verify format: `letter→successor` (use proper arrow or `->`)

## Building from Source

```bash
# Debug build
dotnet build LSystemConsoleApp.csproj -c Debug

# Release build (optimized)
dotnet build LSystemConsoleApp.csproj -c Release
```

## Notes

- This is a **testing tool only** — not for production use
- The console app links to `Arboricultor.csproj` and compiles its core library
- All L-systems are deterministic unless stochastic rules are explicitly used
- Terminal must support UTF-8 encoding for proper box-drawing characters

---

**Happy growing!** 🌱
