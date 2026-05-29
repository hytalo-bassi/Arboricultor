using Godot;
using System;
using System.Collections.Generic;

public static class BinaryTree
{
	public static LSystemPreset ClassicFractalTree()
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
}
