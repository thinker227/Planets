namespace Planets;

[Tool]
public partial class SolarSystem : Node2D
{
    private readonly List<Planet> planets = new();
    private PhysPlanet[] physPlanets = null!;
    private Camera camera = null!;

    [Export]
	public required float Gravity { get; set; }

    public Camera Camera => camera;

    public IReadOnlyList<Planet> Planets => planets;

    public override void _Ready()
    {
        camera = GetNode<Camera>("SystemCamera");

        AddPlanetDescendantsOfNode(this);

        if (Engine.IsEditorHint()) return;

        physPlanets = new PhysPlanet[Planets.Count];
    }

    private void AddPlanetDescendantsOfNode(Node node)
    {
        if (node is Planet planet)
            planets.Add(planet);

        foreach (var child in node.GetChildren())
            AddPlanetDescendantsOfNode(child);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint()) return;

        // Update phys planets.
        for (var i = 0; i < Planets.Count; i++)
        {
            var planet = Planets[i];

            var mass = planet.IsProcessing()
                ? planet.Mass
                // If planet isn't active then it shouldn't affect other planets.
                : 0;

            physPlanets[i] = new(planet.GlobalPosition, mass);
        }

        // Update motions.
        for (var i = 0; i < Planets.Count; i++)
        {
            var planet = Planets[i];

            if (!planet.IsProcessing()) continue;

            planet.Motion += Planet.CalculateMotion(
                physPlanets[i],
                physPlanets,
                Gravity,
                planet.Falloff,
                (float)delta);
        }

        // Update positions.
        for (var i = 0; i < Planets.Count; i++)
        {
            var planet = Planets[i];

            if (!planet.IsProcessing()) continue;

            planet.Position += planet.Motion * (float)delta;
        }
    }
}
