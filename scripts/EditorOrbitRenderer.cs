namespace Planets;

[Tool]
public partial class EditorOrbitRenderer : Node2D
{
    private SolarSystem system = null!;

    [Export(PropertyHint.Range, "0,2147483647")]
    public int Steps { get; set; } = 1000;

    [Export(PropertyHint.Range, "0.001,10,or_greater")]
    public float TimeDelta { get; set; } = 1f / Engine.PhysicsTicksPerSecond;

    [Export]
    public Color OrbitColor { get; set; } = new Color(1, 1, 1, 1);

    [Export(PropertyHint.Range, "0,1000,or_greater")]
    public float OrbitWidth { get; set; } = 3;

    public override void _Ready()
    {
        if (!Engine.IsEditorHint())
        {
            QueueFree();
            return;
        }

        var system = NodeUtility.GetAncestor<SolarSystem>(this);

        if (system is null)
        {
            GD.PushWarning($"Simulation has no ancestor {nameof(SolarSystem)} node.");
            return;
        }

        this.system = system;
    }

    public override void _Process(double delta)
    {
        if (!Engine.IsEditorHint()) return;

        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!Engine.IsEditorHint()) return;

        var planets = system.Planets;

        var simulatedPlanets = (Span<VirtualPlanet>)stackalloc VirtualPlanet[planets.Count];
        var physPlanets = new PhysPlanet[planets.Count];
        var simPoints = new Vector2[planets.Count][];

        // Initialize simulated planets.
        for (var i = 0; i < planets.Count; i++)
        {
            var planet = planets[i];
            simulatedPlanets[i] = new(planet);
            physPlanets[i] = new(planet.GlobalPosition, planet.Mass);
            simPoints[i] = new Vector2[Steps];
        }

        for (var step = 0; step < Steps; step++)
        {
            // Update motions.
            for (var i = 0; i < planets.Count; i++)
            {
                ref var planet = ref simulatedPlanets[i];

                planet.Motion += Planet.CalculateMotion(
                    new(planet.Position, planet.Mass),
                    physPlanets,
                    system.Gravity,
                    planet.Falloff,
                    TimeDelta);
            // }

            // // Update positions.
            // for (var i = 0; i < planets.Count; i++)
            // {
                // ref var planet = ref simulatedPlanets[i];

                var position = planet.Position + planet.Motion * TimeDelta;

                planet.Position = position;
                physPlanets[i] = physPlanets[i] with
                {
                    Position = position
                };

                simPoints[i][step] = position;
            }
        }

        // Draw lines
        for (var step = 1; step < Steps; step++)
        {
            for (var i = 0; i < planets.Count; i++)
            {
                if (!planets[i].Visible) continue;

                var from = simPoints[i][step - 1];
                var to = simPoints[i][step];

                var alpha = (Steps - step) / (float)Steps;
                var color = OrbitColor with
                {
                    A = OrbitColor.A * alpha
                };

                DrawLine(
                    from,
                    to,
                    color,
                    OrbitWidth);
            }
        }
    }

    private struct VirtualPlanet
    {
        public float Mass { get; }

        public GravityFalloff Falloff { get; }

        public Vector2 Position { get; set; }

        public Vector2 Motion { get; set; }

        public VirtualPlanet(Planet other)
        {
            Mass = other.Mass;
            Falloff = other.Falloff;
            Position = other.GlobalPosition;
            Motion = other.Motion;
        }
    }
}
