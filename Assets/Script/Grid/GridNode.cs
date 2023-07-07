using UnityEngine;
[System.Flags]
public enum StateNode
{
  Disable = 1 << 0,
  Empty = 1 << 1,
  Occupied = 1 << 2,
  Open = 1 << 3,
  Entity = 1 << 4,
  Char = 1 << 5,
  Word = 1 << 6,
  Hint = 1 << 7,
  Bonus = 1 << 8,
}


public class GridNode
{
  public int x;
  public int y;
  public StateNode StateNode = StateNode.Empty;
  GridHelper _gridHelper;
  Grid<GridNode> _grid;
  public Vector2Int arrKey => new Vector2Int(x, y);
  public Vector2 position;
  public int countVacantRight;

  public HiddenCharMB OccupiedChar;
  public HiddenWordMB OccupiedWord;
  public BaseEntity OccupiedEntity;
  public BaseEntity BonusEntity;

  public GridNode TopNode => _gridHelper.GetNode(x, y + 1);
  public GridNode BottomNode => _gridHelper.GetNode(x, y - 1);
  public GridNode LeftNode => _gridHelper.GetNode(x - 1, y);
  public GridNode RightNode => _gridHelper.GetNode(x + 1, y);
  // public bool isAllowEntity => StateNode.HasFlag(StateNode.Occupied)
  //   && !StateNode.HasFlag(StateNode.Open)
  //   && !StateNode.HasFlag(StateNode.Entity);

  public GridNode(Grid<GridNode> grid, GridHelper gridHelper, int x, int y)
  {
    _grid = grid;
    _gridHelper = gridHelper;
    position = new Vector2(x * _grid.cellSize, y * _grid.cellSize);
    countVacantRight = _grid.GetWidth() - x;
    this.x = x;
    this.y = y;
  }

  public void SetOccupiedChar(HiddenCharMB _occupiedChar, HiddenWordMB _word)
  {
    OccupiedChar = _occupiedChar;
    OccupiedWord = _word;
    StateNode |= StateNode.Occupied;
    _occupiedChar.OccupiedNode = this;
  }
  public void SetOpen()
  {
    StateNode |= StateNode.Open;

    // if (StateNode.HasFlag(StateNode.Hint))
    // {
    //   StateNode &= ~StateNode.Hint;
    // }
  }

  public GridNode SetOccupiedEntity(BaseEntity _entity)
  {
    OccupiedEntity = _entity;
    if (_entity == null)
    {
      StateNode &= ~StateNode.Entity;
    }
    else
    {
      StateNode |= StateNode.Entity;
    }
    return this;
  }

  public GridNode SetBonusEntity(BaseEntity _entity)
  {
    BonusEntity = _entity;
    if (_entity == null)
    {
      StateNode &= ~StateNode.Bonus;
    }
    else
    {
      StateNode |= StateNode.Bonus;
    }
    return this;
  }

  public void SetHint(bool status = true)
  {
    if (status)
    {
      StateNode |= StateNode.Hint;
    }
    else
    {
      StateNode &= ~StateNode.Hint;
    }
  }


#if UNITY_EDITOR
  public override string ToString()
  {
    return "GridNode:::" +
        "[x" + x + ",y" + y + "] \n" +
        "[arrKey " + arrKey + "] \n" +
        "[position " + position + "] \n" +
        "OccupiedUnit=" + OccupiedChar?.ToString() + ",\n" +
        "GuestedUnit=" + OccupiedWord?.ToString() + ",\n" +
        "Entity=" + OccupiedEntity?.ToString() + ",\n" +
        "StateNode=" + StateNode + ",\n" +
        "StateNode=" + System.Convert.ToString((int)StateNode, 2) + ",\n";
  }
#endif
}