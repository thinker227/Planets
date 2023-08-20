using System.Runtime.CompilerServices;

namespace Planets;

public static class PrintUtility
{
    public static void PrintExpr<T>(
        T value,
        [CallerArgumentExpression(nameof(value))] string expr = "") =>
        GD.Print($"{expr} = {value}");
}
