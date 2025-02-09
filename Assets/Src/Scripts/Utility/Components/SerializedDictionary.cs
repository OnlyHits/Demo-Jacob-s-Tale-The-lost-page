using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class SerializedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    public List<TKey> keys = new List<TKey>();
    public ICollection<TKey> Keys => keys;
    public List<TValue> values = new List<TValue>();
    public ICollection<TValue> Values => values;

    //ICollection<TKey> IDictionary<TKey, TValue>.Keys => keys;
    //ICollection<TKey> IDictionary<TKey, TValue>.Values => values;

    public SerializedDictionary()
    {
        keys = new List<TKey>();
        values = new List<TValue>();
    }

    public TValue this[TKey key]
    {
        get
        {
            int idxKey = keys.IndexOf(key);
            TValue value = values.ElementAt(idxKey);
            return value;
        }
        set
        {
            int idxKey = keys.IndexOf(key);
            if (idxKey <= 0)
            {
                keys.Add(key);
                values.Add(value);
            }
            else
            {
                values[idxKey] = value;
            }
        }
    }

    public bool IsReadOnly => false;
    public int Count => keys.Count;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < keys.Count; i++)
        {
            yield return new KeyValuePair<TKey, TValue>(keys[i], values[i]);
        }
    }

    public void Add(TKey key, TValue value)
    {
        keys.Add(key);
        values.Add(value);
    }
    public void Add(KeyValuePair<TKey, TValue> pair)
    {
        keys.Add(pair.Key);
        values.Add(pair.Value);
    }

    public bool Remove(TKey key)
    {
        var index = keys.IndexOf(key);
        if (index < 0) return false;
        keys.RemoveAt(index);
        values.RemoveAt(index);
        return true;
    }
    public bool Remove(KeyValuePair<TKey, TValue> pair)
    {
        var index = keys.IndexOf(pair.Key);
        if (index < 0) return false;
        keys.RemoveAt(index);
        values.RemoveAt(index);
        return true;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        int idxKey = keys.IndexOf(key);
        if (idxKey < 0)
        {
            value = default;
            return false;
        }
        value = values.ElementAt(idxKey);
        return true;
    }

    public void Clear()
    {
        keys.Clear();
        values.Clear();
    }

    public bool ContainsKey(TKey key)
    {
        int idxKey = keys.IndexOf(key);
        if (idxKey < 0)
        {
            return false;
        }
        return true;
    }
    public bool Contains(KeyValuePair<TKey, TValue> pair)
    {
        int idxKey = keys.IndexOf(pair.Key);
        if (idxKey < 0)
        {
            return false;
        }
        // Check if the value at the found index matches the expected value
        if (EqualityComparer<TValue>.Default.Equals(values[idxKey], pair.Value))
        {
            return true;
        }
        return true;
    }
    public bool Contains(TValue value)
    {
        int idxValue = values.IndexOf(value);
        if (idxValue < 0)
        {
            return false;
        }
        return true;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int startIndex)
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array), "The target array cannot be null.");
        }

        if (startIndex < 0 || startIndex > array.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex), "Start index is out of range.");
        }

        if (array.Length - startIndex < keys.Count)
        {
            throw new ArgumentException("The target array is too small to copy the elements.");
        }

        for (int i = 0; i < keys.Count; i++)
        {
            array[startIndex + i] = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
        }
    }
}
