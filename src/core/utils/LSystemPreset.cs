using LSystemTree.Core.Grammar;
using System;
using System.Collections.Generic;

/// <summary>
/// Simple preset container for L-system definitions.
/// </summary>
public class LSystemPreset : IRuleDefinition
	{
		public string Name { get; }
		public string Axiom { get; }
		public IReadOnlyDictionary<char, string> StaticRules { get; } = null!;
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
