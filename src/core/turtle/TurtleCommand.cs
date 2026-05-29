using System.Collections.Generic;

namespace LSystemTree.Core.Turtle
{
    /// <summary>
    /// Immutable command emitted by TurtleInterpreter.
    /// Pure data — no behavior, fully serializable for testing/debugging.
    /// </summary>
    public readonly struct TurtleCommand
    {
        public CommandType Type { get; }
        public float Value { get; }
        public string? Symbol { get; }
        public IReadOnlyDictionary<string, float>? Parameters { get; }

        public TurtleCommand(
            CommandType type,
            float value = 0f,
            string? symbol = null,
            Dictionary<string, float>? parameters = null)
        {
            Type = type;
            Value = value;
            Symbol = symbol;
            Parameters = parameters != null 
                ? new Dictionary<string, float>(parameters) // defensive copy
                : null;
        }

        public enum CommandType
        {
            Forward,            // F or F(distance)
            ForwardNoDraw,      // f (move without generating geometry)
            TurnLeft,           // + (rotate around Up)
            TurnRight,          // - 
            TurnArbitrary,      // A(angle) — parametric turn
            PitchUp,            // &(angle) — optional 3D extension
            PitchDown,          // ^(angle)
            RollLeft,           // \\(angle)
            RollRight,          // /(angle)
            Push,               // [ — save state
            Pop,                // ] — restore state
            Leaf,               // L — place leaf at current position
            Flower,             // @ — optional extension
            SetRadius,          // !(radius) — change branch thickness
            SetAge              // %(age) — influence seasonal effects
        }

        public override string ToString() =>
            $"TurtleCommand[{Type}{(Symbol != null ? $":'{Symbol}'" : "")}" +
            $"{(Value != 0f ? $", val={Value:F2}" : "")}]";
    }
}