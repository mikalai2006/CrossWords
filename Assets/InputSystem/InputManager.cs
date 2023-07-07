using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager
{
  //   public delegate void ClickEvent(InputAction.CallbackContext context);
  private static InputSystem _input = new InputSystem();
  public static float _timeDragging = 0.0f;
  public float TimeDragging => _timeDragging;
  private static bool _isDragging;
  public bool Dragging => _isDragging;
  public void SetDragging(bool status)
  {
    _isDragging = status;
  }
  public void SetTimeDragging(float value)
  {
    _timeDragging = value;
  }

  public void Enable()
  {
    _input.InputActions.Enable();
  }

  public void Disable()
  {
    _input.InputActions.Disable();
  }

  public event Action<InputAction.CallbackContext> Click
  {
    add
    {
      _input.InputActions.Click.performed += value;
      _input.InputActions.Click.canceled += value;
    }
    remove
    {
      _input.InputActions.Click.performed -= value;
      _input.InputActions.Click.canceled -= value;
    }
  }
  public event Action<InputAction.CallbackContext> ClickChar
  {
    add
    {
      _input.InputActions.Click.performed += value;
    }
    remove
    {
      _input.InputActions.Click.performed -= value;
    }
  }

  public Vector2 clickPosition()
  {
    return _input.InputActions.Position.ReadValue<Vector2>();
  }

  public bool ClickedOnUi()
  {
    PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
    eventDataCurrentPosition.position = clickPosition();
    List<RaycastResult> results = new List<RaycastResult>();
    EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
    foreach (var item in results)
    {
      if (item.gameObject.name == "Panel Settings")
      {
        return true;
      }
    }
    return false;
  }
}
