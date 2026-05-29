using Godot;
using LSystemTree.Core.Grammar;
using LSystemTree.Core.Turtle;
using LSystemTree.Generation;

namespace LSystemTree
{
    public partial class MainScene : Node3D
    {
        [Export] public float IterationInterval = 0.8f;

        private TurtleInterpreter _interpreter;
        private Renderer     _renderer;
        private StringRewriter    _rewriter;
        private StochasticSampler _sampler;
        private LWord             _current;
        private bool              _paused;
        private Timer             _timer;

        public override void _Ready()
        {
            _interpreter = GetNode<TurtleInterpreter>("TurtleInterpreter");
            _renderer    = GetNode<Renderer>("Renderer");

            _sampler  = new StochasticSampler();
            var axiom = StringRewriter.ParseAxiom("F");
            var rules = RuleSet.FromDefinition(BinaryTree.ClassicFractalTree());
            _rewriter = new StringRewriter(rules, _sampler);
            _current  = axiom;

            _timer = new Timer();
            _timer.WaitTime = IterationInterval;
            _timer.Timeout += OnTick;
            AddChild(_timer);
            _timer.Start();

            Render();
        }

        private void OnTick()
        {
            if (_paused) return;
            _current = _rewriter.ApplyRules(_current);
            Render();
        }

        private void Render()
        {
            var segments = _interpreter.Interpret(_current);
            _renderer.Render(segments);
        }

        public override void _Input(InputEvent e)
        {
            if (e is InputEventKey k && k.Pressed)
            {
                switch (k.Keycode)
                {
                    case Key.Space:
                        _paused = !_paused;
                        break;
                    case Key.S:
                        _sampler.Reseed(StochasticSampler.GenerateTimeSeed());
                        _current = StringRewriter.ParseAxiom("F");
                        Render();
                        break;
                }
            }
        }
    }
}