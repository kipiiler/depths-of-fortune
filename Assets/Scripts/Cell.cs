using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Cell
{
    public bool collapsed = false;
    public List<string> possibleOptions = new List<string>();
    public string chooseOption;

    public Dictionary<Tile.EdgeDirection, Cell> neighbors = new Dictionary<Tile.EdgeDirection, Cell>();


    public Cell(List<Tile> listOfTiles)
    {
        foreach (Tile tile in listOfTiles)
        {
            AddOption(tile.nameID);
        }
    }

    public void AddNeighbor(Tile.EdgeDirection direction, Cell neighbor)
    {
        neighbors.Add(direction, neighbor);
    }

    public Cell GetNeighbor(Tile.EdgeDirection direction)
    {
        return neighbors[direction];
    }

    public void Collapse()
    {
        collapsed = true;
        chooseOption = possibleOptions[UnityEngine.Random.Range(0, possibleOptions.Count)];
        this.possibleOptions = new List<string> { chooseOption };
    }

    public int getEntropy()
    {
        return possibleOptions.Count;
    }

    public void AddOption(string option)
    {
        possibleOptions.Add(option);
    }

    public void RemoveOption(string option)
    {
        possibleOptions.Remove(option);
    }

    public List<Tile.EdgeDirection> GetDirection()
    {
        List<Tile.EdgeDirection> directions = new List<Tile.EdgeDirection>();
        foreach (Tile.EdgeDirection direction in neighbors.Keys)
        {
            directions.Add(direction);
        }
        return directions;
    }

    public bool ConstrainsOption(List<string> possibleOptions, Tile.EdgeDirection direction, Dictionary<string, Dictionary<Tile.EdgeDirection, List<Tile>>> tileRules)
    {
        bool isReduced = false;
        if (this.getEntropy() > 0)
        {
            List<string> connectors = new List<string>();
            foreach (string option in possibleOptions)
            {
                foreach (Tile tile in tileRules[option][direction])
                {
                    if (!connectors.Contains(tile.nameID))
                    {
                        connectors.Add(tile.nameID);
                    }
                }
            }

            List<string> optionsToRemove = new List<string>();

            foreach (string option in this.possibleOptions)
            {
                if (!connectors.Contains(option))
                {
                    optionsToRemove.Add(option);
                }
            }

            foreach (string option in optionsToRemove)
            {
                RemoveOption(option);
                isReduced = true;
            }
        }

        return isReduced;
    }

    public string PrintOptions()
    {
        string result = "";
        foreach (string option in possibleOptions)
        {
            result += option + " ";
        }
        return result;
    }
}
