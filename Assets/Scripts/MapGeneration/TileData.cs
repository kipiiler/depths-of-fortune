using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TileData
{
    public string resoursePath;
    public string[] edges;
    public int[] possible_rotation;
    public double weight;
    public string nameID;

    public TileData(string resoursePath, string[] edges, int[] rotation, double weight, string nameID)
    {
        this.resoursePath = resoursePath;
        this.edges = edges;
        this.possible_rotation = rotation;
        this.weight = weight;
        this.nameID = nameID;
    }
}

[Serializable]
public class TileDataList
{
    public List<TileData> tiles;

    public TileDataList(List<TileData> tiles)
    {
        this.tiles = tiles;
    }

    public TileDataList(TileData[] tiles)
    {
        this.tiles = new List<TileData>(tiles);
    }

    public void SaveToFile(string path)
    {
        string result = JsonUtility.ToJson(this);
        // Create file if not exist
        if (!System.IO.File.Exists(path))
        {
            System.IO.File.Create(path).Dispose();
        }
        System.IO.File.WriteAllText(path, result);
    }

    public static void SaveToFile(List<TileData> list, string path)
    {
        TileDataList tileDataList = new TileDataList(list);
        string result = JsonUtility.ToJson(tileDataList);
        // Create file if not exist
        if (!System.IO.File.Exists(path))
        {
            System.IO.File.Create(path).Dispose();
        }
        System.IO.File.WriteAllText(path, result);
    }

    public static List<TileData> CreateListFromJSON(string jsonString)
    {
        TileDataList tileDataList = JsonUtility.FromJson<TileDataList>(jsonString);
        return tileDataList.tiles;
    }

    public static List<TileData> CreateListFromFile(string path)
    {
        string jsonString = System.IO.File.ReadAllText(path);
        return TileDataList.CreateListFromJSON(jsonString);
    }
}
