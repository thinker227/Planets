namespace Planets;

public static class NodeUtility
{
    /// <summary>
    /// Gets the first ancestor of type <typeparamref name="T"/>.
    /// </summary>
    public static T? GetAncestor<T>(Node node) where T : Node
    {
        var current = node.GetParent();

        while (current is not null)
        {
            if (current is T t) return t;

            current = current.GetParent();
        }

        throw new InvalidOperationException(
            $"Node {node.Name} does not have an ancestor of type {typeof(T).FullName}");
    }
}
