namespace Planets;

[Tool]
public partial class SolarSystem : Node2D
{
    private readonly List<Planet> planets = new();
    private Camera camera = null!;

    [Export]
	public required float Gravity { get; set; }

    [Export]
    public SystemSimulation? Simulation { get; set; }

    public Camera Camera => camera;

    public IReadOnlyList<Planet> Planets => planets;

    public override void _Ready()
    {
        camera = GetNode<Camera>("SystemCamera");

        AddPlanetDescendantsOfNode(this);
    }

    private void AddPlanetDescendantsOfNode(Node node)
    {
        if (node is Planet planet)
            planets.Add(planet);

        foreach (var child in node.GetChildren())
            AddPlanetDescendantsOfNode(child);
    }

    public override void _Process(double delta)
    {
        if (Simulation is null) return;

        QueueRedraw();
    }

    public override void _Draw()
    {
        if (Simulation is null) return;

        var simulatedPlanets = (Span<SimulatedPlanet>)stackalloc SimulatedPlanet[Planets.Count];
        var physPlanets = new PhysPlanet[Planets.Count];
        var simPoints = new Vector2[Planets.Count][];

        // Initialize simulated planets.
        for (var i = 0; i < planets.Count; i++)
        {
            var planet = Planets[i];
            simulatedPlanets[i] = new(planet);
            physPlanets[i] = new(planet.GlobalPosition, planet.Mass);
            simPoints[i] = new Vector2[Simulation.Steps];
        }

        for (var step = 0; step < Simulation.Steps; step++)
        {
            // Update motions.
            for (var i = 0; i < planets.Count; i++)
            {
                ref var planet = ref simulatedPlanets[i];

                planet.Motion += Planet.CalculateMotion(
                    new(planet.Position, planet.Mass),
                    physPlanets,
                    Gravity,
                    planet.Falloff,
                    Simulation.TimeDelta);
            }

            // Update positions.
            for (var i = 0; i < planets.Count; i++)
            {
                ref var planet = ref simulatedPlanets[i];

                var position = planet.Position + planet.Motion * Simulation.TimeDelta;

                planet.Position = position;
                physPlanets[i] = physPlanets[i] with
                {
                    Position = planet.Position
                };

                simPoints[i][step] = position;
            }
        }

        // Draw lines
        for (var step = 1; step < Simulation.Steps; step++)
        {
            for (var i = 0; i < planets.Count; i++)
            {
                var from = simPoints[i][step - 1];
                var to = simPoints[i][step];

                var alpha = (Simulation.Steps - step) / (float)Simulation.Steps;
                var color = Simulation.OrbitColor with
                {
                    A = Simulation.OrbitColor.A * alpha
                };

                DrawLine(
                    from,
                    to,
                    color,
                    Simulation.OrbitWidth);
            }
        }
    }

    private struct SimulatedPlanet
    {
        public float Mass { get; }

        public GravityFalloff Falloff { get; }

        public Vector2 Position { get; set; }

        public Vector2 Motion { get; set; }

        public SimulatedPlanet(Planet other)
        {
            Mass = other.Mass;
            Falloff = other.Falloff;
            Position = other.GlobalPosition;
            Motion = other.Motion;
        }
    }
}
