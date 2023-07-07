using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineManager : MonoBehaviour
{
  private GameSetting _gameSetting => GameManager.Instance.GameSettings;
  private GameManager _gameManager => GameManager.Instance;
  [SerializeField] private LineRenderer _lineRenderer;
  [SerializeField] private Material _material;
  private List<Vector3> listPoints = new List<Vector3>();

  private void Awake()
  {
    ChangeTheme();

    GameManager.OnChangeTheme += ChangeTheme;
  }

  private void OnDestroy()
  {
    GameManager.OnChangeTheme -= ChangeTheme;
  }

  private void ChangeTheme()
  {
    _lineRenderer.startColor = _gameManager.Theme.colorLine;
    _lineRenderer.endColor = _gameManager.Theme.colorLine;
  }

  public void ResetLine()
  {
    listPoints.Clear();
    // _lineRenderer.SetPositions(new Vector3[0]);
    DrawLine();
  }

  public void DrawLine()
  {
    _lineRenderer.startWidth = _gameSetting.lineWidth;
    _lineRenderer.endWidth = _gameSetting.lineWidth;

    //get smoothed values
    Vector3[] smoothedPoints = LineSmoother.SmoothLine(listPoints.ToArray(), 0.15f);

    _lineRenderer.positionCount = smoothedPoints.Length;
    _lineRenderer.SetPositions(smoothedPoints);
  }

  public void DrawLine(Vector3 lastPoint)
  {
    _lineRenderer.startWidth = _gameSetting.lineWidth;
    _lineRenderer.endWidth = _gameSetting.lineWidth;

    var listWithLastPoint = listPoints.ToList();
    listWithLastPoint.Add(lastPoint);

    //get smoothed values
    Vector3[] smoothedPoints = LineSmoother.SmoothLine(listWithLastPoint.ToArray(), 0.15f);

    _lineRenderer.positionCount = smoothedPoints.Length;

    _lineRenderer.SetPositions(smoothedPoints);
  }

  public void AddPoint(Vector3 position)
  {
    listPoints.Add(position);
  }

  public void RemovePoint(Vector3 position)
  {
    listPoints.Remove(position);
  }
}
