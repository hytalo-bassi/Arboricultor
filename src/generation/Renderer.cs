using Godot;
using System.Collections.Generic;
using LSystemTree.Core.Turtle;

namespace LSystemTree
{
    public partial class Renderer : Node3D
    {
        [Export] public Material BranchMaterial;
        private MeshInstance3D _meshInstance;

        public override void _Ready()
        {
            var mat = new StandardMaterial3D();
            mat.AlbedoColor = new Color(0.4f, 0.2f, 0.1f);
            BranchMaterial = mat;
            _meshInstance = new MeshInstance3D();
            AddChild(_meshInstance);
        }

        public void Render(List<BranchSegment> segments)
        {
            if (_meshInstance.Mesh is ImmediateMesh im)
                im.ClearSurfaces();

            var mesh = new ImmediateMesh();
            mesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

            foreach (var seg in segments)
            {
                mesh.SurfaceAddVertex(seg.From);
                mesh.SurfaceAddVertex(seg.To);
            }

            mesh.SurfaceEnd();
            _meshInstance.Mesh = mesh;

            var mat = new StandardMaterial3D();
            mat.AlbedoColor = Colors.Green;
            mat.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
            _meshInstance.MaterialOverride = mat;
        }
        // public void Render(List<TurtleCommand> commands)
        // {
        //     var st = new SurfaceTool();
        //     st.Begin(Mesh.PrimitiveType.Triangles);

        //     foreach (var cmd in commands)
        //     {
        //         if (cmd.Type != TurtleCommand.CommandType.Forward) continue;
        //         if (cmd.Parameters == null) continue;

        //         var from = new Godot.Vector3(
        //             cmd.Parameters["fromX"],
        //             cmd.Parameters["fromY"],
        //             cmd.Parameters["fromZ"]
        //         );
        //         float radius = cmd.Parameters["radius"];
        //         float length = cmd.Value;

        //         // Reconstruct 'to' from heading embedded in TurtleInterpreter
        //         // We need direction — derive it from the Forward distance and stored state
        //         // For now we pass it directly by also storing toX/Y/Z
        //         // See note below
        //     }

        //     st.GenerateNormals();
        //     _meshInstance.Mesh = st.Commit();
        //     if (BranchMaterial != null)
        //         _meshInstance.MaterialOverride = BranchMaterial;
        // }

        private void AddCylinder(SurfaceTool st, Godot.Vector3 from, Godot.Vector3 to,
                                  float radiusBottom, float radiusTop, int segments = 6)
        {
            if ((to - from).Length() < 0.001f) return;

            var dir = (to - from).Normalized();
            var basis = Basis.LookingAt(dir, Vector3.Up);

            for (int i = 0; i < segments; i++)
            {
                float a0 = Mathf.Tau * i / segments;
                float a1 = Mathf.Tau * (i + 1) / segments;

                var b0 = basis * new Godot.Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0));
                var b1 = basis * new Godot.Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1));

                var p0 = from + b0 * radiusBottom;
                var p1 = from + b1 * radiusBottom;
                var p2 = to   + b0 * radiusTop;
                var p3 = to   + b1 * radiusTop;

                // Triangle 1
                st.AddVertex(p0);
                st.AddVertex(p1);
                st.AddVertex(p2);

                // Triangle 2
                st.AddVertex(p1);
                st.AddVertex(p3);
                st.AddVertex(p2);
            }
        }
    }
}