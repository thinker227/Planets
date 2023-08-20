namespace Planets;

public partial class PathRenderer : Node2D
{
    private List<Vector2>? points = null;
    private double timer = 0f;
    private int maxVerticies = 100;

    [Export(PropertyHint.Range, "0,2147483647")]
	public int MaxVerticies
    {
        get => maxVerticies;
        set
        {
            if (points is not null && value < points.Count)
                points.RemoveRange(0, points.Count - value);

            maxVerticies = value;
        }
    }

    [Export(PropertyHint.Range, "0,1")]
    public double VertexFrequency { get; set; } = 1;

    [Export]
    public Color Color { get; set; } = new(1, 1, 1, 1);

    [Export(PropertyHint.Range, "0,1000,or_greater")]
    public float Width { get; set; } = 3;

    [Export]
    public Node2D? RelativeTo { get; set; }

    public override void _Ready()
    {
        points = new(MaxVerticies);
    }

    public override void _Process(double delta)
    {
        if (points is null) return;

        timer += (float)delta;
        if (timer >= VertexFrequency)
        {
            timer = 0f;

            if (points.Count > maxVerticies) points.RemoveAt(0);

            var position = GlobalPosition;
            if (RelativeTo is not null)
                position -= RelativeTo.GlobalPosition;

            points.Add(position);
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        if (points is null) return;

        var count = points.Count;
        for (var i = 1; i < count; i++)
        {
            var a = points[i - 1];
            var b = points[i];

            var alpha = i / (float)count;
            var color = new Color(
                Color.R,
                Color.G,
                Color.B,
                Color.A * alpha);

            if (RelativeTo is not null)
            {
                a += RelativeTo.GlobalPosition;
                b += RelativeTo.GlobalPosition;
            }

            DrawLine(
                a - GlobalPosition,
                b - GlobalPosition,
                color,
                Width);
        }
    }
}
