# L-System Console App - Getting Started

You now have a **standalone C# console application** for testing the L-system engine without Godot! 

## 📋 Files Created

- **`src/LSystemConsole.cs`** - Main console application with 3 presets and custom rule builder
- **`LSystemConsoleApp.csproj`** - Project file for the executable console app
- **`run-lsystem-console.sh`** - Convenient bash launcher script
- **`CONSOLE_APP_README.md`** - Detailed documentation

## 🚀 Quick Start (3 Ways)

### Option 1: Using the Bash Script (Easiest)
```bash
cd /path/to/arboricultor
./run-lsystem-console.sh
```

### Option 2: Using dotnet run
```bash
dotnet run --project LSystemConsoleApp.csproj
```

### Option 3: Run Pre-built Binary
```bash
# First build if needed:
dotnet build LSystemConsoleApp.csproj -c Release

# Then run:
dotnet bin/Release/net10.0/LSystemConsoleApp.dll
```

## 🎮 Interactive Menu

Once started, you'll see:

```
📋 Available Presets:
  1. Classic Fractal Tree
  2. Stochastic Bush
  3. Simple Plant
  4. Custom Rules
  5. Exit
```

### Preset 1: Classic Fractal Tree
- Binary tree structure with brackets for push/pop
- Deterministic (same seed = same result)
- Good for understanding basic L-system expansion
- Grows exponentially: ~300 symbols at iteration 3

### Preset 2: Stochastic Bush
- More complex plant with multiple branches
- Larger expansion rate
- Demonstrates how production rules can grow rapidly
- ~1500 symbols at iteration 4

### Preset 3: Simple Plant (Fibonacci)
- Generates Fibonacci sequence growth
- Linear growth pattern (1, 2, 3, 5, 8, 13...)
- Useful for understanding simple rule expansion
- Very fast computation

### Preset 4: Custom Rules
- Create your own L-system on the fly
- Enter axiom and production rules interactively
- Format: `letter→successor` (e.g., `F→F+F`)

## 📊 Sample Run

```
Select preset (1-5): 1
🌱 Running: Classic Fractal Tree
   Description: A deterministic binary tree using push/pop (brackets) and rotation

Enter number of iterations (default 5): 4
Enter seed for reproducibility (press Enter for random): 12345

📌 Axiom: F (length: 1)
  Iteration 1:     11 symbols
  Iteration 2:     61 symbols
  Iteration 3:    311 symbols
  Iteration 4:   1561 symbols

✅ Expansion complete in 42ms
🔤 Final L-word length: 1561 symbols
🌳 Seed used: 12345
📄 Output (first 100 chars): F[+F]F[-F]F[+F[+F]F[-F]F]F[+F]F[-F]F[-F[+F]F[-F]F]F[+F]F[-F]F[+F[+F]F[-F]F[+F[+F]F[-F]F]F[+F]F[-F]F[...

View full symbol details? (y/n): y
```

## 🔧 Understanding the Output

| Field | Meaning |
|-------|---------|
| **Axiom** | Starting symbol sequence |
| **Iteration N** | Number of symbols after each expansion |
| **Expansion time** | How long the entire generation took |
| **Final L-word length** | Total symbols in expanded sequence |
| **Seed used** | Random seed (useful for reproducibility) |
| **Output** | First 100 characters of the result |

## 💡 Use Cases

1. **Debug expansion rules**: Watch how rules transform step by step
2. **Benchmark performance**: See how fast the engine expands complex rules
3. **Experiment with custom grammars**: Define and test your own plant structures
4. **Understand L-systems**: See concrete examples of formal grammar growth
5. **Test reproducibility**: Use seeds to ensure consistent expansion

## 🛠️ Custom Rules Examples

### Dragon Curve
```
Axiom: FX
Rules:
  X→X+YF+
  Y→-FX-Y
```

### Serpinski Triangle
```
Axiom: A
Rules:
  A→B-A-B
  B→A+B+A
```

### Simple Fern
```
Axiom: A
Rules:
  A→A+B
  B→B-A
```

## ⚡ Performance Notes

- **Iteration 0-3**: Instant (<1ms)
- **Iteration 4-5**: Fast (<50ms)
- **Iteration 6-7**: Noticeable (~100-500ms)
- **Iteration 8+**: Slow (seconds, high memory usage)

L-systems grow exponentially, so be careful with high iteration counts on complex rules!

## 🔗 Integration with Godot

This console app uses the **exact same engine** as the Godot version:
- `StringRewriter` - Expansion engine
- `RuleSet` - Rule management
- `LSymbol` - Symbol representation with parameters
- `LWord` - Immutable word sequences

The console app is perfect for **testing and validation** before visualizing in Godot.

## 🐛 Troubleshooting

**Q: "Cannot find .NET runtime"**
- Ensure .NET 10.0+ is installed: `dotnet --version`

**Q: App is slow / consuming lots of memory**
- Reduce iterations (8+ iterations on complex rules explodes exponentially)
- Try simpler presets like "Simple Plant"

**Q: Custom rule format not working**
- Use proper arrow: `→` or ASCII alternative `->`
- Format: `letter→successor` (single letter only)
- Example: `F→F+F[-F]` ✓ or `ABC→XYZ` ✗

**Q: Want to see raw parameters in symbols**
- Select "View full symbol details" (y/n) after expansion
- Shows index, letter, and parameter values

## 📚 Next Steps

1. Explore the three presets to understand basic L-systems
2. Create custom rules to experiment with growth patterns
3. Review `src/LSystemConsole.cs` to add more presets
4. Look at `CONSOLE_APP_README.md` for advanced options
5. Run in Godot to visualize the expanded structures as 3D plants

## 🔄 Building from Source

```bash
# Build debug version (faster iteration)
dotnet build LSystemConsoleApp.csproj -c Debug

# Build release version (optimized)
dotnet build LSystemConsoleApp.csproj -c Release

# Run tests
dotnet test LSystemConsoleApp.csproj
```

---

**Happy growing!** 🌱

For more detailed documentation, see `CONSOLE_APP_README.md`
