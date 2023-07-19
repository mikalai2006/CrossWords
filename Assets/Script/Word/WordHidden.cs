using Cysharp.Threading.Tasks;

public class WordHidden : BaseWord
{

  // public async UniTask ShowWord()
  // {
  //   foreach (var charObj in _chars)
  //   {
  //     await charObj.Open(false);
  //     // OpenNeighbours(charObj).Forget();
  //   }
  // }

  public override void Draw()
  {
    foreach (CharHidden charHidden in _chars)
    {
      // var newChar = GameObject.Instantiate(
      //   charMB,
      //   Vector3.zero,
      //   Quaternion.identity,
      //   gameObject.transform
      // );
      // newChar.transform.localPosition = new Vector3(i + .5f, 0 + .5f);
      // // newChar.transform.localPosition = new Vector3(i * _size, 0, 0);
      // var currentChar = word.ElementAt(i);
      // // var currentCharMB = Chars.ElementAt(i);
      // newChar.gameObject.SetActive(true);
      // newChar.SetChar(currentChar);
      // HiddenWord.Chars.Add(newChar);

      // Draw hidden char MonoBehaviour
      charHidden.Draw().Forget();
    }
  }

  // public override void Open(bool runEffect)
  // {
  //   // open all chars.
  //   foreach (CharHidden charHidden in _chars)
  //   {
  //     // Draw hidden char MonoBehaviour
  //     charHidden.Open(runEffect).Forget();
  //   }

  //   // open crosswords if exists.
  //   foreach (var crossWordItem in Crosswords)
  //   {
  //     WordHidden crossWord = (WordHidden)crossWordItem.Key;
  //     bool isAlreadyOpenCrossWord = crossWordItem.Value;

  //     if (!isAlreadyOpenCrossWord && crossWord.isOpen)
  //     {
  //       crossWord.AutoOpenWord().Forget();
  //     }
  //   }
  // }

  public void AddChar(CharHidden newChar, GridNode node)
  {
    _chars.Add(newChar);
    // node.SetOccupiedChar(newChar, this);
  }


}
