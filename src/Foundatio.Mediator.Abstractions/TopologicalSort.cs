namespace Foundatio.Mediator;

/// <summary>
/// Provides topological sorting with support for relative ordering constraints (OrderBefore/OrderAfter)
/// and numeric order fallback. Uses Kahn's algorithm (BFS-based) for deterministic output.
/// Used at runtime for PublishAsync handler ordering.
/// </summary>
internal static class TopologicalSort
{
    /// <summary>
    /// Sorts items respecting OrderBefore/OrderAfter constraints with numeric Order as tiebreaker.
    /// </summary>
    /// <typeparam name="T">The type of items to sort.</typeparam>
    /// <param name="items">The items to sort.</param>
    /// <param name="getKey">Function to get the unique key (handler class name) for an item.</param>
    /// <param name="getOrderBefore">Function to get the type names this item must run before.</param>
    /// <param name="getOrderAfter">Function to get the type names this item must run after.</param>
    /// <param name="getNumericOrder">Function to get the numeric order for tiebreaking.</param>
    /// <returns>The sorted list of items.</returns>
    public static List<T> Sort<T>(
        IEnumerable<T> items,
        Func<T, string> getKey,
        Func<T, IReadOnlyList<string>> getOrderBefore,
        Func<T, IReadOnlyList<string>> getOrderAfter,
        Func<T, int> getNumericOrder)
    {
        var itemList = items.ToList();

        if (itemList.Count <= 1)
            return itemList;

        // Build a lookup from key to item
        var keyToItem = new Dictionary<string, T>(StringComparer.Ordinal);
        var keyToIndex = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < itemList.Count; i++)
        {
            var key = getKey(itemList[i]);
            keyToItem[key] = itemList[i];
            keyToIndex[key] = i;
        }

        // Check if any items have relative ordering constraints
        bool hasRelativeConstraints = false;
        foreach (var item in itemList)
        {
            if (getOrderBefore(item).Count > 0 || getOrderAfter(item).Count > 0)
            {
                hasRelativeConstraints = true;
                break;
            }
        }

        // Fast path: no relative constraints, just use numeric Order
        if (!hasRelativeConstraints)
            return [.. itemList.OrderBy(getNumericOrder)];

        // Build adjacency list and in-degree map for Kahn's algorithm
        var adjacency = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var inDegree = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var item in itemList)
        {
            var key = getKey(item);
            if (!adjacency.ContainsKey(key))
                adjacency[key] = [];
            if (!inDegree.ContainsKey(key))
                inDegree[key] = 0;
        }

        // Process OrderBefore: if A says OrderBefore = [B], then A must come before B → edge A → B
        foreach (var item in itemList)
        {
            var key = getKey(item);
            foreach (var beforeTarget in getOrderBefore(item))
            {
                if (keyToIndex.ContainsKey(beforeTarget))
                {
                    adjacency[key].Add(beforeTarget);
                    inDegree[beforeTarget]++;
                }
            }
        }

        // Process OrderAfter: if A says OrderAfter = [C], then C must come before A → edge C → A
        foreach (var item in itemList)
        {
            var key = getKey(item);
            foreach (var afterTarget in getOrderAfter(item))
            {
                if (keyToIndex.ContainsKey(afterTarget))
                {
                    if (!adjacency.ContainsKey(afterTarget))
                        adjacency[afterTarget] = [];
                    adjacency[afterTarget].Add(key);
                    inDegree[key]++;
                }
            }
        }

        // Kahn's algorithm with sorted ready queue for deterministic output
        var ready = new List<string>();
        foreach (var kvp in inDegree)
        {
            if (kvp.Value == 0)
                ready.Add(kvp.Key);
        }

        ready.Sort((a, b) =>
        {
            int orderCmp = getNumericOrder(keyToItem[a]).CompareTo(getNumericOrder(keyToItem[b]));
            return orderCmp != 0 ? orderCmp : string.Compare(a, b, StringComparison.Ordinal);
        });

        var sorted = new List<T>(itemList.Count);

        while (ready.Count > 0)
        {
            var current = ready[0];
            ready.RemoveAt(0);

            sorted.Add(keyToItem[current]);

            var newlyReady = new List<string>();
            foreach (var neighbor in adjacency[current])
            {
                inDegree[neighbor]--;
                if (inDegree[neighbor] == 0)
                    newlyReady.Add(neighbor);
            }

            if (newlyReady.Count > 0)
            {
                newlyReady.Sort((a, b) =>
                {
                    int orderCmp = getNumericOrder(keyToItem[a]).CompareTo(getNumericOrder(keyToItem[b]));
                    return orderCmp != 0 ? orderCmp : string.Compare(a, b, StringComparison.Ordinal);
                });

                MergeSorted(ready, newlyReady, (a, b) =>
                {
                    int orderCmp = getNumericOrder(keyToItem[a]).CompareTo(getNumericOrder(keyToItem[b]));
                    return orderCmp != 0 ? orderCmp : string.Compare(a, b, StringComparison.Ordinal);
                });
            }
        }

        // Handle cycles: add remaining items in numeric order (fallback)
        if (sorted.Count < itemList.Count)
        {
            var remaining = new List<T>();
            foreach (var kvp in inDegree)
            {
                if (kvp.Value > 0)
                    remaining.Add(keyToItem[kvp.Key]);
            }

            remaining.Sort((a, b) =>
            {
                int orderCmp = getNumericOrder(a).CompareTo(getNumericOrder(b));
                return orderCmp != 0 ? orderCmp : string.Compare(getKey(a), getKey(b), StringComparison.Ordinal);
            });

            sorted.AddRange(remaining);
        }

        return sorted;
    }

    private static void MergeSorted(List<string> target, List<string> source, Comparison<string> comparison)
    {
        if (source.Count == 0) return;

        int insertIndex = 0;
        foreach (var item in source)
        {
            while (insertIndex < target.Count && comparison(target[insertIndex], item) <= 0)
                insertIndex++;

            target.Insert(insertIndex, item);
            insertIndex++;
        }
    }
}
