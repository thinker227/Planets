namespace Planets;

[Tool]
public partial class Camera : Camera2D
{
    private bool hold = false;
    private const float ScrollSensitivity = 0.1f;

    [Export]
    public required Planet Following { get; set; }

    [Export]
    public required SolarSystem System { get; set; }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint()) return;

        GlobalPosition = Following.GlobalPosition;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
            {
                var multiplier = 1 + ScrollSensitivity *
                    (mouseButton.ButtonIndex is MouseButton.WheelUp ? 1 : -1);
                Zoom *= multiplier;
            }

            else if (mouseButton.ButtonIndex is MouseButton.Left)
                hold = mouseButton.Pressed;

            else if (mouseButton.ButtonIndex is MouseButton.Right && mouseButton.Pressed)
            {
                var target = null as (float, Planet)?;

                foreach (var planet in System.Planets)
                {
                    var distance = GetGlobalMousePosition().DistanceTo(planet.GlobalPosition);

                    if (target is null ||
                        target is (var minDistance, _) && distance < minDistance)
                        target = (distance, planet);
                }

                if (target is not (_, var targetPlanet)) return;

                Following = targetPlanet;

                Offset = GlobalPosition + Offset - Following.GlobalPosition;
                GlobalPosition = Following.GlobalPosition;
            }
        }

        else if (@event is InputEventMouseMotion mouseMotion && hold)
            Offset -= mouseMotion.Relative / Zoom;

        else if (@event is InputEventKey {
            Pressed: true,
            Keycode: Key.R,
        })
            Offset = Vector2.Zero;
    }
}
