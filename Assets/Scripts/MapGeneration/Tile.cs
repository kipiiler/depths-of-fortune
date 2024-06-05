using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Tile
{
    public enum EdgeDirection
    {
        NORTH = 0,
        EAST = 1,
        SOUTH = 2,
        WEST = 3,
        NONE = -1
    }

    public string ResourcePath;
    public string[] Edges;
    public Dictionary<EdgeDirection, List<Tile>> rules;
    public Quaternion rotation;
    public string nameID;
    public double weight;

    public static List<Tile> CreateFromData(TileData data)
    {
        Tile t = new Tile(data.resoursePath, data.edges, data.nameID, data.weight);
        List<Tile> tiles = new List<Tile>();
        for (int i = 0; i < data.possible_rotation.Length; i++)
        {
            if (data.possible_rotation[i] == 0)
            {
                tiles.Add(t);
            }
            else
            {
                tiles.Add(t.Rotate(data.possible_rotation[i], data.nameID + " Rotate " + data.possible_rotation[i]));
            }
        }
        return tiles;
    }

    public static List<Tile> CreateListFromData(List<TileData> data)
    {
        List<Tile> tiles = new List<Tile>();
        foreach (TileData tileData in data)
        {
            tiles.AddRange(CreateFromData(tileData));
        }
        return tiles;
    }

    public static List<Tile> CreateListFromPath(string path)
    {
        List<TileData> tileDatas = TileDataList.CreateListFromFile(path);
        return CreateListFromData(tileDatas);
    }

    public Tile(string resourcePath, string[] edges, string nameID, double weight)
    {
        this.ResourcePath = resourcePath;
        this.nameID = nameID;
        this.Edges = edges;
        this.rules = new Dictionary<EdgeDirection, List<Tile>>();
        this.rules.Add(EdgeDirection.NORTH, new List<Tile>());
        this.rules.Add(EdgeDirection.EAST, new List<Tile>());
        this.rules.Add(EdgeDirection.SOUTH, new List<Tile>());
        this.rules.Add(EdgeDirection.WEST, new List<Tile>());
        this.rotation = Quaternion.identity;
        this.weight = weight;
    }

    public Tile(string resourcePath, string[] edges, Quaternion rotation, string nameID, double weight)
    {
        this.ResourcePath = resourcePath;
        this.Edges = edges;
        this.nameID = nameID;
        this.rules = new Dictionary<EdgeDirection, List<Tile>>();
        this.rules.Add(EdgeDirection.NORTH, new List<Tile>());
        this.rules.Add(EdgeDirection.EAST, new List<Tile>());
        this.rules.Add(EdgeDirection.SOUTH, new List<Tile>());
        this.rules.Add(EdgeDirection.WEST, new List<Tile>());
        this.rotation = rotation;
        this.weight = weight;
    }

    public void GenerateRules(List<Tile> tiles)
    {
        foreach (Tile tile in tiles)
        {
            for (int i = 0; i < 4; i++)
            {
                bool isMatched = IsMatch(Edges[i], tile.Edges[(int)GetOpposeDirection((EdgeDirection)i)]);
                if (isMatched)
                {
                    rules[(EdgeDirection)i].Add(tile);
                }
            }
        }
    }

    public void PrintRules()
    {
        string result = "";
        foreach (KeyValuePair<EdgeDirection, List<Tile>> rule in rules)
        {
            result = result + "Edge: " + rule.Key + " contains: ";
            for (int i = 0; i < rule.Value.Count; i++)
            {
                result += rule.Value[i].ResourcePath + " " + rule.Value[i].nameID + " ";
            }
            result += "\n";
        }
        Debug.Log(result);
    }

    private bool IsMatch(string edge1, string edge2)
    {
        // Check if the edges match reverse
        return edge1 == ReverseString(edge2);
    }

    private string ReverseString(string str)
    {
        char[] charArray = str.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }


    public static EdgeDirection GetOpposeDirection(EdgeDirection edge)
    {
        if (edge == EdgeDirection.NORTH)
        {
            return EdgeDirection.SOUTH;
        }
        else if (edge == EdgeDirection.EAST)
        {
            return EdgeDirection.WEST;
        }
        else if (edge == EdgeDirection.SOUTH)
        {
            return EdgeDirection.NORTH;
        }
        else
        {
            return EdgeDirection.EAST;
        };
    }

    public Tile Rotate(int rotationAmount, string name)
    {
        if (name == "")
        {
            Debug.Log("Name is empty");
            name = nameID;
        }

        string[] newEdges = new string[4];
        for (int i = 0; i < 4; i++)
        {
            newEdges[i] = Edges[i];
        }

        for (int i = 0; i < rotationAmount; i++)
        {
            string temp = newEdges[3];
            newEdges[3] = newEdges[2];
            newEdges[2] = newEdges[1];
            newEdges[1] = newEdges[0];
            newEdges[0] = temp;
        }
        Tile newTile = new Tile(ResourcePath, newEdges, rotation * Quaternion.Euler(0, 90 * rotationAmount, 0), name, weight);
        return newTile;
    }


    public GameObject DrawTile(Vector3 position)
    {
        // Draw the tile
        GameObject TileObject = (GameObject)UnityEngine.Object.Instantiate(Resources.Load(ResourcePath), position, rotation);
        return TileObject;
    }
}
