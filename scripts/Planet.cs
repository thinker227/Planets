namespace Planets;

public enum GravityFalloff
{
    InverseSquare,
    InverseLinear,
}

public readonly record struct PhysPlanet(Vector2 Position, float Mass);

[Tool]
public partial class Planet : Node2D
{
    private SolarSystem? system = null;
    private Label label = null!;

    [Export]
    public Vector2 Motion { get; set; }

    [Export]
    public float Mass { get; set; }

    [Export]
    public GravityFalloff Falloff { get; set; } = GravityFalloff.InverseSquare;

    [Export]
    public bool Enabled { get; set; } = true;

    [Export]
    public string? Title { get; set; }

    public override void _Ready()
    {
        system = NodeUtility.GetAncestor<SolarSystem>(this);
        label = GetNode<Label>("Label");

        label.Text = Title ?? "";

        if (system is null && !Engine.IsEditorHint())
            GD.PushWarning("Planet system is null.");
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
        {
            QueueRedraw();
            return;
        }

        if (system is not null)
        {
            label.LabelSettings.FontSize = (int)(32 / system.Camera.Zoom.X);
            label.LabelSettings.OutlineSize = (int)(8 / system.Camera.Zoom.X);
        }

        QueueRedraw();
    }

    public static Vector2 CalculateMotion(
        PhysPlanet planet,
        IEnumerable<PhysPlanet> attractors,
        float gravity,
        GravityFalloff mode,
        float timeDelta)
    {
        var motion = Vector2.Zero;

        foreach (var attractor in attractors)
        {
            var force = CalculateForce(
                planet,
                attractor,
                gravity,
                mode);

            var acceleration = force / planet.Mass;
            motion += acceleration * timeDelta;
        }

        return motion;
    }

    public static Vector2 CalculateForce(
        PhysPlanet a,
        PhysPlanet b,
        float gravity,
        GravityFalloff mode)
    {
        if (a == b) return Vector2.Zero;

        var distance = mode switch
        {
            GravityFalloff.InverseSquare => a.Position.DistanceSquaredTo(b.Position),
            GravityFalloff.InverseLinear => a.Position.DistanceTo(b.Position),
            _ => throw new System.Diagnostics.UnreachableException(),
        };
        var masses = a.Mass * b.Mass;
        var force = gravity * (masses / distance);
        var direction = (b.Position - a.Position).Normalized();
        return direction * force;
    }

    public override void _Draw()
    {
        if (system is null) return;
        if (!Engine.IsEditorHint() && system.Camera.Following != this) return;

        foreach (var attractor in system.Planets)
        {
            var force = CalculateForce(
                new(GlobalPosition, Mass),
                new(attractor.GlobalPosition, attractor.Mass),
                system.Gravity,
                Falloff);

            var actual = force / Mass;
            DrawLine(Vector2.Zero, actual, new(1, 0, 0), 3);
        }

        DrawLine(Vector2.Zero, Motion, new(0, 1, 0), 3);
    }
}
