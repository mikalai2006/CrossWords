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
          list.Add(_grid.GetGridObject(new Vector3Int(x, y)));
        }
      }
    }
    return list;
  }

  public SerializableDictionary<string, List<GridNode>> GetGroupNodeChars()
  {
    var result = new SerializableDictionary<string, List<GridNode>>();
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
      var charText = allNodes.ElementAt(i).OccupiedChar.CharValue;
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
    return _grid.GetGridObject(new Vector3Int(Math.Abs(x), Math.Abs(y)));
  }
  public GridNode GetNode(Vector3 pos)
  {
    return _grid.GetGridObject(new Vector3Int((int)Math.Abs(pos.x), (int)Math.Abs(pos.y)));
  }
  public GridNode GetNode(Vector3Int pos)
  {
    return _grid.GetGridObject(new Vector3Int(Math.Abs(pos.x), Math.Abs(pos.y)));
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


  public List<GridNode> FindNodeForSpawnBonusWord(string word)
  {
    var countChars = word.Length;
    List<GridNode> result = new();
    var potentialNodes = GetAllGridNodes()
      .Where(t => t.x == 1 && t.y < countChars && t.StateNode.HasFlag(StateNode.Empty))
      .OrderBy(t => -t.y)
      .ToList();
    return potentialNodes;
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
        && startNode.OccupiedChar.CharValue == startNode.TopNode.OccupiedChar.CharValue
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
        && startNode.OccupiedChar.CharValue == startNode.BottomNode.OccupiedChar.CharValue
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

  public BaseWord SetWord(string word, GridNode startNode, DirectionWord directionWord)
  {
    List<char> chars = word.ToCharArray().ToList();

    // Create word.
    var newWord = new WordHidden();
    newWord.Init(word, directionWord);

    // Close start and end node.
    GridNode prevStartNode = GetPrevNode(startNode, directionWord);
    if (prevStartNode != null)
    {
      prevStartNode.SetDisable();
    }
    GridNode nextLastNode = GetNextNode(startNode, directionWord);
    if (nextLastNode != null)
    {
      nextLastNode.SetDisable();
    }

    // check exist node with char.
    var newChar = startNode.OccupiedChar;

    // create new char for node.
    if (newChar == null)
    {
      newChar = new CharHidden();
      newChar.Init(chars.ElementAt(0).ToString(), startNode, newWord);
    }
    else
    {
      newWord.AddCrossWord(newChar.OccupiedWord);
    }

    newWord.AddChar(newChar, startNode);

    chars.RemoveAt(0);

    int x = startNode.x;
    int y = startNode.y;

    while (chars.Count > 0)
    {
      string currentChar = chars.ElementAt(0).ToString();

      switch (directionWord)
      {
        case DirectionWord.Horizontal:
          x++;
          break;
        default:
          y++;
          break;
      }

      GridNode node = GetNode(x, y);
      // Debug.Log($"Add Node {node}");
      if (node != null)
      {
        // check exist node with char.
        var newCharNode = node.OccupiedChar;

        // create new char for node.
        if (newCharNode == null)
        {
          newCharNode = new CharHidden();
          newCharNode.Init(currentChar, node, newWord);
        }
        else
        {
          newWord.AddCrossWord(newCharNode.OccupiedWord);
        }

        newWord.AddChar(newCharNode, node);
      }

      chars.RemoveAt(0);
    }

    return newWord;
  }

  public WordItemStartNode FindStartNodeForWord(string word)
  {
    // Debug.Log($"FindStartNodeForWord::: {word}");
    WordItemStartNode result = new()
    {
      directionWord = DirectionWord.Horizontal,
      node = null
    };

    List<GridNode> allEmptyNodes = GetAllGridNodes()
      .Where(t => !t.StateNode.HasFlag(StateNode.Disable))
      // .OrderBy(t => UnityEngine.Random.value)
      .ToList();
    // Debug.Log($"FindStartNodeForWord::: allEmptyNodes={allEmptyNodes.Count}");

    for (int j = 0; j < allEmptyNodes.Count; j++)
    {
      GridNode potentialNode = allEmptyNodes.ElementAt(j);

      // if (potentialNode.StateNode.HasFlag(StateNode.Use)) continue;
      // if (potentialNode == null) return result;

      GridNode firstNode = CheckNodesHorizontal(word, potentialNode);
      if (firstNode == null)
      {
        result.directionWord = DirectionWord.Vertical;
        firstNode = CheckNodesVertical(word, potentialNode);
      }
      else
      {
        result.directionWord = DirectionWord.Horizontal;
      }

      if (firstNode == null) continue;

      result.node = firstNode;
      result.node.SetUse();
      if (result.node != null) return result;
    }
    return result;
  }

  public GridNode CheckNodesVertical(string word, GridNode startNode)
  {
    GridNode resultStartNode = null;

    List<GridNode> listNodes = GetListNodeForPotentialStarNode(startNode, word, DirectionWord.Vertical);

    // cancel, if not exist occupied node for list potential nodes or count nodes less word length
    if (listNodes.Count < word.Length) return resultStartNode;
    if (listNodes.Where(t => t.StateNode.HasFlag(StateNode.Occupied)).Count() == 0) return resultStartNode;

    List<GridNode> checkedNodes = new();
    for (int i = 0; i < listNodes.Count; i++)
    {
      string c = word.ElementAt(i).ToString();
      GridNode checkedNode = listNodes.ElementAt(i);

      if (
        (
          (checkedNode.RightNode == null || (checkedNode.RightNode != null && checkedNode.RightNode.StateNode.HasFlag(StateNode.Empty)))
          && (checkedNode.LeftNode == null || (checkedNode.LeftNode != null && checkedNode.LeftNode.StateNode.HasFlag(StateNode.Empty)))
          && checkedNode.StateNode.HasFlag(StateNode.Empty)
        )
        ||
        (
          checkedNode.StateNode.HasFlag(StateNode.Occupied)
          && checkedNode.OccupiedChar.CharValue == c.ToString()
          && (checkedNode.TopNode == null || (checkedNode.TopNode != null && checkedNode.TopNode.StateNode.HasFlag(StateNode.Empty)))
          && (checkedNode.BottomNode == null || (checkedNode.BottomNode != null && checkedNode.BottomNode.StateNode.HasFlag(StateNode.Empty)))
        )
      )
      {
        checkedNodes.Add(checkedNode);
        // return resultStartNode;
      }
      // else {
      //   return resultStartNode;
      // }
    }
    if (checkedNodes.Count != listNodes.Count) return resultStartNode;

    // Check prev next
    GridNode prevStartNode = GetPrevNode(startNode, DirectionWord.Vertical);
    if (prevStartNode != null && (prevStartNode.StateNode.HasFlag(StateNode.Disable) || prevStartNode.StateNode.HasFlag(StateNode.Occupied) || prevStartNode.StateNode.HasFlag(StateNode.Use)))
    {
      return resultStartNode;
    }

    GridNode lastNode = GetNode(startNode.x, startNode.y + word.Length - 1);
    GridNode nextLastNode = GetNextNode(lastNode, DirectionWord.Vertical);
    if (
      nextLastNode != null
      &&
      (
        nextLastNode.StateNode.HasFlag(StateNode.Disable)
        || nextLastNode.StateNode.HasFlag(StateNode.Occupied)
        || nextLastNode.StateNode.HasFlag(StateNode.Use)
      )
    )
    {
      return resultStartNode;
    }

    resultStartNode = startNode;

    return resultStartNode;
  }


  public GridNode CheckNodesHorizontal(string word, GridNode startNode) //, DirectionCheck dirCheck
  {
    GridNode resultStartNode = null;

    List<GridNode> listNodes = GetListNodeForPotentialStarNode(startNode, word, DirectionWord.Horizontal);

    // cancel, if not exist occupied node for list potential nodes or count nodes less word length
    if (listNodes.Count < word.Length) return resultStartNode;
    if (listNodes.Where(t => t.StateNode.HasFlag(StateNode.Occupied)).Count() == 0) return resultStartNode;

    List<GridNode> checkedNodes = new();
    for (int i = 0; i < listNodes.Count; i++)
    {
      string c = word.ElementAt(i).ToString();
      GridNode checkedNode = listNodes.ElementAt(i);

      if (
        (
          (checkedNode.TopNode == null || (checkedNode.TopNode != null && checkedNode.TopNode.StateNode.HasFlag(StateNode.Empty)))
          && (checkedNode.BottomNode == null || (checkedNode.BottomNode != null && checkedNode.BottomNode.StateNode.HasFlag(StateNode.Empty)))
          && checkedNode.StateNode.HasFlag(StateNode.Empty)
        )
        ||
        (
          checkedNode.StateNode.HasFlag(StateNode.Occupied)
          && checkedNode.OccupiedChar.CharValue == c.ToString()
          && (checkedNode.LeftNode == null || (checkedNode.LeftNode != null && checkedNode.LeftNode.StateNode.HasFlag(StateNode.Empty)))
          && (checkedNode.RightNode == null || (checkedNode.RightNode != null && checkedNode.RightNode.StateNode.HasFlag(StateNode.Empty)))
        )
      )
      {
        checkedNodes.Add(checkedNode);
        // return resultStartNode;
      }
      // else {
      //   return resultStartNode;
      // }
    }
    if (checkedNodes.Count != listNodes.Count) return resultStartNode;

    // Check prev next
    GridNode prevStartNode = GetPrevNode(startNode, DirectionWord.Horizontal);
    if (prevStartNode != null && (prevStartNode.StateNode.HasFlag(StateNode.Disable) || prevStartNode.StateNode.HasFlag(StateNode.Occupied) || prevStartNode.StateNode.HasFlag(StateNode.Use)))
    {
      return resultStartNode;
    }

    GridNode lastNode = GetNode(startNode.x + word.Length - 1, startNode.y);
    GridNode nextLastNode = GetNextNode(lastNode, DirectionWord.Horizontal);
    if (nextLastNode != null && (nextLastNode.StateNode.HasFlag(StateNode.Disable) || nextLastNode.StateNode.HasFlag(StateNode.Occupied) || nextLastNode.StateNode.HasFlag(StateNode.Use)))
    {
      return resultStartNode;
    }

    resultStartNode = startNode;

    return resultStartNode;
  }


  private List<GridNode> GetListNodeForPotentialStarNode(GridNode startNode, string word, DirectionWord directionWord)
  {
    List<GridNode> result = new() { startNode };

    for (int i = 1; i < word.Length; i++)
    {
      GridNode node = GetNode(
        startNode.x + (directionWord == DirectionWord.Horizontal ? i : 0),
        startNode.y + (directionWord == DirectionWord.Vertical ? i : 0)
      );

      if (node != null)
      {
        result.Add(node);
      }
    }

    return result;
  }


  private GridNode GetPrevNode(GridNode startNode, DirectionWord directionWord)
  {
    return directionWord == DirectionWord.Horizontal
      ? GetNode(startNode.x - 1, startNode.y)
      : GetNode(startNode.x, startNode.y - 1);
  }
  private GridNode GetNextNode(GridNode startNode, DirectionWord directionWord)
  {
    return directionWord == DirectionWord.Horizontal
      ? GetNode(startNode.x + 1, startNode.y)
      : GetNode(startNode.x, startNode.y + 1);
  }


  // public bool CheckNodesRightHorizontal(string word, GridNode startNode)
  // {
  //   bool result = false;

  //   int x = startNode.x;
  //   int y = startNode.y;

  //   foreach (char c in word)
  //   {
  //     x++;
  //     GridNode node = GetNode(x, y);

  //     if (
  //       node.RightNode != null
  //       && (node.TopNode == null || (node.TopNode != null && node.TopNode.StateNode.HasFlag(StateNode.Empty)))
  //       && (node.BottomNode == null || (node.BottomNode != null && node.BottomNode.StateNode.HasFlag(StateNode.Empty)))
  //       && (
  //           node.StateNode.HasFlag(StateNode.Empty)
  //           || (
  //               node.BottomNode.StateNode.HasFlag(StateNode.Occupied)
  //               && node.BottomNode.OccupiedChar.CharValue == c.ToString()
  //             )
  //           )
  //     )
  //     {
  //       result = true;
  //     }
  //     else
  //     {
  //       return false;
  //     }
  //   }

  //   return result;
  // }

  // public GridNode CheckNodeForChar(string word, GridNode startNode, DirectionWord directionWord)
  // {
  //   GridNode result = null;


  //   return result;
  // }
}


public enum DirectionWord
{
  Horizontal = 1,
  Vertical = 2
}

public struct WordItemStartNode
{
  public GridNode node;
  public DirectionWord directionWord;
}