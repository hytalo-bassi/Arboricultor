using System;

#nullable enable

namespace LSystemTree.Core.Grammar
{
    /// <summary>
    /// Represents a single L-system production rule: Predecessor → Successor.
    /// Supports parametric conditions, stochastic weighting, and functional successors.
    /// Immutable and safe for concurrent evaluation.
    /// </summary>
    public sealed class ProductionRule
    {
        public char PredecessorLetter { get; }
        public Predicate<float[]>? ParameterCondition { get; }
        public Func<float[], LSymbol[]>? ParametricSuccessor { get; }
        public LSymbol[]? StaticSuccessor { get; }
        public float Weight { get; }
        public bool IsParametric => ParametricSuccessor != null;

        /// <summary>
        /// Creates a static rule: letter + optional condition → fixed successor sequence.
        /// </summary>
        public ProductionRule(char predecessorLetter, LSymbol[] staticSuccessor,
                              Predicate<float[]>? parameterCondition = null,
                              float weight = 1.0f)
        {
            if (staticSuccessor == null || staticSuccessor.Length == 0)
                throw new ArgumentException("Static successor must contain at least one symbol.", nameof(staticSuccessor));

            PredecessorLetter = predecessorLetter;
            ParameterCondition = parameterCondition;
            ParametricSuccessor = null;
            StaticSuccessor = (LSymbol[])staticSuccessor.Clone();
            Weight = Math.Max(0.0f, weight);
        }

        /// <summary>
        /// Creates a parametric rule: letter + condition → function that computes successor from parameters.
        /// </summary>
        public ProductionRule(char predecessorLetter, Func<float[], LSymbol[]> parametricSuccessor,
                              Predicate<float[]>? parameterCondition = null,
                              float weight = 1.0f)
        {
            PredecessorLetter = predecessorLetter;
            ParameterCondition = parameterCondition;
            ParametricSuccessor = parametricSuccessor ?? throw new ArgumentNullException(nameof(parametricSuccessor));
            StaticSuccessor = null;
            Weight = Math.Max(0.0f, weight);
        }

        /// <summary>
        /// Checks if this rule matches the given symbol.
        /// Matches if letter is identical AND parameter condition (if provided) returns true.
        /// </summary>
        public bool Matches(LSymbol symbol)
        {
            if (PredecessorLetter != symbol.Letter) return false;
            return ParameterCondition == null || ParameterCondition(symbol.Parameters);
        }

        /// <summary>
        /// Gets the successor sequence for a symbol. 
        /// For parametric rules, invokes the function with the symbol's parameters.
        /// </summary>
        public LSymbol[] GetSuccessor(LSymbol symbol)
        {
            if (!Matches(symbol)) 
                return new[] { symbol }; // Identity fallback

            if (IsParametric && ParametricSuccessor != null)
                return ParametricSuccessor(symbol.Parameters);
            
            return StaticSuccessor ?? new[] { symbol };
        }
    }
}