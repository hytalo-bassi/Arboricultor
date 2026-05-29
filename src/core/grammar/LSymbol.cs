using System;
using System.Text;

#nullable enable

namespace LSystemTree.Core.Grammar
{
	/// <summary>
	/// Represents a single atom in an L-system alphabet: a letter identifier 
	/// and an optional sequence of floating-point parameters.
	/// Fully immutable, value-type semantics, and safe for high-frequency string rewriting.
	/// </summary>
	public readonly struct LSymbol : IEquatable<LSymbol>
	{
		public char Letter { get; }
		public float[] Parameters { get; }
		public bool HasParameters => Parameters.Length > 0;

		/// <summary>
		/// Creates a new L-system symbol. Parameters are defensively copied to ensure immutability.
		/// </summary>
		public LSymbol(char letter, params float[] parameters)
		{
			Letter = letter;
			Parameters = parameters != null && parameters.Length > 0
				? (float[])parameters.Clone()
				: Array.Empty<float>();
		}

		/// <summary>
		/// Exact value equality. Preferred for deterministic L-system expansion.
		/// </summary>
		public bool Equals(LSymbol other)
		{
			if (Letter != other.Letter) return false;
			if (Parameters.Length != other.Parameters.Length) return false;

			for (int i = 0; i < Parameters.Length; i++)
			{
				if (Parameters[i] != other.Parameters[i])
					return false;
			}

			return true;
		}

		/// <summary>
		/// Tolerance-based equality for use after parametric transformations 
		/// (e.g., tropism, growth scaling) that accumulate floating-point error.
		/// </summary>
		public bool Equals(LSymbol other, float tolerance)
		{
			if (Letter != other.Letter) return false;
			if (Parameters.Length != other.Parameters.Length) return false;

			for (int i = 0; i < Parameters.Length; i++)
			{
				if (MathF.Abs(Parameters[i] - other.Parameters[i]) > tolerance)
					return false;
			}

			return true;
		}

		public override bool Equals(object? obj) => obj is LSymbol other && Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = Letter.GetHashCode();
				foreach (float p in Parameters)
					hash = (hash * 397) ^ p.GetHashCode();
				return hash;
			}
		}

		/// <summary>
		/// Canonical string representation for debugging, logging, and serialization.
		/// Format: "A" or "A(x,y,z)"
		/// </summary>
		public override string ToString()
		{
			if (Parameters.Length == 0) return Letter.ToString();

			var sb = new StringBuilder();
			sb.Append(Letter);
			sb.Append('(');
			for (int i = 0; i < Parameters.Length; i++)
			{
				// "G9" format avoids trailing zeros while preserving full float precision
				sb.Append(Parameters[i].ToString("G9"));
				if (i < Parameters.Length - 1) sb.Append(',');
			}
			sb.Append(')');
			return sb.ToString();
		}

		public static bool operator ==(LSymbol left, LSymbol right) => left.Equals(right);
		public static bool operator !=(LSymbol left, LSymbol right) => !left.Equals(right);
	}
}
