using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;

namespace WebPWrapper.Classes
{
    class ReadWriteLockDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private Dictionary<TKey, TValue> innerDict;
        private ReaderWriterLockSlim locking;

        public ReadWriteLockDictionary() : this(EqualityComparer<TKey>.Default) { }

        public ReadWriteLockDictionary(IEqualityComparer<TKey> comparer)
        {
            this.locking = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            this.innerDict = new Dictionary<TKey, TValue>(comparer);
        }

        public TValue this[TKey key]
        {
            get
            {
                if (this.locking.IsReadLockHeld)
                {
                    return this.innerDict[key];
                }
                else
                {
                    this.locking.EnterReadLock();
                    try
                    {
                        return this.innerDict[key];
                    }
                    finally
                    {
                        this.locking.ExitReadLock();
                    }
                }
            }
            set
            {
                if (this.locking.IsWriteLockHeld)
                {
                    this.innerDict[key] = value;
                }
                else
                {
                    this.locking.EnterWriteLock();
                    try
                    {
                        this.innerDict[key] = value;
                    }
                    finally
                    {
                        this.locking.EnterWriteLock();
                    }
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                if (this.locking.IsReadLockHeld)
                {
                    return this.innerDict.Keys;
                }
                else
                {
                    this.locking.EnterReadLock();
                    try
                    {
                        return this.innerDict.Keys;
                    }
                    finally
                    {
                        this.locking.ExitReadLock();
                    }
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                if (this.locking.IsReadLockHeld)
                {
                    return this.innerDict.Values;
                }
                else
                {
                    this.locking.EnterReadLock();
                    try
                    {
                        return this.innerDict.Values;
                    }
                    finally
                    {
                        this.locking.ExitReadLock();
                    }
                }
            }
        }

        public int Count
        {
            get
            {
                if (this.locking.IsReadLockHeld)
                {
                    return this.innerDict.Count;
                }
                else
                {
                    this.locking.EnterReadLock();
                    try
                    {
                        return this.innerDict.Count;
                    }
                    finally
                    {
                        this.locking.ExitReadLock();
                    }
                }
            }
        }

        public bool IsReadOnly => false;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get
            {
                if (this.locking.IsReadLockHeld)
                {
                    return this.innerDict.Keys;
                }
                else
                {
                    this.locking.EnterReadLock();
                    try
                    {
                        return this.innerDict.Keys;
                    }
                    finally
                    {
                        this.locking.ExitReadLock();
                    }
                }
            }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get
            {
                if (this.locking.IsReadLockHeld)
                {
                    return this.innerDict.Values;
                }
                else
                {
                    this.locking.EnterReadLock();
                    try
                    {
                        return this.innerDict.Values;
                    }
                    finally
                    {
                        this.locking.ExitReadLock();
                    }
                }
            }
        }

        public void Add(TKey key, TValue value)
        {
            if (this.locking.IsWriteLockHeld)
            {
                this.innerDict.Add(key, value);
            }
            else
            {
                this.locking.EnterWriteLock();
                try
                {
                    this.innerDict.Add(key, value);
                }
                finally
                {
                    this.locking.ExitWriteLock();
                }
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (this.locking.IsWriteLockHeld)
            {
                this.innerDict.Add(item.Key, item.Value);
            }
            else
            {
                this.locking.EnterWriteLock();
                try
                {
                    this.innerDict.Add(item.Key, item.Value);
                }
                finally
                {
                    this.locking.ExitWriteLock();
                }
            }
        }

        public void Clear()
        {
            if (this.locking.IsWriteLockHeld)
            {
                this.innerDict.Clear();
            }
            else
            {
                this.locking.EnterWriteLock();
                try
                {
                    this.innerDict.Clear();
                }
                finally
                {
                    this.locking.ExitWriteLock();
                }
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (this.locking.IsReadLockHeld)
            {
                return (this.innerDict.ContainsKey(item.Key) && this.innerDict.ContainsValue(item.Value));
            }
            else
            {
                this.locking.EnterReadLock();
                try
                {
                    return (this.innerDict.ContainsKey(item.Key) && this.innerDict.ContainsValue(item.Value));
                }
                finally
                {
                    this.locking.ExitReadLock();
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            if (this.locking.IsReadLockHeld)
            {
                return this.innerDict.ContainsKey(key);
            }
            else
            {
                this.locking.EnterReadLock();
                try
                {
                    return this.innerDict.ContainsKey(key);
                }
                finally
                {
                    this.locking.ExitReadLock();
                }
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (this.locking.IsReadLockHeld)
            {
                foreach (var item in this.innerDict)
                {
                    array[arrayIndex++] = item;
                }
            }
            else
            {
                this.locking.EnterReadLock();
                try
                {
                    foreach (var item in this.innerDict)
                    {
                        array[arrayIndex++] = item;
                    }
                }
                finally
                {
                    this.locking.ExitReadLock();
                }
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => ((IDictionary<TKey, TValue>)innerDict).GetEnumerator();

        public bool Remove(TKey key)
        {
            if (this.locking.IsWriteLockHeld)
            {
                return this.innerDict.Remove(key);
            }
            else
            {
                this.locking.EnterWriteLock();
                try
                {
                    return this.innerDict.Remove(key);
                }
                finally
                {
                    this.locking.ExitWriteLock();
                }
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (this.locking.IsWriteLockHeld)
            {
                return (this.innerDict.ContainsValue(item.Value) && this.innerDict.Remove(item.Key));
            }
            else
            {
                this.locking.EnterWriteLock();
                try
                {
                    return (this.innerDict.ContainsValue(item.Value) && this.innerDict.Remove(item.Key));
                }
                finally
                {
                    this.locking.ExitWriteLock();
                }
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (this.locking.IsReadLockHeld)
            {
                return this.innerDict.TryGetValue(key, out value);
            }
            else
            {
                this.locking.EnterReadLock();
                try
                {
                    return this.innerDict.TryGetValue(key, out value);
                }
                finally
                {
                    this.locking.ExitReadLock();
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<TKey, TValue>)innerDict).GetEnumerator();
    }
}
