using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Message : MonoBehaviour
{
  [SerializeField] protected TMPro.TextMeshProUGUI textMessage;

  public void Pulse()
  {
    textMessage.DOFade(0f, 1f);
  }
}
