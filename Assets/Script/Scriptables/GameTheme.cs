using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

[CreateAssetMenu]
public class GameTheme : ScriptableObject
{
  [Space(5)]
  [Header("Hidden Word")]
  public Color bgHiddenWord;
  public Color bgOpentHiddenWord;
  public Color textOpentHiddenWord;
  public Sprite bgImageHiddenWord;
  public Color bgOpenNeiHiddenWord;
  public Color textOpenNeiHiddenWord;


  [Space(5)]
  [Header("Choosed Word")]
  public Color bgChoosedWord;
  public Color textChoosedWord;
  public Color bgFindAllowWord;
  public Color textFindAllowWord;
  public Color bgFindHiddenWord;
  public Color textFindHiddenWord;
  public Color bgNotFoundWord;
  public Color textNotFoundWord;


  [Space(5)]
  [Header("Chars")]
  public Color colorTextChar;
  public Color bgColorChar;
  public Color colorTextChooseChar;
  public Color bgColorChooseChar;
  public Sprite bgImageChar;
  public Sprite bgImageCharChoose;

  [Space(5)]
  [Header("Game")]
  public Color bgColor;
  public Color colorBgGrid;
  public Color colorLine;
  public Color colorPrimary;
  public Color colorSecondary;
  public Color colorAccent;


  [Space(5)]
  [Header("Entity-Bonus")]
  public Color entityColor;
  // public Color entityActiveColor;
  public Color colorDisable;
  public Color entityColorAsBonus;

  [Space(5)]
  [Header("Hint")]
  public Color colorBgHint;
  public Color colorTextHint;

  [Space(5)]
  [Header("Button")]
  public Color colorBgButton;

  [Space(5)]
  [Header("Form")]
  public Color colorBgInput;
  public Color colorTextInput;
}
