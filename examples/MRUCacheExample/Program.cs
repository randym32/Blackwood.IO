using Blackwood;

namespace MRUCacheExample;

/// <summary>
/// Example program demonstrating MRU (Most Recently Used) cache using Blackwood.IO.
/// This example shows how to use MRUCache for caching items with automatic eviction
/// of least recently used items when the cache reaches its maximum capacity.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("MRU Cache Example");
        Console.WriteLine("=================");
        Console.WriteLine();

        // ====================================================================
        // Example 1: Creating and Populating the Cache
        // ====================================================================
        // MRUCache is a thread-safe cache that automatically evicts the least
        // recently used items when it reaches its maximum capacity. This is
        // useful for caching frequently accessed data while limiting memory usage.
        Console.WriteLine("1. Creating and Populating the Cache:");
        Console.WriteLine("-------------------------------------");

        // Create an MRU cache with a maximum of 5 items
        // When the cache exceeds 5 items, the least recently used items
        // will be automatically evicted
        var cache = new MRUCache<string, string>(5);

        // Add items to cache using the indexer
        // Each access updates the item's position in the MRU list
        Console.WriteLine("Adding items to cache:");
        cache["item1"] = "Value 1";
        cache["item2"] = "Value 2";
        cache["item3"] = "Value 3";
        cache["item4"] = "Value 4";
        cache["item5"] = "Value 5";

        // Note: MRUCache doesn't expose a Count property, but we know we've
        // added 5 items, which matches the cache capacity
        Console.WriteLine("Cache is at capacity (5 items)");
        Console.WriteLine();

        // ====================================================================
        // Example 2: Accessing Items (Updates MRU Position)
        // ====================================================================
        // Accessing items through the indexer updates their position in the
        // MRU list, making them less likely to be evicted. This is the key
        // behavior of an MRU cache - frequently accessed items stay in cache.
        Console.WriteLine("2. Accessing Items (Updates MRU Position):");
        Console.WriteLine("------------------------------------------");

        // Access items - this updates their position in the MRU list
        // item1 and item3 are now more recently used than item2, item4, item5
        Console.WriteLine("Accessing items:");
        Console.WriteLine($"item1: {cache["item1"]}");
        Console.WriteLine($"item3: {cache["item3"]}");
        Console.WriteLine();

        // ====================================================================
        // Example 3: Eviction of Least Recently Used Items
        // ====================================================================
        // When adding more items than the cache capacity, the least recently
        // used items are evicted. Since item1 and item3 were recently accessed,
        // they are less likely to be evicted than item2, item4, or item5.
        Console.WriteLine("3. Eviction of Least Recently Used Items:");
        Console.WriteLine("----------------------------------------");

        // Add more items - this will evict least recently used items
        // Since item1 and item3 were recently accessed, they should remain
        // item2, item4, and item5 are candidates for eviction
        Console.WriteLine("Adding more items (will evict LRU items):");
        cache["item6"] = "Value 6";
        cache["item7"] = "Value 7";

        // The cache still contains 5 items (its maximum capacity)
        // Some of the original items have been evicted
        Console.WriteLine("Cache is still at capacity (5 items)");
        Console.WriteLine();

        // ====================================================================
        // Example 4: Checking Cache Contents
        // ====================================================================
        // To check if an item exists in the cache, use the indexer and check
        // if the result is not null. The indexer returns null (default) when
        // the key is not found in the cache.
        Console.WriteLine("4. Checking Cache Contents:");
        Console.WriteLine("---------------------------");

        // Check which items are still in cache
        // The indexer returns null if the key is not found
        // Note: item1 and item3 were recently accessed, so they should still
        // be in cache. item2, item4, and item5 may have been evicted.
        Console.WriteLine("Checking cache contents:");

        // Check if items exist by accessing them and checking for null
        var item1 = cache["item1"];
        var item2 = cache["item2"];
        var item3 = cache["item3"];
        var item6 = cache["item6"];
        var item7 = cache["item7"];

        Console.WriteLine($"item1 in cache: {item1 != null} {(item1 != null ? $"({item1})" : "")}");
        Console.WriteLine($"item2 in cache: {item2 != null} {(item2 != null ? $"({item2})" : "")}");
        Console.WriteLine($"item3 in cache: {item3 != null} {(item3 != null ? $"({item3})" : "")}");
        Console.WriteLine($"item6 in cache: {item6 != null} {(item6 != null ? $"({item6})" : "")}");
        Console.WriteLine($"item7 in cache: {item7 != null} {(item7 != null ? $"({item7})" : "")}");
    }
}

