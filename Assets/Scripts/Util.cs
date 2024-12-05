using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static void ConvertDictionaryToArrays(Dictionary<ulong, int> dict, out ulong[] keys, out int[] values)
    {
        keys = new ulong[dict.Count];
        values = new int[dict.Count];

        int index = 0;
        foreach (var pair in dict)
        {
            keys[index] = pair.Key;
            values[index] = pair.Value;
            index++;
        }
    }

    public static Dictionary<ulong, int> ConvertArraysToDictionary(ulong[] keys, int[] values)
    {
        Dictionary<ulong, int> dict = new Dictionary<ulong, int>();

        for (int i = 0; i < keys.Length; i++)
        {
            dict[keys[i]] = values[i];
        }

        return dict;
    }

    public static void ConvertDictionaryToArrays(Dictionary<ulong, Color> dict, out ulong[] keys, out Color[] values)
    {
        keys = new ulong[dict.Count];
        values = new Color[dict.Count];

        int index = 0;
        foreach (var pair in dict)
        {
            keys[index] = pair.Key;
            values[index] = pair.Value;
            index++;
        }
    }

    public static Dictionary<ulong, Color> ConvertArraysToDictionary(ulong[] keys, Color[] values)
    {
        Dictionary<ulong, Color> dict = new Dictionary<ulong, Color>();

        for (int i = 0; i < keys.Length; i++)
        {
            dict[keys[i]] = values[i];
        }

        return dict;
    }
}
