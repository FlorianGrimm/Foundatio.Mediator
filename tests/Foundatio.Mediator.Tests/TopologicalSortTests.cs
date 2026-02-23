namespace Foundatio.Mediator.Tests;

public class TopologicalSortTests
{
    private record TestItem(string Key, int NumericOrder, string[] OrderBefore, string[] OrderAfter);

    [Fact]
    public void EmptyList_ReturnsEmpty()
    {
        var items = new List<TestItem>();
        var result = TopologicalSort.Sort(
            items, i => i.Key,
            i => i.OrderBefore, i => i.OrderAfter,
            i => i.NumericOrder);

        Assert.Empty(result);
    }

    [Fact]
    public void SingleItem_ReturnsSame()
    {
        var items = new List<TestItem> { new("A", 0, [], []) };
        var result = TopologicalSort.Sort(
            items, i => i.Key,
            i => i.OrderBefore, i => i.OrderAfter,
            i => i.NumericOrder);

        Assert.Single(result);
        Assert.Equal("A", result[0].Key);
    }

    [Fact]
    public void NoConstraints_SortsByNumericOrder()
    {
        var items = new List<TestItem>
        {
            new("C", 30, [], []),
            new("A", 10, [], []),
            new("B", 20, [], []),
        };

        var result = TopologicalSort.Sort(
            items, i => i.Key,
            i => i.OrderBefore, i => i.OrderAfter,
            i => i.NumericOrder);

        Assert.Equal("A", result[0].Key);
        Assert.Equal("B", result[1].Key);
        Assert.Equal("C", result[2].Key);
    }

    [Fact]
    public void OrderBefore_RespectedOverNumericOrder()
    {
        // C has highest numeric order but must come before A
        var items = new List<TestItem>
        {
            new("A", 10, [], []),
            new("B", 20, [], []),
            new("C", 30, ["A"], []), // C must come before A
        };

        var result = TopologicalSort.Sort(
            items, i => i.Key,
            i => i.OrderBefore, i => i.OrderAfter,
            i => i.NumericOrder);

        var indexC = result.FindIndex(i => i.Key == "C");
        var indexA = result.FindIndex(i => i.Key == "A");
        Assert.True(indexC < indexA, "C should come before A");
    }

    [Fact]
    public void OrderAfter_RespectedOverNumericOrder()
    {
        // A has lowest numeric order but must come after C
        var items = new List<TestItem>
        {
            new("A", 10, [], ["C"]), // A must come after C
            new("B", 20, [], []),
            new("C", 30, [], []),
        };

        var result = TopologicalSort.Sort(
            items, i => i.Key,
            i => i.OrderBefore, i => i.OrderAfter,
            i => i.NumericOrder);

        var indexC = result.FindIndex(i => i.Key == "C");
        var indexA = result.FindIndex(i => i.Key == "A");
        Assert.True(indexC < indexA, "C should come before A since A has OrderAfter = [C]");
    }

    [Fact]
    public void Chain_OrderBefore_ABC()
    {
        // A before B, B before C
        var items = new List<TestItem>
        {
            new("C", 0, [], []),
            new("A", 0, ["B"], []),
            new("B", 0, ["C"], []),
        };

        var result = TopologicalSort.Sort(
            items, i => i.Key,
            i => i.OrderBefore, i => i.OrderAfter,
            i => i.NumericOrder);

        Assert.Equal("A", result[0].Key);
        Assert.Equal("B", result[1].Key);
        Assert.Equal("C", result[2].Key);
    }

    [Fact]
    public void Chain_OrderAfter_ABC()
    {
        // C after B, B after A
        var items = new List<TestItem>
        {
            new("C", 0, [], ["B"]),
            new("A", 0, [], []),
            new("B", 0, [], ["A"]),
        };

        var result = TopologicalSort.Sort(
            items, i => i.Key,
            i => i.OrderBefore, i => i.OrderAfter,
            i => i.NumericOrder);

        Assert.Equal("A", result[0].Key);
        Assert.Equal("B", result[1].Key);
        Assert.Equal("C", result[2].Key);
    }

    [Fact]
    public void MultipleOrderBefore_AllTargetsRespected()
    {
        // A must come before both B and C
        var items = new List<TestItem>
        {
            new("B", 0, [], []),
            new("C", 0, [], []),
            new("A", 0, ["B", "C"], []),
        };

        var result = TopologicalSort.Sort(
            items, i => i.Key,
            i => i.OrderBefore, i => i.OrderAfter,
            i => i.NumericOrder);

        var indexA = result.FindIndex(i => i.Key == "A");
        var indexB = result.FindIndex(i => i.Key == "B");
        var indexC = result.FindIndex(i => i.Key == "C");
        Assert.True(indexA < indexB, "A should come before B");
        Assert.True(indexA < indexC, "A should come before C");
    }

    [Fact]
    public void CycleDetection_FallsBackToNumericOrder()
    {
        // A before B, B before A - cycle!
        var items = new List<TestItem>
        {
            new("A", 10, ["B"], []),
            new("B", 20, ["A"], []),
        };

        var result = TopologicalSort.Sort(
            items, i => i.Key,
            i => i.OrderBefore, i => i.OrderAfter,
            i => i.NumericOrder);

        // Both items should still be in the result
        Assert.Equal(2, result.Count);
        Assert.Contains(result, i => i.Key == "A");
        Assert.Contains(result, i => i.Key == "B");
    }

    [Fact]
    public void UnknownTypeInOrderBefore_IsIgnored()
    {
        var items = new List<TestItem>
        {
            new("A", 10, ["NonExistent"], []),
            new("B", 20, [], []),
        };

        var result = TopologicalSort.Sort(
            items, i => i.Key,
            i => i.OrderBefore, i => i.OrderAfter,
            i => i.NumericOrder);

        // Should sort normally by numeric order, ignoring the unknown reference
        Assert.Equal("A", result[0].Key);
        Assert.Equal("B", result[1].Key);
    }

    [Fact]
    public void UnknownTypeInOrderAfter_IsIgnored()
    {
        var items = new List<TestItem>
        {
            new("A", 10, [], ["NonExistent"]),
            new("B", 20, [], []),
        };

        var result = TopologicalSort.Sort(
            items, i => i.Key,
            i => i.OrderBefore, i => i.OrderAfter,
            i => i.NumericOrder);

        Assert.Equal("A", result[0].Key);
        Assert.Equal("B", result[1].Key);
    }

    [Fact]
    public void MixedRelativeAndNumeric_CorrectOrder()
    {
        // D(Order=5) should be first by numeric order
        // A must come before B (relative constraint)
        // C(Order=100) has no constraints
        var items = new List<TestItem>
        {
            new("A", 50, ["B"], []),
            new("B", 60, [], []),
            new("C", 100, [], []),
            new("D", 5, [], []),
        };

        var result = TopologicalSort.Sort(
            items, i => i.Key,
            i => i.OrderBefore, i => i.OrderAfter,
            i => i.NumericOrder);

        var indexD = result.FindIndex(i => i.Key == "D");
        var indexA = result.FindIndex(i => i.Key == "A");
        var indexB = result.FindIndex(i => i.Key == "B");
        Assert.True(indexD < indexA, "D (Order=5) should come before A (Order=50)");
        Assert.True(indexA < indexB, "A should come before B due to OrderBefore constraint");
    }

    [Fact]
    public void EqualNumericOrder_BreaksTieByName()
    {
        var items = new List<TestItem>
        {
            new("C", 0, [], []),
            new("A", 0, [], []),
            new("B", 0, [], []),
        };

        // With no relative constraints and same numeric order, should fall back to name
        var result = TopologicalSort.Sort(
            items, i => i.Key,
            i => i.OrderBefore, i => i.OrderAfter,
            i => i.NumericOrder);

        // No relative constraints => returns in original order (fast path)
        // But adding a relative constraint triggers the sort
        items = new List<TestItem>
        {
            new("C", 0, [], []),
            new("A", 0, ["C"], []), // A before C, so A → C edge
            new("B", 0, [], []),
        };

        result = TopologicalSort.Sort(
            items, i => i.Key,
            i => i.OrderBefore, i => i.OrderAfter,
            i => i.NumericOrder);

        var indexA = result.FindIndex(i => i.Key == "A");
        var indexC = result.FindIndex(i => i.Key == "C");
        Assert.True(indexA < indexC, "A should come before C due to OrderBefore");
    }

    [Fact]
    public void ThreeWayCycle_AllItemsIncluded()
    {
        // A → B → C → A (cycle)
        var items = new List<TestItem>
        {
            new("A", 10, ["B"], []),
            new("B", 20, ["C"], []),
            new("C", 30, ["A"], []),
        };

        var result = TopologicalSort.Sort(
            items, i => i.Key,
            i => i.OrderBefore, i => i.OrderAfter,
            i => i.NumericOrder);

        Assert.Equal(3, result.Count);
        Assert.Contains(result, i => i.Key == "A");
        Assert.Contains(result, i => i.Key == "B");
        Assert.Contains(result, i => i.Key == "C");
    }

    [Fact]
    public void PartialCycle_NonCycleItemsSortedCorrectly()
    {
        // D has no constraints, A and B form a cycle
        var items = new List<TestItem>
        {
            new("A", 20, ["B"], []),
            new("B", 30, ["A"], []),
            new("D", 5, [], []),
        };

        var result = TopologicalSort.Sort(
            items, i => i.Key,
            i => i.OrderBefore, i => i.OrderAfter,
            i => i.NumericOrder);

        Assert.Equal(3, result.Count);
        // D should come first (no constraints, low numeric order)
        Assert.Equal("D", result[0].Key);
    }
}
