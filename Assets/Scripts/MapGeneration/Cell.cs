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

    public void Collapse(Dictionary<string, double> weights)
    {
        collapsed = true;

        this.chooseOption = ChooseOption(weights, possibleOptions);

        while (chooseOption.Contains("Start") || chooseOption.Contains("End"))
        {
            this.chooseOption = ChooseOption(weights, possibleOptions);
        }

        this.possibleOptions = new List<string> { chooseOption };
    }

    private string ChooseOption(Dictionary<string, double> weights, List<string> possibleOptions)
    {
        if (possibleOptions.Count == 0)
        {
            throw new System.Exception("No options to choose from.");
            // return possibleOptions[0];/
        }
        int p = UnityEngine.Random.Range(0, 100);
        foreach (string option in possibleOptions)
        {
            if (p < weights[option] * 100)
            {
                return option;
            }
            else
            {
                p -= (int)(weights[option] * 100);
            }
        }

        // In case something goes wrong, return the first option (fail-safe)
        try
        {
            return possibleOptions[0];
        }
        catch (Exception e)
        {
            string result = "";
            foreach (string option in possibleOptions)
            {
                result += option + " ";
            }
            Debug.Log("Error: " + e + "Options: " + result);
        }
        return possibleOptions[0];
    }


    public void Collapse(string option)
    {
        if (collapsed)
        {
            throw new System.Exception("Cell is already collapsed.");
        }

        if (!possibleOptions.Contains(option))
        {
            throw new System.Exception("Option not in possible options.");
        }

        collapsed = true;
        chooseOption = option;
        this.possibleOptions = new List<string> { chooseOption };
    }

    public void Collapse(List<string> options, Dictionary<string, double> weights)
    {
        if (collapsed)
        {
            throw new System.Exception("Cell is already collapsed.");
        }

        if (options.Count == 0)
        {
            throw new System.Exception("No options to collapse.");
        }

        List<string> validOptions = new List<string>();
        foreach (string option in options)
        {
            if (possibleOptions.Contains(option))
            {
                validOptions.Add(option);
            }
        }

        if (validOptions.Count == 0)
        {
            // print valid option
            string result = "";
            foreach (string option in possibleOptions)
            {
                result += option + " ";
            }
            result += "While options to collapse: ";
            foreach (string option in options)
            {
                result += option + " ";
            }
            throw new System.Exception("No valid options to collapse. Options: " + result);
        }

        collapsed = true;
        chooseOption = ChooseOption(weights, validOptions);
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
