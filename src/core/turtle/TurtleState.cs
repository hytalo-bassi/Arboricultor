using System;
using System.Collections.Generic;
using System.Numerics; // Part of .NET Standard, not Godot-specific

namespace LSystemTree.Core.Turtle
{
    /// <summary>
    /// Represents the state of a turtle in 3D turtle graphics for L-System interpretation.
    /// Pure C# — no Godot dependencies. Fully unit-testable.
    /// </summary>
    public readonly struct TurtleState : IEquatable<TurtleState>
    {
        // ─────────────────────────────────────────────────────────────
        // Core spatial properties
        // ─────────────────────────────────────────────────────────────
        
        /// <summary>3D position in world space.</summary>
        public Vector3 Position { get; }

        /// <summary>Forward direction vector (normalized).</summary>
        public Vector3 Heading { get; }

        /// <summary>Up vector for orientation context (normalized).</summary>
        public Vector3 Up { get; }

        // ─────────────────────────────────────────────────────────────
        // Branch properties for geometry & seasonal effects
        // ─────────────────────────────────────────────────────────────
        
        /// <summary>Current branch radius (used for tapering geometry).</summary>
        public float Radius { get; }

        /// <summary>Biological age of current branch segment [0.0 → 1.0].</summary>
        public float Age { get; }

        /// <summary>Depth/recursion level in the L-System derivation.</summary>
        public int Depth { get; }

        // ─────────────────────────────────────────────────────────────
        // Extensible parameter bag for stochastic/parametric rules
        // ─────────────────────────────────────────────────────────────
        
        /// <summary>Optional named parameters from LSymbol (e.g., "taper", "curvature").</summary>
        public IReadOnlyDictionary<string, float>? Parameters { get; }

        // ─────────────────────────────────────────────────────────────
        // Constructors
        // ─────────────────────────────────────────────────────────────

        public TurtleState(
            Vector3 position,
            Vector3 heading,
            Vector3 up,
            float radius = 0.1f,
            float age = 0f,
            int depth = 0,
            Dictionary<string, float>? parameters = null)
        {
            Position = position;
            Heading = Vector3.Normalize(heading);
            Up = Vector3.Normalize(up);
            Radius = radius;
            Age = Math.Clamp(age, 0f, 1f);
            Depth = depth;
            Parameters = parameters != null 
                ? new Dictionary<string, float>(parameters) // defensive copy
                : null;
        }

        /// <summary>
        /// Copy constructor — creates immutable snapshot for stack operations.
        /// </summary>
        public TurtleState(TurtleState other)
            : this(
                other.Position,
                other.Heading,
                other.Up,
                other.Radius,
                other.Age,
                other.Depth,
                other.Parameters != null ? new Dictionary<string, float>(other.Parameters) : null)
        {
        }

        // ─────────────────────────────────────────────────────────────
        // Fluent mutation methods (return new instances — immutable pattern)
        // ─────────────────────────────────────────────────────────────

        public TurtleState WithPosition(Vector3 newPosition) =>
            new TurtleState(newPosition, Heading, Up, Radius, Age, Depth, Parameters);

        public TurtleState WithHeading(Vector3 newHeading) =>
            new TurtleState(Position, newHeading, Up, Radius, Age, Depth, Parameters);

        public TurtleState WithUp(Vector3 newUp) =>
            new TurtleState(Position, Heading, newUp, Radius, Age, Depth, Parameters);

        public TurtleState WithRadius(float newRadius) =>
            new TurtleState(Position, Heading, Up, newRadius, Age, Depth, Parameters);

        public TurtleState WithAge(float newAge) =>
            new TurtleState(Position, Heading, Up, Radius, newAge, Depth, Parameters);

        public TurtleState WithDepth(int newDepth) =>
            new TurtleState(Position, Heading, Up, Radius, Age, newDepth, Parameters);

        public TurtleState WithParameter(string key, float value)
        {
            var newParams = Parameters != null 
                ? new Dictionary<string, float>(Parameters) 
                : new Dictionary<string, float>();
            newParams[key] = value;
            return new TurtleState(Position, Heading, Up, Radius, Age, Depth, newParams);
        }

        // ─────────────────────────────────────────────────────────────
        // Turtle movement primitives (return new state — functional style)
        // ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Move forward by distance along current heading.
        /// </summary>
        public TurtleState Forward(float distance) =>
            WithPosition(Position + Heading * distance);

        /// <summary>
        /// Rotate heading around up axis by angle (radians).
        /// Positive = counter-clockwise (right-hand rule).
        /// </summary>
        public TurtleState Turn(float angleRad)
        {
            // Rotate heading vector around Up axis
            var rotated = Vector3.Transform(
                Heading - Vector3.Dot(Heading, Up) * Up, // project to plane
                Quaternion.CreateFromAxisAngle(Up, angleRad)
            );
            return WithHeading(Vector3.Normalize(rotated));
        }

        /// <summary>
        /// Pitch heading up/down around lateral axis (right vector).
        /// </summary>
        public TurtleState Pitch(float angleRad)
        {
            var right = Vector3.Normalize(Vector3.Cross(Heading, Up));
            var rotated = Vector3.Transform(Heading, Quaternion.CreateFromAxisAngle(right, angleRad));
            var newUp = Vector3.Transform(Up, Quaternion.CreateFromAxisAngle(right, angleRad));
            return new TurtleState(Position, Vector3.Normalize(rotated), Vector3.Normalize(newUp), Radius, Age, Depth, Parameters);
        }

        /// <summary>
        /// Roll around heading axis.
        /// </summary>
        public TurtleState Roll(float angleRad)
        {
            var newUp = Vector3.Transform(Up, Quaternion.CreateFromAxisAngle(Heading, angleRad));
            return WithUp(Vector3.Normalize(newUp));
        }

        // ─────────────────────────────────────────────────────────────
        // Utility & equality
        // ─────────────────────────────────────────────────────────────

        public bool Equals(TurtleState other) =>
            Position == other.Position &&
            Heading == other.Heading &&
            Up == other.Up &&
            Radius == other.Radius &&
            Age == other.Age &&
            Depth == other.Depth &&
            ParametersEqual(Parameters, other.Parameters);

        public override bool Equals(object? obj) =>
            obj is TurtleState other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Position, Heading, Up, Radius, Age, Depth);

        public static bool operator ==(TurtleState left, TurtleState right) => left.Equals(right);
        public static bool operator !=(TurtleState left, TurtleState right) => !(left == right);

        private static bool ParametersEqual(
            IReadOnlyDictionary<string, float>? a,
            IReadOnlyDictionary<string, float>? b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            if (a.Count != b.Count) return false;
            foreach (var kvp in a)
                if (!b.TryGetValue(kvp.Key, out var v) || v != kvp.Value) return false;
            return true;
        }

        public override string ToString() =>
            $"TurtleState[pos={Position:F2}, heading={Heading:F2}, up={Up:F2}, r={Radius:F3}, age={Age:F2}, depth={Depth}]";
    }
}