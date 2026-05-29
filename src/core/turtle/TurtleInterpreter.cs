using Godot;
using System;
using System.Collections.Generic;
using LSystemTree.Core.Grammar;

namespace LSystemTree.Core.Turtle
{
    public readonly struct BranchSegment
    {
        public Godot.Vector3 From     { get; }
        public Godot.Vector3 To       { get; }
        public float         RadiusBottom { get; }
        public float         RadiusTop    { get; }
        public int           Depth    { get; }

        public BranchSegment(Godot.Vector3 from, Godot.Vector3 to,
                             float radiusBottom, float radiusTop, int depth)
        {
            From         = from;
            To           = to;
            RadiusBottom = radiusBottom;
            RadiusTop    = radiusTop;
            Depth        = depth;
        }
    }

    public partial class TurtleInterpreter : Node
    {
        [Export] public float StepLength = 1.0f;
        [Export] public float Angle = 25.0f; // degrees
        [Export] public float InitialRadius = 0.2f;
        [Export] public float RadiusTaper = 0.7f;

        public List<BranchSegment> Interpret(LWord word)
        {
            var segments = new List<BranchSegment>(word.Count);
            var commands = new List<TurtleCommand>(word.Count);
            var stack = new Stack<TurtleState>();

            var state = new TurtleState(
                position: System.Numerics.Vector3.Zero,
                heading:  System.Numerics.Vector3.UnitY,  // grow upward
                up:       System.Numerics.Vector3.UnitZ,
                radius:   InitialRadius
            );

            float angleRad = Angle * (MathF.PI / 180f);
            var symbols = word.AsSpan();

            for (int i = 0; i < symbols.Length; i++)
            {
                var sym = symbols[i];
                float param0 = sym.HasParameters ? sym.Parameters[0] : 1f;

                switch (sym.Letter)
                {
                    case 'F':
                        float dist = StepLength * param0;
                        var from = ToGodotVector(state.Position);
                        var nextState = state.Forward(dist)
                                             .WithRadius(state.Radius * RadiusTaper)
                                             .WithDepth(state.Depth + 1);
                        segments.Add(new BranchSegment(
                            from:         from,
                            to:           ToGodotVector(nextState.Position),
                            radiusBottom: state.Radius,
                            radiusTop:    nextState.Radius,
                            depth:        state.Depth
                        ));
                        state = nextState;
                        break;
                    case 'f':
                        state = state.Forward(StepLength * param0);
                        commands.Add(new TurtleCommand(TurtleCommand.CommandType.ForwardNoDraw));
                        break;

                    case '+':
                        state = state.Turn(angleRad);
                        commands.Add(new TurtleCommand(TurtleCommand.CommandType.TurnLeft, angleRad));
                        break;

                    case '-':
                        state = state.Turn(-angleRad);
                        commands.Add(new TurtleCommand(TurtleCommand.CommandType.TurnRight, angleRad));
                        break;

                    case '&':
                        state = state.Pitch(angleRad);
                        commands.Add(new TurtleCommand(TurtleCommand.CommandType.PitchUp, angleRad));
                        break;

                    case '^':
                        state = state.Pitch(-angleRad);
                        commands.Add(new TurtleCommand(TurtleCommand.CommandType.PitchDown, angleRad));
                        break;

                    case '\\':
                        state = state.Roll(angleRad);
                        commands.Add(new TurtleCommand(TurtleCommand.CommandType.RollLeft, angleRad));
                        break;

                    case '/':
                        state = state.Roll(-angleRad);
                        commands.Add(new TurtleCommand(TurtleCommand.CommandType.RollRight, angleRad));
                        break;

                    case '[':
                        stack.Push(state);
                        commands.Add(new TurtleCommand(TurtleCommand.CommandType.Push));
                        break;

                    case ']':
                        if (stack.Count > 0)
                            state = stack.Pop();
                        commands.Add(new TurtleCommand(TurtleCommand.CommandType.Pop));
                        break;

                    case 'L':
                        commands.Add(new TurtleCommand(
                            TurtleCommand.CommandType.Leaf,
                            parameters: new Dictionary<string, float>
                            {
                                { "x", state.Position.X },
                                { "y", state.Position.Y },
                                { "z", state.Position.Z }
                            }
                        ));
                        break;

                    case '!':
                        float newRadius = sym.HasParameters ? sym.Parameters[0] : state.Radius * RadiusTaper;
                        state = state.WithRadius(newRadius);
                        commands.Add(new TurtleCommand(TurtleCommand.CommandType.SetRadius, newRadius));
                        break;

                    // Unknown symbols are silently skipped (identity)
                }
            }

            return segments;
        }

        private static Godot.Vector3 ToGodotVector(System.Numerics.Vector3 v)
            => new Godot.Vector3(v.X, v.Y, v.Z);
    }
}