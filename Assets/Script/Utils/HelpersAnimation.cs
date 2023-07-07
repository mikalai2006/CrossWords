using DG.Tweening;
using UnityEngine;

public static class HelpersAnimation
{
  public static void Pulse(GameObject gameObject, Vector3 amount, float time)
  {
    gameObject.transform
      .DOPunchScale(amount, time)
      .SetEase(Ease.OutBack);
  }

}