using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dict<T>
{
    [SerializeField] List<Item> values;

    public T Find(string key)
    {
        foreach (Item item in values)
        {
            if (item.Key == key)
            {
                return item.Value;
            }
        }
        return default;
    }

    public void Add(string key, T value)
    {
        values.Add(new Item { Key = key, Value = value});
    }

    [System.Serializable] 
    public struct Item
    {
        public string Key;
        public T Value;
    }
}