
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Blackwood
{
/// <summary>
/// An item in the doubly linked list.
/// </summary>
/// <typeparam name="t">The type of item to cache.</typeparam>
public class CacheItem<t>
{
    /// <summary>
    /// The value to cache
    /// </summary>
    /// <value>The type of item to cache.</value>
    public readonly t key;

    /// <summary>
    /// The next item in the chain.
    /// </summary>
    internal CacheItem<t> next;

    /// <summary>
    /// The previous item in the chain.
    /// </summary>
    internal CacheItem<t> prev;

    /// <summary>
    /// Creates a cache slot to hold the item
    /// </summary>
    /// <param name="value">The value to cache</param>
    internal CacheItem(t value)
    {
        next = this;
        prev = this;
        this.key = value;
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
    internal void Link(CacheItem<t> before)
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
/// <typeparam name="kt"></typeparam>
/// <typeparam name="vt"></typeparam>
struct A<kt, vt>
{
    /// <summary>
    /// The key from the associative look up
    /// </summary>
    internal readonly kt key;

    /// <summary>
    /// The value that is the intended to be look ed 
    /// </summary>
    internal readonly vt value;
    internal A(kt key, vt value)
    {
        this.key = key;
        this.value = value;
    }
}

/// <summary>
/// An associative Most-Recently-Used cache.  The least recently used items are subject to
/// being ejected.
/// </summary>
/// <typeparam name="kt">The type of the key</typeparam>
/// <typeparam name="vt">The type of the item to cache</typeparam>
/// <remarks>A ring buffer is used track the oldest item for eviction.
/// If the dictionary is allowed to grow too big, it becomes a botle neck</remarks>
public class MRUCache<kt, vt>
{
    /// <summary>
    /// This is used to map the key to the value
    /// </summary>
    readonly ConcurrentDictionary<kt, A<CacheItem<kt>, vt>> lookup;

    /// <summary>
    /// The most recently used item.
    /// </summary>
    /// <value>The cache-item, specialized for type of the key, to hold the recently used item.</value>
    protected volatile CacheItem<kt> head;

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
        lookup = new ConcurrentDictionary<kt, A<CacheItem<kt>, vt>>(4,maxItems);
    }


    /// <summary>
    /// The number of items currently in the cache.
    /// </summary>
    int numItems;

    /// <summary>
    /// Inserts the given item onto the head of the list
    /// </summary>
    /// <param name="item">The item to insert</param>
    /// <returns>The item</returns>
    internal CacheItem<kt> Insert(CacheItem<kt> item)
    {
        lock (this)
        {
            if (head != item)
            {
                item.Unlink();
                item.Link(head);
                return head = item;
            }
        }
        return item;
    }

    /// <summary>
    /// Evicts the oldest item from the cache
    /// </summary>
    void Evict()
    {
        lock (this)
        {
            var prev = head.prev;
            prev.Unlink();
            lookup.TryRemove(prev.key, out _);
            numItems--;
            Evicts++;
        }
    }

    /// <summary>
    /// Allow iteration over all of the items in the cache
    /// </summary>
    /// <returns>Each of the cache slots.. the slot is returned to allow managing the list</returns>
    public IEnumerable<CacheItem<kt>> Enumerate()
    {
        var ret = new List<CacheItem<kt>>();
        // Is there any thing in there anyway?
        if (null == head)
            return ret ;
        lock (this)
        {
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
    public vt this[kt key]
    {
        get
        {
            // Look up a weak reference and, if still valid, return the value
            if (lookup.TryGetValue(key, out var w))
                lock(this)
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

        // Store a weak reference to the item
        set
        {
            // Look up a weak reference and, if still valid, move the item to the head
            if (lookup.TryGetValue(key, out var w))
                lock (this)
                {
                    // Unlink the previous one
                    w.key.Unlink();
                }
            CacheItem<kt> ret;
            lock (this)
            {
                // Add this item to the MRU list
                ret = Insert(new CacheItem<kt>(key));

                // See if we need to evict an old item
                numItems++;
                if (numItems > maxItems)
                    Evict();
                Inserts++;
            }
            lookup[key] = new A<CacheItem<kt>, vt>(ret, value);
        }
    }

}
}
