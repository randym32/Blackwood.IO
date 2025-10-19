// Copyright © 2020-2025 Randall Maas. All rights reserved.
// See LICENSE file in the project root for full license information.
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Blackwood;

/// <summary>
/// An item in the doubly linked list.
/// </summary>
/// <typeparam name="itemType">The type of item to cache.</typeparam>
public class CacheItem<itemType>
{
    /// <summary>
    /// The value to cache
    /// </summary>
    /// <value>The type of item to cache.</value>
    public readonly itemType key;

    /// <summary>
    /// The next item in the chain.
    /// </summary>
    internal CacheItem<itemType> next;

    /// <summary>
    /// The previous item in the chain.
    /// </summary>
    internal CacheItem<itemType> prev;

    /// <summary>
    /// Creates a cache slot to hold the item
    /// </summary>
    /// <param name="value">The value to cache</param>
    internal CacheItem(itemType value)
    {
        next = this;
        prev = this;
        key = value;
    }

    /// <summary>
    /// Removes this item from the chain
    /// </summary>
    internal void Unlink()
    {
        // This unlinks the current node by having our previous node point to
        // our next node, and our next node point back to our previous node
        next.prev = prev;
        prev.next = next;
        next = this;
        prev = this;
    }

    /// <summary>
    /// Put this item in before the given item
    /// </summary>
    /// <param name="before">The item to come after</param>
    internal void Link(CacheItem<itemType> before)
    {
        // Sanity check
        if (null == before)
        {
            next = this;
            prev = this;
            return;
        }
        // Set our next to point to the item that is after us
        next = before;
        // Inherit the previous item
        prev = before.prev;
        prev.next  = this;
        before.prev = this;
    }

}


/// <summary>
/// A helper that tracks the key, which is needed when evicting items from the cache
/// </summary>
/// <typeparam name="keyType">The type of the key used for lookup.</typeparam>
/// <typeparam name="valueType">The type of the value stored in the cache.</typeparam>
struct A<keyType, valueType>
{
    /// <summary>
    /// The key from the associative look up
    /// </summary>
    internal readonly keyType key;

    /// <summary>
    /// The value that is the intended to be look ed
    /// </summary>
    internal readonly valueType value;
    internal A(keyType key, valueType value)
    {
        this.key = key;
        this.value = value;
    }
}

/// <summary>
/// An associative Most-Recently-Used cache.  The least recently used items are subject to
/// being ejected.
/// </summary>
/// <typeparam name="keyType">The type of the key</typeparam>
/// <typeparam name="valueType">The type of the item to cache</typeparam>
/// <remarks>A ring buffer is used track the oldest item for eviction.
/// If the dictionary is allowed to grow too big, it becomes a botle neck</remarks>
public class MRUCache<keyType, valueType>
{
    /// <summary>
    /// This is used to map the key to the value
    /// </summary>
    readonly ConcurrentDictionary<keyType, A<CacheItem<keyType>, valueType>> lookup;

    /// <summary>
    /// The most recently used item.
    /// </summary>
    /// <value>The cache-item, specialized for type of the key, to hold the recently used item.</value>
    protected CacheItem<keyType> head;

    /// <summary>
    /// The maximum number of items to hold in the cache; more than this, the
    /// older items are evicted.
    /// </summary>
    readonly int maxItems;

    int Hits, Misses, Evicts, Inserts;

    /// <summary>
    /// Constructs the cache
    /// </summary>
    /// <param name="maxItems">The maximum number of items to hold in the queue</param>
    public MRUCache(int maxItems = 1024)
    {
        this.maxItems = maxItems;
        lookup = new ConcurrentDictionary<keyType, A<CacheItem<keyType>, valueType>>(4,maxItems);
    }


    /// <summary>
    /// The number of items currently in the cache.
    /// </summary>
    volatile int numItems;

    /// <summary>
    /// Inserts the given item onto the head of the list
    /// </summary>
    /// <param name="item">The item to insert</param>
    /// <returns>The item</returns>
    internal CacheItem<keyType> Insert(CacheItem<keyType> item)
    {
        if (head != item)
        {
            item.Unlink();
            item.Link(head);
            head = item;
        }
        return item;
    }

    /// <summary>
    /// Evicts the oldest item from the cache
    /// </summary>
    void Evict()
    {
        if (head != null)
        {
            var prev = head.prev;
            prev.Unlink();
            lookup.TryRemove(prev.key, out _);
            Interlocked.Decrement(ref numItems);
            Evicts++;
        }
    }

    /// <summary>
    /// Allow iteration over all of the items in the cache
    /// </summary>
    /// <returns>Each of the cache slots.. the slot is returned to allow managing the list</returns>
    public IEnumerable<CacheItem<keyType>> Enumerate()
    {
        var ret = new List<CacheItem<keyType>>();
        lock (this)
        {
            // Is there any thing in there anyway?
            if (null == head)
                return ret;

            // Give the first item
            ret.Add(head);
            // Go round until we reach the beginning
            for (var p = head.next; p != head; p = p.next)
                ret.Add(p);
        }
        return ret;
    }


    /// <summary>
    /// This is used to fetch the cached items
    /// </summary>
    /// <param name="key">The key used to associate with the item.</param>
    /// <returns>The associated value</returns>
    public valueType this[keyType key]
    {
        get
        {
            lock(this)
            {
                // Look up a weak reference and, if still valid, return the value
                if (lookup.TryGetValue(key, out var w))
                {
                    // Update the cache
                    // Move the item to the head of the cache
                    Insert(w.key);
                    Hits++;
                    return w.value;
                }

                // Didn't find the value
                Misses++;
                return default;
            }
        }

        // Store a weak reference to the item
        set
        {
            lock (this)
            {
                // Look up a weak reference and, if still valid, move the item to the head
                if (lookup.TryGetValue(key, out var w))
                {
                    // Unlink the previous one
                    w.key.Unlink();
                }

                // Add this item to the MRU list
                var ret = Insert(new CacheItem<keyType>(key));

                // See if we need to evict an old item
                var currentCount = Interlocked.Increment(ref numItems);
                if (currentCount > maxItems)
                    Evict();
                Inserts++;

                // Update the dictionary within the lock to prevent race conditions
                lookup[key] = new A<CacheItem<keyType>, valueType>(ret, value);
            }
        }
    }

}
