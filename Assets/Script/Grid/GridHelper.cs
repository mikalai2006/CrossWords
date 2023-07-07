using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridHelper
{
  private Grid<GridNode> _grid;
  public Grid<GridNode> Grid
  {
    get { return _grid; }
    private set { }
  }

  public GridHelper(int width, int height, float cellSize = 0.5f)
  {
    _grid = new Grid<GridNode>(width, height, cellSize, this, (Grid<GridNode> grid, GridHelper gridHelper, int x, int y) => new GridNode(grid, gridHelper, x, y));
  }

  public List<GridNode> GetAllGridNodes()
  {
    List<GridNode> list = new List<GridNode>(_grid.GetWidth() * _grid.GetHeight());
    if (_grid != null)
    {
      for (int x = 0; x < _grid.GetWidth(); x++)
      {
        for (int y = 0; y < _grid.GetHeight(); y++)
        {
          list.Add(_grid.GetGridObject(new Vector2Int(x, y)));
        }
      }
    }
    return list;
  }

  public SerializableDictionary<char, List<GridNode>> GetGroupNodeChars()
  {
    var result = new SerializableDictionary<char, List<GridNode>>();
    var allNodes = GetAllGridNodes()
      .Where(t =>
        t.StateNode.HasFlag(StateNode.Occupied)
        && !t.StateNode.HasFlag(StateNode.Open)
        && !t.StateNode.HasFlag(StateNode.Hint)
        // && !t.StateNode.HasFlag(StateNode.Entity)
        )
      .ToList();

    for (int i = 0; i < allNodes.Count; i++)
    {
      var charText = allNodes.ElementAt(i).OccupiedChar.charTextValue;
      if (result.ContainsKey(charText))
      {
        result[charText].Add(allNodes.ElementAt(i));
      }
      else
      {
        result[charText] = new List<GridNode>() { allNodes.ElementAt(i) };
      }
    }
    return result;
  }


  public GridNode GetNode(int x, int y)
  {
    return _grid.GetGridObject(new Vector2Int(x, y));
  }
  public GridNode GetNode(Vector2 pos)
  {
    return _grid.GetGridObject(new Vector2Int((int)pos.x, (int)pos.y));
  }
  public GridNode GetNode(Vector2Int pos)
  {
    return _grid.GetGridObject(pos);
  }

  public List<GridNode> FindNodeForSpawnWord(string word, int index)
  {
    var countChars = word.Length;
    List<GridNode> result = new();
    var potentialNodes = GetAllGridNodes()
      .Where(t => t.x <= _grid.GetWidth() - countChars && t.y <= index && t.StateNode.HasFlag(StateNode.Empty))
      .OrderBy(t => t.y)
      .ThenBy(t => t.x);

    var chooseNode = potentialNodes.FirstOrDefault();
    if (chooseNode != null) result = GetLineNodesByCount(chooseNode, countChars);
    return result;
  }

  private List<GridNode> GetLineNodesByCount(GridNode startNode, int count)
  {
    List<GridNode> result = GetAllGridNodes()
      .Where(t => t.y == startNode.y && t.x >= startNode.x && t.x < startNode.x + count)
      .OrderBy(t => t.x)
      .ToList();
    foreach (var node in result)
    {
      node.StateNode ^= StateNode.Empty;
    }
    var lastNode = result.LastOrDefault();
    if (GetNode(lastNode.x + 1, lastNode.y) != null)
    {
      GetNode(lastNode.x + 1, lastNode.y).StateNode ^= StateNode.Empty;
    }

    return result;
  }

  public List<GridNode> GetEqualsHiddenNeighbours()
  {
    List<GridNode> result = new();

    foreach (var node in GetAllGridNodes())
    {
      if (
        node != null
        && node.StateNode.HasFlag(StateNode.Occupied)
        && node.StateNode.HasFlag(StateNode.Open)
        )
      {
        var equalHiddenNei = FindNeighboursNodesOfByEqualChar(node);
        result.AddRange(equalHiddenNei);
      }
    }

    return result;
  }

  public List<GridNode> FindNeighboursNodesOfByEqualChar(GridNode startNode)
  {
    List<GridNode> result = new();
    // Debug.Log($"FindNeighboursNodesOfByEqualChar::: {startNode.ToString()}");
    if (
        startNode.TopNode != null
        && !startNode.TopNode.StateNode.HasFlag(StateNode.Open)
        && !startNode.TopNode.StateNode.HasFlag(StateNode.Hint)
        && startNode.TopNode.OccupiedChar != null
        && startNode.OccupiedChar.charTextValue == startNode.TopNode.OccupiedChar.charTextValue
      )
    {
      result.Add(startNode.TopNode);
    }
    // Debug.Log($"startNode.TopNode::: {startNode.TopNode.ToString()}");
    if (
        startNode.BottomNode != null
        && !startNode.BottomNode.StateNode.HasFlag(StateNode.Open)
        && !startNode.BottomNode.StateNode.HasFlag(StateNode.Hint)
        && startNode.BottomNode.OccupiedChar != null
        && startNode.OccupiedChar.charTextValue == startNode.BottomNode.OccupiedChar.charTextValue
      )
    {
      result.Add(startNode.BottomNode);
    }
    // Debug.Log($"startNode.BottomNode::: {startNode.TopNode.ToString()}");

    return result;
  }

  public GridNode GetRandomNodeWithHiddenChar()
  {
    return GetAllGridNodes()
      .Where(t =>
        t.StateNode.HasFlag(StateNode.Occupied)
        && !t.StateNode.HasFlag(StateNode.Open)
        && !t.StateNode.HasFlag(StateNode.Entity)
        && !t.StateNode.HasFlag(StateNode.Hint)
        && !t.StateNode.HasFlag(StateNode.Bonus)
      )
      .OrderBy(t => UnityEngine.Random.value)
      .FirstOrDefault();
  }

  /// <summary>
  /// Get All neighbours node with char
  /// </summary>
  /// <param name="startNode"></param>
  /// <param name="isDiagonal"></param>
  /// <returns></returns>
  public List<GridNode> GetAllNeighboursWithChar(GridNode startNode, bool isDiagonal = true)
  {
    List<GridNode> neighbours = new();

    var leftNode = startNode.LeftNode;
    if (leftNode != null && leftNode.StateNode.HasFlag(StateNode.Occupied))
    {
      neighbours.Add(leftNode);
    }
    var rightNode = startNode.RightNode;
    if (rightNode != null && rightNode.StateNode.HasFlag(StateNode.Occupied))
    {
      neighbours.Add(rightNode);
    }
    var topNode = startNode.TopNode;
    if (topNode != null && topNode.StateNode.HasFlag(StateNode.Occupied))
    {
      neighbours.Add(topNode);
    }
    var bottomNode = startNode.BottomNode;
    if (bottomNode != null && bottomNode.StateNode.HasFlag(StateNode.Occupied))
    {
      neighbours.Add(bottomNode);
    }
    if (isDiagonal)
    {
      var bottomLeftNode = GetNode(startNode.x - 1, startNode.y - 1);
      if (bottomLeftNode != null && bottomLeftNode.StateNode.HasFlag(StateNode.Occupied))
      {
        neighbours.Add(bottomLeftNode);
      }
      var topLeftNode = GetNode(startNode.x - 1, startNode.y + 1);
      if (topLeftNode != null && topLeftNode.StateNode.HasFlag(StateNode.Occupied))
      {
        neighbours.Add(topLeftNode);
      }
      var bottomRightNode = GetNode(startNode.x + 1, startNode.y - 1);
      if (bottomRightNode != null && bottomRightNode.StateNode.HasFlag(StateNode.Occupied))
      {
        neighbours.Add(bottomRightNode);
      }
      var topRightNode = GetNode(startNode.x + 1, startNode.y + 1);
      if (topRightNode != null && topRightNode.StateNode.HasFlag(StateNode.Occupied))
      {
        neighbours.Add(topRightNode);
      }
    }

    return neighbours;
  }

  /// <summary>
  /// Get all nodes matching the condition
  /// </summary>
  /// <param name="callback">condition</param>
  /// <returns></returns>
  public List<GridNode> GetNodeForEntity(Func<GridNode, bool> callback)
  {
    List<GridNode> result = new();

    foreach (var node in GetAllGridNodes())
    {
      if (callback(node))
      {
        result.Add(node);
      }
    }

    return result;
  }

  /// <summary>
  /// Get all nodes with char by x
  /// </summary>
  /// <param name="startNode"></param>
  /// <returns></returns>
  public List<GridNode> GetAllNodeWithHiddenCharByX(GridNode startNode)
  {
    List<GridNode> result = new();

    foreach (var node in GetAllGridNodes().Where(t =>
      t.StateNode.HasFlag(StateNode.Occupied)
      && !t.StateNode.HasFlag(StateNode.Open)
      && !t.StateNode.HasFlag(StateNode.Entity)
      && !t.StateNode.HasFlag(StateNode.Bonus)
    ))
    {
      if (node != startNode && node.x == startNode.x)
      {
        result.Add(node);
      }
    }

    return result;
  }

  /// <summary>
  /// Get all nodes with gidden char
  /// </summary>
  /// <returns></returns>
  public List<GridNode> GetAllNodeWithHiddenChar()
  {
    List<GridNode> result = new();

    foreach (var node in GetAllGridNodes())
    {
      if (
        node.StateNode.HasFlag(StateNode.Occupied)
        && !node.StateNode.HasFlag(StateNode.Open)
        && !node.StateNode.HasFlag(StateNode.Entity)
        && !node.StateNode.HasFlag(StateNode.Hint)
        && !node.StateNode.HasFlag(StateNode.Bonus)
        )
      {
        result.Add(node);
      }
    }

    return result;
  }

  public List<GridNode> GetExistEntityByVertical(GridNode startNode)
  {
    List<GridNode> result = new();

    foreach (var node in GetAllGridNodes().Where(t => t.StateNode.HasFlag(StateNode.Occupied)))
    {
      if (node != startNode && node.x == startNode.x && node.StateNode.HasFlag(StateNode.Entity))
      {
        result.Add(node);
      }
    }

    return result;
  }

}
