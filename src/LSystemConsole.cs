using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using LSystemTree.Core.Grammar;
using LSystemTree.Generation;

#nullable enable

namespace LSystemTree
{
	/// <summary>
	/// Standalone console application to test and visualize L-system expansion without Godot.
	/// Run directly: dotnet run --project Arboricultor.csproj
	/// </summary>
	class LSystemConsole
	{
		static void Main(string[] args)
		{
			Console.OutputEncoding = Encoding.UTF8;
			Console.WriteLine("╔════════════════════════════════════════════════╗");
			Console.WriteLine("║   L-System Engine - Console Test              ║");
			Console.WriteLine("║   (Non-production testing environment)         ║");
			Console.WriteLine("╚════════════════════════════════════════════════╝\n");

			while (true)
			{
				Console.WriteLine("\n📋 Available Presets:");
				Console.WriteLine("  1. Classic Fractal Tree");
				Console.WriteLine("  2. Stochastic Bush");
				Console.WriteLine("  3. Simple Plant");
				Console.WriteLine("  4. Custom Rules");
				Console.WriteLine("  5. Exit");
				Console.Write("\nSelect preset (1-5): ");

				string? choice = Console.ReadLine();

				switch (choice)
				{
					case "1":
						RunPreset(ClassicFractalTree());
						break;
					case "2":
						RunPreset(StochasticBush());
						break;
					case "3":
						RunPreset(SimplePlant());
						break;
					case "4":
						RunCustom();
						break;
					case "5":
						Console.WriteLine("\n👋 Goodbye!");
						return;
					default:
						Console.WriteLine("❌ Invalid choice. Please try again.");
						continue;
				}
			}
		}

		static void RunPreset(LSystemPreset preset)
		{
			Console.WriteLine($"\n🌱 Running: {preset.Name}");
			Console.WriteLine($"   Description: {preset.Description}");

			Console.Write("\nEnter number of iterations (default 5): ");
			if (!int.TryParse(Console.ReadLine() ?? "5", out int iterations) || iterations < 0)
				iterations = 5;

			Console.Write("Enter seed for reproducibility (press Enter for random): ");
			long seed = string.IsNullOrWhiteSpace(Console.ReadLine())
				? StochasticSampler.GenerateTimeSeed()
				: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

			var stopwatch = Stopwatch.StartNew();

			try
			{
				// Parse axiom
				var axiom = StringRewriter.ParseAxiom(preset.Axiom);
				Console.WriteLine($"\n📌 Axiom: {axiom} (length: {axiom.Count})");

				// Build rule set
				var ruleSet = RuleSet.FromDefinition(preset);

				// Create rewriter
				var sampler = new StochasticSampler(seed);
				var rewriter = new StringRewriter(ruleSet, sampler);

				// Expand iteratively with progress
				var current = axiom;
				for (int i = 0; i < iterations; i++)
				{
					current = rewriter.ApplyRules(current);
					Console.WriteLine($"  Iteration {i + 1}: {current.Count,6} symbols");
				}

				stopwatch.Stop();

				// Display results
				Console.WriteLine($"\n✅ Expansion complete in {stopwatch.ElapsedMilliseconds}ms");
				Console.WriteLine($"🔤 Final L-word length: {current.Count} symbols");
				Console.WriteLine($"🌳 Seed used: {seed}");

				// Show compact representation
				var compact = current.ToString(); // Shows only letters
				if (compact.Length > 100)
					Console.WriteLine($"📄 Output (first 100 chars): {compact.Substring(0, 100)}...");
				else
					Console.WriteLine($"📄 Output: {compact}");

				// Option to see full detail
				Console.Write("\nView full symbol details? (y/n): ");
				if (Console.ReadLine()?.ToLower() == "y")
				{
					DisplaySymbols(current);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ Error: {ex.Message}");
				if (ex.InnerException != null)
					Console.WriteLine($"   Details: {ex.InnerException.Message}");
			}
		}

		static void RunCustom()
		{
			Console.WriteLine("\n🛠️  Custom L-System Builder\n");

			Console.Write("Enter axiom (e.g., 'F' or 'A+B'): ");
			string? axiomStr = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(axiomStr))
			{
				Console.WriteLine("❌ Axiom cannot be empty.");
				return;
			}

			var rules = new Dictionary<char, string>();
			Console.WriteLine("\nEnter production rules (format: letter→successor, e.g., 'F→F+F')");
			Console.WriteLine("Enter empty line to finish:");

			while (true)
			{
				Console.Write("  Rule: ");
				string? ruleLine = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(ruleLine))
					break;

				var parts = ruleLine.Split(new[] { "→", "->" }, StringSplitOptions.None);
				if (parts.Length != 2 || parts[0].Length != 1)
				{
					Console.WriteLine("    ⚠️  Invalid format. Use: letter→successor");
					continue;
				}

				char letter = parts[0][0];
				string successor = parts[1].Trim();
				rules[letter] = successor;
				Console.WriteLine($"    ✓ Added: {letter} → {successor}");
			}

			if (rules.Count == 0)
			{
				Console.WriteLine("⚠️  No rules defined. Using identity (no expansion).");
			}

			var preset = new LSystemPreset(
				name: "Custom",
				description: "User-defined L-system",
				axiom: axiomStr,
				staticRules: rules
			);

			RunPreset(preset);
		}

		static void DisplaySymbols(LWord word)
		{
			Console.WriteLine("\n📊 Symbol Details:\n");
			Console.WriteLine("Index | Letter | Parameters");
			Console.WriteLine("------|--------|-------------------");

			var symbols = word.AsSpan();
			for (int i = 0; i < Math.Min(symbols.Length, 50); i++)
			{
				var sym = symbols[i];
				string paramStr = sym.HasParameters
					? string.Join(", ", sym.Parameters)
					: "(none)";
				Console.WriteLine($" {i,4} | {sym.Letter,6} | {paramStr}");
			}

			if (symbols.Length > 50)
				Console.WriteLine($"\n... and {symbols.Length - 50} more symbols");
		}

		static LSystemPreset ClassicFractalTree()
		{
			var rules = new Dictionary<char, string>
			{
				{ 'F', "F[+F]F[-F]F" },
				{ '+', "+" },
				{ '-', "-" },
				{ '[', "[" },
				{ ']', "]" }
			};

			return new LSystemPreset(
				name: "Classic Fractal Tree",
				description: "A deterministic binary tree using push/pop (brackets) and rotation",
				axiom: "F",
				staticRules: rules
			);
		}

		static LSystemPreset StochasticBush()
		{
			var rules = new Dictionary<char, string>
			{
				{ 'X', "F-[[X]+X]+F[+FX]-X" },
				{ 'F', "FF" },
				{ '-', "-" },
				{ '+', "+" },
				{ '[', "[" },
				{ ']', "]" }
			};

			return new LSystemPreset(
				name: "Stochastic Bush",
				description: "A more complex plant with branching and probabilistic choices",
				axiom: "X",
				staticRules: rules
			);
		}

		static LSystemPreset SimplePlant()
		{
			var rules = new Dictionary<char, string>
			{
				{ 'A', "AB" },
				{ 'B', "A" }
			};

			return new LSystemPreset(
				name: "Simple Plant (Fibonacci)",
				description: "Generates Fibonacci-like growth pattern",
				axiom: "A",
				staticRules: rules
			);
		}
	}

	/// <summary>
	/// Simple preset container for L-system definitions.
	/// </summary>
	class LSystemPreset : IRuleDefinition
	{
		public string Name { get; }
		public string Axiom { get; }
		public IReadOnlyDictionary<char, string> StaticRules { get; }
		public IReadOnlyDictionary<char, Func<float[], string>>? ParametricRules => null;
		public bool HasParametric => false;
		public string Description { get; }

		public LSystemPreset(string name, string description, string axiom, Dictionary<char, string> staticRules)
		{
			Name = name;
			Description = description;
			Axiom = axiom;
			StaticRules = staticRules;
		}
	}
}
