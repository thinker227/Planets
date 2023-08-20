namespace Planets;

public enum GravityFalloff
{
    InverseSquare,
    InverseLinear,
}

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
    public required string Title { get; set; }

    public override void _Ready()
    {
        system = NodeUtility.GetAncestor<SolarSystem>(this);
        label = GetNode<Label>("Label");

        label.Text = Title;

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

    public override void _PhysicsProcess(double delta)
    {
        if (system is null || Engine.IsEditorHint()) return;

        Motion += CalculateMotion(
            this,
            system.Planets,
            system.Gravity,
            Falloff,
            (float)delta);

        Position += Motion * (float)delta;
    }

    public static Vector2 CalculateMotion(
        Planet planet,
        IEnumerable<Planet> attractors,
        float gravity,
        GravityFalloff mode,
        float timeDelta)
    {
        var motion = Vector2.Zero;

        foreach (var attractor in attractors)
        {
            var force = CalculateForce(
                (planet.GlobalPosition, planet.Mass),
                (attractor.GlobalPosition, attractor.Mass),
                gravity,
                mode);

            var acceleration = force / planet.Mass;
            motion += acceleration * timeDelta;
        }

        return motion;
    }

    public static Vector2 CalculateForce(
        (Vector2 position, float mass) a,
        (Vector2 position, float mass) b,
        float gravity,
        GravityFalloff mode)
    {
        if (a == b) return Vector2.Zero;

        var distance = mode switch
        {
            GravityFalloff.InverseSquare => a.position.DistanceSquaredTo(b.position),
            GravityFalloff.InverseLinear => a.position.DistanceTo(b.position),
            _ => throw new System.Diagnostics.UnreachableException(),
        };
        var masses = a.mass * b.mass;
        var force = gravity * (masses / distance);
        var direction = (b.position - a.position).Normalized();
        return direction * force;
    }

    public override void _Draw()
    {
        if (system is null) return;
        if (!Engine.IsEditorHint() && system.Camera.Following != this) return;

        foreach (var attractor in system.Planets)
        {
            var force = CalculateForce(
                (GlobalPosition, Mass),
                (attractor.GlobalPosition, attractor.Mass),
                system.Gravity,
                Falloff);

            var actual = force / Mass;
            DrawLine(Vector2.Zero, actual, new(1, 0, 0), 3);
        }

        DrawLine(Vector2.Zero, Motion, new(0, 1, 0), 3);
    }
}
