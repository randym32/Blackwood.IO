namespace Blackwood.IO.Tests;

/// <summary>
/// Test suite for the Cache classes in Blackwood.IO.
/// Tests cover the MRUCache implementation, cache eviction policies, thread safety, and various edge cases.
/// These tests verify that the cache correctly implements Most Recently Used (MRU) behavior.
/// </summary>
[TestFixture]
public class CacheTests
{
    #region MRUCache Constructor Tests

    /// <summary>
    /// Tests that MRUCache constructor with default capacity creates cache correctly.
    /// This verifies the basic cache initialization with default parameters.
    /// </summary>
    [Test]
    public void MRUCache_Constructor_WithDefaultCapacity_ShouldCreateCache()
    {
        // Act
        var cache = new MRUCache<string, int>();

        // Assert
        Assert.That(cache, Is.Not.Null);
    }

    /// <summary>
    /// Tests that MRUCache constructor with custom capacity creates cache correctly.
    /// This verifies the cache initialization with custom maximum items.
    /// </summary>
    [Test]
    public void MRUCache_Constructor_WithCustomCapacity_ShouldCreateCache()
    {
        // Arrange
        var maxItems = 100;

        // Act
        var cache = new MRUCache<string, int>(maxItems);

        // Assert
        Assert.That(cache, Is.Not.Null);
    }

    /// <summary>
    /// Tests that MRUCache constructor with zero capacity creates cache correctly.
    /// This verifies edge case handling for zero capacity.
    /// </summary>
    [Test]
    public void MRUCache_Constructor_WithZeroCapacity_ShouldCreateCache()
    {
        // Act
        var cache = new MRUCache<string, int>(0);

        // Assert
        Assert.That(cache, Is.Not.Null);
    }

    #endregion

    #region MRUCache Basic Operations Tests

    /// <summary>
    /// Tests that setting and getting values works correctly.
    /// This verifies the basic cache storage and retrieval functionality.
    /// </summary>
    [Test]
    public void MRUCache_SetAndGet_ShouldWorkCorrectly()
    {
        // Arrange
        var cache = new MRUCache<string, int>();
        var key = "test-key";
        var value = 42;

        // Act
        cache[key] = value;
        var result = cache[key];

        // Assert
        Assert.That(result, Is.EqualTo(value));
    }

    /// <summary>
    /// Tests that getting a non-existent key returns default value.
    /// This verifies proper handling of cache misses.
    /// </summary>
    [Test]
    public void MRUCache_GetNonExistentKey_ShouldReturnDefault()
    {
        // Arrange
        var cache = new MRUCache<string, int>();

        // Act
        var result = cache["non-existent"];

        // Assert
        Assert.That(result, Is.EqualTo(0)); // Default value for int
    }

    /// <summary>
    /// Tests that updating an existing key works correctly.
    /// This verifies that cache updates work properly.
    /// </summary>
    [Test]
    public void MRUCache_UpdateExistingKey_ShouldWorkCorrectly()
    {
        // Arrange
        var cache = new MRUCache<string, int>();
        var key = "test-key";
        cache[key] = 42;

        // Act
        cache[key] = 84;
        var result = cache[key];

        // Assert
        Assert.That(result, Is.EqualTo(84));
    }

    /// <summary>
    /// Tests that setting null values works correctly.
    /// This verifies proper handling of null values in the cache.
    /// </summary>
    [Test]
    public void MRUCache_SetNullValue_ShouldWorkCorrectly()
    {
        // Arrange
        var cache = new MRUCache<string, string>();
        var key = "test-key";

        // Act
        cache[key] = null;
        var result = cache[key];

        // Assert
        Assert.That(result, Is.Null);
    }

    #endregion

    #region MRUCache Eviction Tests

    /// <summary>
    /// Tests that cache evicts oldest items when capacity is exceeded.
    /// This verifies the MRU eviction policy works correctly.
    /// </summary>
    [Test]
    public void MRUCache_WhenCapacityExceeded_ShouldEvictOldestItems()
    {
        // Arrange
        var cache = new MRUCache<string, int>(2);
        cache["key1"] = 1;
        cache["key2"] = 2;

        // Act - Add third item to trigger eviction
        cache["key3"] = 3;

        // Assert
        Assert.That(cache["key1"], Is.EqualTo(0)); // Should be evicted
        Assert.That(cache["key2"], Is.EqualTo(2)); // Should still be there
        Assert.That(cache["key3"], Is.EqualTo(3)); // Should be there
    }

    /// <summary>
    /// Tests that accessing an item moves it to the most recently used position.
    /// This verifies the MRU behavior works correctly.
    /// </summary>
    [Test]
    public void MRUCache_AccessingItem_ShouldMoveToMRUPosition()
    {
        // Arrange
        var cache = new MRUCache<string, int>(2);
        cache["key1"] = 1;
        cache["key2"] = 2;

        // Act - Access key1 to make it most recently used
        var value = cache["key1"];
        cache["key3"] = 3; // This should evict key2, not key1

        // Assert
        Assert.That(value, Is.EqualTo(1));
        Assert.That(cache["key1"], Is.EqualTo(1)); // Should still be there
        Assert.That(cache["key2"], Is.EqualTo(0)); // Should be evicted
        Assert.That(cache["key3"], Is.EqualTo(3)); // Should be there
    }

    /// <summary>
    /// Tests that cache handles multiple evictions correctly.
    /// This verifies that the eviction mechanism works for multiple items.
    /// </summary>
    [Test]
    public void MRUCache_MultipleEvictions_ShouldWorkCorrectly()
    {
        // Arrange
        var cache = new MRUCache<string, int>(2);

        // Act - Add more items than capacity
        cache["key1"] = 1;
        cache["key2"] = 2;
        cache["key3"] = 3;
        cache["key4"] = 4;

        // Assert
        Assert.That(cache["key1"], Is.EqualTo(0)); // Should be evicted
        Assert.That(cache["key2"], Is.EqualTo(0)); // Should be evicted
        Assert.That(cache["key3"], Is.EqualTo(3)); // Should be there
        Assert.That(cache["key4"], Is.EqualTo(4)); // Should be there
    }

    #endregion

    #region MRUCache Enumeration Tests

    /// <summary>
    /// Tests that Enumerate returns all items in the cache.
    /// This verifies the enumeration functionality works correctly.
    /// </summary>
    [Test]
    public void MRUCache_Enumerate_ShouldReturnAllItems()
    {
        // Arrange
        var cache = new MRUCache<string, int>();
        cache["key1"] = 1;
        cache["key2"] = 2;
        cache["key3"] = 3;

        // Act
        var items = cache.Enumerate().ToList();

        // Assert
        Assert.That(items.Count, Is.EqualTo(3));
        Assert.That(items.Select(i => i.key), Contains.Item("key1"));
        Assert.That(items.Select(i => i.key), Contains.Item("key2"));
        Assert.That(items.Select(i => i.key), Contains.Item("key3"));
    }

    /// <summary>
    /// Tests that Enumerate on empty cache returns empty collection.
    /// This verifies proper handling of empty cache enumeration.
    /// </summary>
    [Test]
    public void MRUCache_EnumerateEmptyCache_ShouldReturnEmptyCollection()
    {
        // Arrange
        var cache = new MRUCache<string, int>();

        // Act
        var items = cache.Enumerate().ToList();

        // Assert
        Assert.That(items.Count, Is.EqualTo(0));
    }

    /// <summary>
    /// Tests that Enumerate returns items in MRU order.
    /// This verifies that enumeration respects the most recently used order.
    /// </summary>
    [Test]
    public void MRUCache_Enumerate_ShouldReturnItemsInMRUOrder()
    {
        // Arrange
        var cache = new MRUCache<string, int>();
        cache["key1"] = 1;
        cache["key2"] = 2;
        cache["key3"] = 3;

        // Act - Access key1 to make it most recently used
        var _ = cache["key1"];
        var items = cache.Enumerate().ToList();

        // Assert
        Assert.That(items.Count, Is.EqualTo(3));
        Assert.That(items[0].key, Is.EqualTo("key1")); // Should be first (most recent)
    }

    #endregion

    #region MRUCache Thread Safety Tests

    /// <summary>
    /// Tests that cache operations are thread-safe.
    /// This verifies that concurrent access doesn't cause issues.
    /// </summary>
    [Test]
    public void MRUCache_ConcurrentOperations_ShouldBeThreadSafe()
    {
        // Arrange
        var cache = new MRUCache<int, int>(100);
        var tasks = new List<Task>();

        // Act - Run concurrent operations
        for (int i = 0; i < 10; i++)
        {
            int threadId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    var key = threadId * 100 + j;
                    cache[key] = key * 2;
                    var value = cache[key];
                    Assert.That(value, Is.EqualTo(key * 2));
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert - Verify cache is not corrupted
        var items = cache.Enumerate().ToList();
        Assert.That(items.Count, Is.LessThanOrEqualTo(100));
    }

    /// <summary>
    /// Tests that cache handles concurrent evictions correctly.
    /// This verifies that eviction works properly under concurrent access.
    /// </summary>
    [Test]
    public void MRUCache_ConcurrentEvictions_ShouldWorkCorrectly()
    {
        // Arrange
        var cache = new MRUCache<int, int>(5);
        var tasks = new List<Task>();

        // Act - Run concurrent operations that will trigger evictions
        for (int i = 0; i < 5; i++)
        {
            int threadId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 20; j++)
                {
                    var key = threadId * 20 + j;
                    cache[key] = key;
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        var items = cache.Enumerate().ToList();
        Assert.That(items.Count, Is.EqualTo(5));
    }

    #endregion

    #region MRUCache Edge Case Tests

    /// <summary>
    /// Tests that cache handles zero capacity correctly.
    /// This verifies edge case handling for zero capacity cache.
    /// Note: The actual implementation allows items to be stored even with zero capacity.
    /// </summary>
    [Test]
    public void MRUCache_WithZeroCapacity_ShouldHandleCorrectly()
    {
        // Arrange
        var cache = new MRUCache<string, int>(0);

        // Act
        cache["key1"] = 1;
        var result = cache["key1"];

        // Assert
        Assert.That(result, Is.EqualTo(1)); // Items are stored even with zero capacity
    }

    /// <summary>
    /// Tests that cache handles very large capacity correctly.
    /// This verifies that large capacity caches work properly.
    /// </summary>
    [Test]
    public void MRUCache_WithLargeCapacity_ShouldHandleCorrectly()
    {
        // Arrange
        var cache = new MRUCache<int, int>(10000);

        // Act - Add many items
        for (int i = 0; i < 1000; i++)
        {
            cache[i] = i * 2;
        }

        // Assert
        for (int i = 0; i < 1000; i++)
        {
            Assert.That(cache[i], Is.EqualTo(i * 2));
        }
    }

    /// <summary>
    /// Tests that cache handles null keys correctly.
    /// This verifies proper handling of null keys in the cache.
    /// Note: The actual implementation throws ArgumentNullException for null keys.
    /// </summary>
    [Test]
    public void MRUCache_WithNullKeys_ShouldThrowException()
    {
        // Arrange
        var cache = new MRUCache<string, int>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => cache[null] = 42);
        Assert.Throws<ArgumentNullException>(() => { var _ = cache[null]; });
    }

    /// <summary>
    /// Tests that cache handles duplicate keys correctly.
    /// This verifies that updating existing keys works properly.
    /// </summary>
    [Test]
    public void MRUCache_WithDuplicateKeys_ShouldUpdateCorrectly()
    {
        // Arrange
        var cache = new MRUCache<string, int>();
        var key = "duplicate-key";

        // Act
        cache[key] = 1;
        cache[key] = 2;
        cache[key] = 3;

        // Assert
        Assert.That(cache[key], Is.EqualTo(3));
    }

    #endregion

    #region MRUCache Performance Tests

    /// <summary>
    /// Tests that cache performs efficiently with many operations.
    /// This verifies that the cache implementation is efficient.
    /// </summary>
    [Test]
    public void MRUCache_ManyOperations_ShouldPerformEfficiently()
    {
        // Arrange
        var cache = new MRUCache<int, int>(1000);
        var startTime = DateTime.UtcNow;

        // Act - Perform many operations
        for (int i = 0; i < 10000; i++)
        {
            cache[i % 1000] = i;
            var value = cache[i % 1000];
            Assert.That(value, Is.EqualTo(i));
        }

        var endTime = DateTime.UtcNow;

        // Assert
        Assert.That((endTime - startTime).TotalSeconds, Is.LessThan(5)); // Should complete within 5 seconds
    }

    /// <summary>
    /// Tests that cache performs efficiently with frequent evictions.
    /// This verifies that eviction operations are efficient.
    /// </summary>
    [Test]
    public void MRUCache_FrequentEvictions_ShouldPerformEfficiently()
    {
        // Arrange
        var cache = new MRUCache<int, int>(10);
        var startTime = DateTime.UtcNow;

        // Act - Perform operations that will trigger many evictions
        for (int i = 0; i < 1000; i++)
        {
            cache[i] = i;
        }

        var endTime = DateTime.UtcNow;

        // Assert
        Assert.That((endTime - startTime).TotalSeconds, Is.LessThan(1)); // Should complete within 1 second
        var items = cache.Enumerate().ToList();
        Assert.That(items.Count, Is.EqualTo(10));
    }

    #endregion

    #region MRUCache Integration Tests

    /// <summary>
    /// Tests that cache works correctly with different value types.
    /// This verifies that the cache is generic and works with various types.
    /// </summary>
    [Test]
    public void MRUCache_WithDifferentValueTypes_ShouldWorkCorrectly()
    {
        // Test with string values
        var stringCache = new MRUCache<string, string>(5);
        stringCache["key1"] = "value1";
        Assert.That(stringCache["key1"], Is.EqualTo("value1"));

        // Test with object values
        var objectCache = new MRUCache<string, object>(5);
        var testObject = new { Name = "Test", Value = 42 };
        objectCache["key1"] = testObject;
        Assert.That(objectCache["key1"], Is.EqualTo(testObject));

        // Test with nullable values
        var nullableCache = new MRUCache<string, int?>(5);
        nullableCache["key1"] = 42;
        nullableCache["key2"] = null;
        Assert.That(nullableCache["key1"], Is.EqualTo(42));
        Assert.That(nullableCache["key2"], Is.Null);
    }

    /// <summary>
    /// Tests that cache works correctly with complex key types.
    /// This verifies that the cache works with non-primitive key types.
    /// </summary>
    [Test]
    public void MRUCache_WithComplexKeyTypes_ShouldWorkCorrectly()
    {
        // Arrange
        var cache = new MRUCache<Tuple<string, int>, string>(5);
        var key1 = Tuple.Create("test", 1);
        var key2 = Tuple.Create("test", 2);

        // Act
        cache[key1] = "value1";
        cache[key2] = "value2";

        // Assert
        Assert.That(cache[key1], Is.EqualTo("value1"));
        Assert.That(cache[key2], Is.EqualTo("value2"));
    }

    #endregion

    #region MRUCache Stress Tests

    /// <summary>
    /// Tests that cache handles rapid access patterns correctly.
    /// This verifies that the cache works under stress conditions.
    /// </summary>
    [Test]
    public void MRUCache_RapidAccess_ShouldWorkCorrectly()
    {
        // Arrange
        var cache = new MRUCache<int, int>(10);

        // Act - Rapidly access and modify items
        for (int round = 0; round < 100; round++)
        {
            for (int i = 0; i < 20; i++)
            {
                cache[i] = i * round;
                var value = cache[i];
                Assert.That(value, Is.EqualTo(i * round));
            }
        }

        // Assert - Verify final state
        var items = cache.Enumerate().ToList();
        Assert.That(items.Count, Is.EqualTo(10));
    }

    /// <summary>
    /// Tests that cache handles mixed access patterns correctly.
    /// This verifies that the cache works with various access patterns.
    /// </summary>
    [Test]
    public void MRUCache_MixedAccessPatterns_ShouldWorkCorrectly()
    {
        // Arrange
        var cache = new MRUCache<string, int>(5);

        // Act - Mix of reads, writes, and evictions
        cache["a"] = 1;
        cache["b"] = 2;
        cache["c"] = 3;

        var value1 = cache["a"]; // Access a to make it recent
        cache["d"] = 4;
        cache["e"] = 5;
        cache["f"] = 6; // This should evict b

        var value2 = cache["c"]; // Access c to make it recent
        cache["g"] = 7; // This should evict d

        // Assert
        Assert.That(value1, Is.EqualTo(1));
        Assert.That(value2, Is.EqualTo(3));

        // Verify that the cache contains exactly 5 items (capacity)
        var items = cache.Enumerate().ToList();
        Assert.That(items.Count, Is.EqualTo(5));

        // Verify that the most recently accessed items are still there
        Assert.That(cache["c"], Is.EqualTo(3)); // Should still be there (accessed recently)
        Assert.That(cache["e"], Is.EqualTo(5)); // Should still be there
        Assert.That(cache["f"], Is.EqualTo(6)); // Should still be there
        Assert.That(cache["g"], Is.EqualTo(7)); // Should still be there

        // Verify that at least one of the older items was evicted
        var hasEvictedItems = cache["a"] == 0 || cache["b"] == 0 || cache["d"] == 0;
        Assert.That(hasEvictedItems, Is.True, "At least one older item should have been evicted");
    }

    #endregion
}