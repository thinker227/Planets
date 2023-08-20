namespace Planets;

[GlobalClass]
[Tool]
public partial class SystemSimulation : Resource
{
    [Export(PropertyHint.Range, "0,2147483647")]
    public int Steps { get; set; } = 1000;

    [Export(PropertyHint.Range, "0.001,10,or_greater")]
    public float TimeDelta { get; set; } = 1f / Engine.PhysicsTicksPerSecond;

    [Export]
    public Color OrbitColor { get; set; } = new Color(1, 1, 1, 1);

    [Export(PropertyHint.Range, "0,1000,or_greater")]
    public float OrbitWidth { get; set; } = 3;
}
