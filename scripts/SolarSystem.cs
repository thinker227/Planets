namespace Planets;

[Tool]
public partial class SolarSystem : Node2D
{
    private readonly List<Planet> planets = new();
    private Camera camera = null!;

    [Export]
	public required float Gravity { get; set; }

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
}
