using System;

using UnityEngine;

public class Grid<T>
{

  private readonly int _width;
  private readonly int _height;
  public float cellSize;
  private readonly T[,] _gridArray;

  public int SizeGrid { get { return _width * _height; } private set { } }

  public Grid(int width, int height, float cellSize, GridHelper gridTileHelper, Func<Grid<T>, GridHelper, int, int, T> createValue)
  {
    _width = width;
    _height = height;
    this.cellSize = cellSize;

    _gridArray = new T[width, height];

    for (int x = 0; x < _gridArray.GetLength(0); x++)
    {
      for (int y = 0; y < _gridArray.GetLength(1); y++)
      {
        _gridArray[x, y] = createValue(this, gridTileHelper, x, y);
      }
    }
  }

  public void SetValue(int x, int y, T value)
  {
    _gridArray[x, y] = value;
  }

  public T[,] GetGrid()
  {
    return _gridArray;
  }
  public T GetGridObject(Vector3Int pos)
  {
    //Debug.Log($"GetGrid {x},{z}: {GetWorldPosition(x, z)}");
    return pos.x >= 0 && pos.y >= 0 && pos.x < _width && pos.y < _height ? _gridArray[pos.x, pos.y] : default;
  }

  public int GetHeight()
  {
    return _height;
  }

  public int GetWidth()
  {
    return _width;
  }

}
