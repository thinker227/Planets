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
    private List<Vector2> drawPoints = null!;
    private int pointCount = 100;
    private float drawTimer = 0f;
    private const float Frequency = 0.1f;


    [Export]
    public Vector2 Motion { get; set; }

    [Export]
    public float Mass { get; set; }

    [Export]
    public GravityFalloff Falloff { get; set; } = GravityFalloff.InverseSquare;

    [Export]
    public required string Title { get; set; }

    [Export]
    public int PointCount
    {
        get => pointCount;
        set
        {
            if (drawPoints?.Count > value)
                drawPoints.RemoveRange(0, drawPoints.Count - value);

            pointCount = value;
        }
    }

    public override void _Ready()
    {
        system = NodeUtility.GetAncestor<SolarSystem>(this);
        label = GetNode<Label>("Label");
        drawPoints = new(pointCount);

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

        drawTimer += (float)delta;
        if (drawTimer >= Frequency)
        {
            drawTimer = 0f;

            if (drawPoints.Count > pointCount) drawPoints.RemoveAt(0);
            drawPoints.Add(GlobalPosition);
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
        if (!Engine.IsEditorHint())
        {
            var count = drawPoints.Count;
            for (var i = 1; i < count; i++)
            {
                var a = drawPoints[i - 1];
                var b = drawPoints[i];

                var alpha = i / (float)count;

                DrawLine(
                    a - GlobalPosition,
                    b - GlobalPosition,
                    new(1, 1, 1, alpha),
                    5);
            }
        }

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
