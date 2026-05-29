using System;
using System.Buffers;
using System.Collections.Generic;
using LSystemTree.Generation;

#nullable enable

namespace LSystemTree.Core.Grammar
{
    /// <summary>
    /// Iterative expansion engine for L-systems.
    /// Applies ProductionRules to LWords using seeded stochastic selection.
    /// Zero-allocation hot path via Span<T> and ArrayPool<T>.
    /// </summary>
    public sealed class StringRewriter
    {
        private readonly RuleSet _rules;
        private readonly StochasticSampler _rng;

        public StringRewriter(RuleSet rules, StochasticSampler rng)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        /// <summary>
        /// Generates an LWord by applying rules iteratively to an axiom.
        /// </summary>
        public LWord Generate(LWord axiom, int iterations)
        {
            if (iterations < 0) throw new ArgumentOutOfRangeException(nameof(iterations), "Iterations must be non-negative");
            if (axiom == null) throw new ArgumentNullException(nameof(axiom));

            LWord current = axiom;
            for (int i = 0; i < iterations; i++)
            {
                current = ApplyRules(current);
            }
            return current;
        }

        /// <summary>
        /// Applies production rules to transform the current word.
        /// Uses pooled buffers to minimize GC pressure during expansion.
        /// </summary>
        public LWord ApplyRules(LWord input)
        {
            // Conservative capacity estimate: average 3x growth per iteration
            using var builder = new LWord.Builder(input.Count * 3);
            var symbols = input.AsSpan();

            for (int i = 0; i < symbols.Length; i++)
            {
                var symbol = symbols[i];
                var successor = _rules.GetSuccessor(symbol, _rng);
                builder.AddRange(successor.AsSpan());
            }

            return builder.ToWord();
        }

        /// <summary>
        /// Legacy-style parser: converts a string like "F(10)+G(2,3)" into an LWord.
        /// Useful for loading axioms from text presets or debugging.
        /// </summary>
        public static LWord ParseAxiom(string input)
        {
            if (string.IsNullOrEmpty(input)) return new LWord(Array.Empty<LSymbol>());

            var symbols = new List<LSymbol>(input.Length / 2);
            int i = 0;

            while (i < input.Length)
            {
                char letter = input[i++];
                
                // Check for parametric module: F(10,20)
                if (i < input.Length && input[i] == '(')
                {
                    int start = i + 1; // skip '('
                    int depth = 1;
                    
                    while (i < input.Length && depth > 0)
                    {
                        i++;
                        if (i < input.Length)
                        {
                            if (input[i] == '(') depth++;
                            else if (input[i] == ')') depth--;
                        }
                    }
                    
                    if (depth != 0)
                        throw new FormatException($"Malformed module: no closing ')' for '{letter}' at position {start - 1}");
                    
                    string paramStr = input.Substring(start, i - start);
                    var parameters = ParseParameters(paramStr);
                    symbols.Add(new LSymbol(letter, parameters));
                    i++; // skip closing ')'
                }
                else
                {
                    // Simple symbol: +, -, [, ], F, etc.
                    symbols.Add(new LSymbol(letter));
                }
            }

            return new LWord(symbols);
        }

        private static float[] ParseParameters(string paramStr)
        {
            if (string.IsNullOrWhiteSpace(paramStr)) return Array.Empty<float>();
            
            var parts = paramStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var parameters = new float[parts.Length];
            
            for (int i = 0; i < parts.Length; i++)
            {
                if (!float.TryParse(parts[i], out var value))
                    throw new FormatException($"Invalid float parameter: '{parts[i]}'");
                parameters[i] = value;
            }
            
            return parameters;
        }
    }
}