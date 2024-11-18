using AIINInterfaces;
using NUnit.Framework.Constraints;

namespace AIINLib.Test;

class ConnectedConstraint(GraphNode expected, bool twoWay) : Constraint
{
    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        if (actual is not GraphNode node)
        {
            return new ConstraintResult(this, actual, ConstraintStatus.Error);
        }

        var connected = node.ConnectedNodes.Any(x => x.node.Id == expected.Id);
        if (!twoWay)
        {
            return new ConstraintResult(this, actual, connected ? ConstraintStatus.Success : ConstraintStatus.Failure);
        }

        var connectedBack = expected.ConnectedNodes.Any(x => x.node.Id == node.Id);
        return new ConstraintResult(this, actual,
            connected && connectedBack ? ConstraintStatus.Success : ConstraintStatus.Failure);
    }
}

class Is : NUnit.Framework.Is
{
    public static ConnectedConstraint ConnectedTo(GraphNode expected, bool twoWay = true)
    {
        return new ConnectedConstraint(expected, twoWay);
    }
}

static class IsExtensions
{
    public static ConnectedConstraint ConnectedTo(this ConstraintExpression expression, GraphNode expected,
        bool twoWay = true)
    {
        return new ConnectedConstraint(expected, twoWay);
    }
}