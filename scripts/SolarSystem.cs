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

        ChildEnteredTree += AddPlanet;
        ChildExitingTree += RemovePlanet;

        foreach (var child in GetChildren())
        {
            if (child is Planet planet) planets.Add(planet);
        }

        physPlanets = new PhysPlanet[planets.Count];
    }

    private void AddPlanet(Node node)
    {
        if (node is not Planet planet) return;

        if (planets.Contains(planet)) return;

        planets.Add(planet);
        physPlanets = new PhysPlanet[planets.Count];
    }

    private void RemovePlanet(Node node)
    {
        if (node is not Planet planet) return;

        planets.Remove(planet);
        physPlanets = new PhysPlanet[planets.Count];
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Engine.IsEditorHint()) return;

        // Update phys planets.
        for (var i = 0; i < Planets.Count; i++)
        {
            var planet = Planets[i];

            var mass = planet.Enabled
                ? planet.Mass
                // If planet isn't active then it shouldn't affect other planets.
                : 0;

            physPlanets[i] = new(planet.GlobalPosition, mass);
        }

        // Update motions.
        for (var i = 0; i < Planets.Count; i++)
        {
            var planet = Planets[i];

            if (!planet.Enabled) continue;

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

            if (!planet.Enabled) continue;

            planet.Position += planet.Motion * (float)delta;
        }
    }
}
