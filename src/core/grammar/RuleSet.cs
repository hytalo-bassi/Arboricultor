using System;
using System.Collections.Generic;
using System.Linq;
using LSystemTree.Generation;

#nullable enable

namespace LSystemTree.Core.Grammar
{
    /// <summary>
    /// Runtime collection of ProductionRules grouped by predecessor letter.
    /// Handles fast lookup and stochastic weighted selection via StochasticSampler.
    /// Immutable after construction for thread-safe evaluation.
    /// </summary>
    public sealed class RuleSet
    {
        private readonly Dictionary<char, List<ProductionRule>> _rulesByLetter;
        private readonly Dictionary<char, float[]> _cumulativeWeights;

        /// <summary>
        /// Constructs a RuleSet from a sequence of ProductionRules.
        /// Rules are grouped by PredecessorLetter and cumulative weights are precomputed for O(1) stochastic selection.
        /// </summary>
        public RuleSet(IEnumerable<ProductionRule> rules)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));

            _rulesByLetter = new Dictionary<char, List<ProductionRule>>();
            _cumulativeWeights = new Dictionary<char, float[]>();

            // Group rules by predecessor letter
            foreach (var rule in rules)
            {
                if (!_rulesByLetter.TryGetValue(rule.PredecessorLetter, out var list))
                {
                    list = new List<ProductionRule>(4);
                    _rulesByLetter[rule.PredecessorLetter] = list;
                }
                list.Add(rule);
            }

            // Precompute cumulative weights for each letter group
            foreach (var kvp in _rulesByLetter)
            {
                var letter = kvp.Key;
                var ruleList = kvp.Value;
                var weights = new float[ruleList.Count];
                
                float sum = 0f;
                for (int i = 0; i < ruleList.Count; i++)
                {
                    sum += ruleList[i].Weight;
                    weights[i] = sum;
                }
                
                // Normalize cumulative weights to [0,1] for sampler
                if (sum > 0f)
                {
                    for (int i = 0; i < weights.Length; i++)
                        weights[i] /= sum;
                }
                
                _cumulativeWeights[letter] = weights;
            }
        }

        /// <summary>
        /// Factory method: builds a RuleSet from an IRuleDefinition preset.
        /// Parses static/parametric rules into ProductionRule instances.
        /// </summary>
        public static RuleSet FromDefinition(IRuleDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            
            var rules = new List<ProductionRule>();
            
            // Parse static rules
            foreach (var kvp in definition.StaticRules)
            {
                var successor = StringRewriter.ParseAxiom(kvp.Value);
                rules.Add(new ProductionRule(kvp.Key, successor.AsSpan().ToArray()));
            }
            
            // Parse parametric rules
            if (definition.HasParametric && definition.ParametricRules != null)
            {
                foreach (var kvp in definition.ParametricRules)
                {
                    var func = kvp.Value;
                    rules.Add(new ProductionRule(
                        kvp.Key,
                        parameters => StringRewriter.ParseAxiom(func(parameters)).AsSpan().ToArray()
                    ));
                }
            }
            
            return new RuleSet(rules);
        }

        /// <summary>
        /// Gets the successor sequence for a symbol using stochastic weighted selection.
        /// If no matching rule is found, returns the symbol unchanged (identity).
        /// </summary>
        public LSymbol[] GetSuccessor(LSymbol symbol, StochasticSampler rng)
        {
            if (!_rulesByLetter.TryGetValue(symbol.Letter, out var candidates))
                return new[] { symbol }; // Identity fallback

            // Filter candidates by parameter condition
            var matching = new List<ProductionRule>(candidates.Count);
            foreach (var rule in candidates)
            {
                if (rule.Matches(symbol))
                    matching.Add(rule);
            }

            if (matching.Count == 0)
                return new[] { symbol };

            if (matching.Count == 1)
                return matching[0].GetSuccessor(symbol);

            // Stochastic selection via cumulative weights
            float roll = rng.NextFloat(); // [0.0, 1.0)
            var weights = _cumulativeWeights[symbol.Letter];
            
            // Map matching rules to their original weight indices
            for (int i = 0; i < candidates.Count; i++)
            {
                if (matching.Contains(candidates[i]))
                {
                    if (roll < weights[i])
                        return candidates[i].GetSuccessor(symbol);
                }
            }
            
            // Fallback to last matching rule (shouldn't happen with proper normalization)
            return matching[^1].GetSuccessor(symbol);
        }

        /// <summary>
        /// Gets all rules for a given predecessor letter (for debugging/editor inspection).
        /// </summary>
        public IReadOnlyList<ProductionRule> GetRulesForLetter(char letter)
        {
            return _rulesByLetter.TryGetValue(letter, out var list) 
                ? list.AsReadOnly() 
                : Array.Empty<ProductionRule>();
        }
    }
}