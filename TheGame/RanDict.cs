using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;

namespace TheGame;


public class RanDict<T> : Dictionary<T, double>
{
    private System.Random _rand = new(RandomNumberGenerator.GetInt32(2147483647));
    private Dictionary<T, bool> _used;
    private bool _nonRepeating = false;
    public T Next()
    {
        if(_nonRepeating)
        {
            double sum = 0;
            foreach(KeyValuePair<T,double> kv in this) if(!_used[kv.Key]) sum += kv.Value;
            double x = _rand.NextDouble() * sum;
            foreach(KeyValuePair<T,double> kv in this)
            {
                if(_used[kv.Key]) continue;
                x -= kv.Value;
                if(x<0)
                {
                    _used[kv.Key] = true;
                    return kv.Key;
                }
            }
        }
        else
        {
            double x = _rand.NextDouble() * Values.Sum();
            foreach(KeyValuePair<T,double> kv in this)
            {
                x -= kv.Value;
                if(x<0) return kv.Key;
            }
        }
        throw new ArgumentOutOfRangeException();
    }
    public void BeginNonRepeating()
    {
        Debug.Assert(!_nonRepeating);
        _nonRepeating = true;
        if(_used == null)
        {
            _used = new();
            foreach(T t in Keys) _used.Add(t,false);
        }
    }
    public void EndNonRepeating()
    {
        Debug.Assert(_nonRepeating);
        _nonRepeating = false;
        foreach(T t in Keys) _used[t] = false;
    }
}