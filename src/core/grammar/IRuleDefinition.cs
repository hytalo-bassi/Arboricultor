using System;
using System.Collections.Generic;

#nullable enable

namespace LSystemTree.Core.Grammar
{
    /// <summary>
    /// Contract for defining an L-system rule preset: axiom + production rules.
    /// Pure C# interface with zero Godot dependencies for unit testing and portability.
    /// </summary>
    public interface IRuleDefinition
    {
        /// <summary>
        /// Gets the axiom (initial symbol sequence) as a string.
        /// Example: "F(1.0)" or "A+B"
        /// </summary>
        string Axiom { get; }

        /// <summary>
        /// Gets static production rules: letter → fixed successor string.
        /// Example: 'F' → "F[+F]F[-F]F"
        /// </summary>
        IReadOnlyDictionary<char, string> StaticRules { get; }

        /// <summary>
        /// Gets parametric production rules: letter → function that computes successor from parameters.
        /// The function receives float[] parameters and returns a successor string (e.g., "F(x*0.9)").
        /// Null if no parametric rules are defined.
        /// </summary>
        IReadOnlyDictionary<char, Func<float[], string>>? ParametricRules { get; }

        /// <summary>
        /// Returns true if this definition includes parametric rules.
        /// </summary>
        bool HasParametric { get; }

        /// <summary>
        /// Human-readable description for editor display and preset identification.
        /// Example: "Oak tree - stochastic branching with phototropism"
        /// </summary>
        string Description { get; }
    }
}