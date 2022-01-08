using System.Collections.Generic;
using UnityEngine;

public enum Control
{
    RotateClockwise,
    RotateCounterClockwise,
    HardDrop,
    SoftDrop,
    Hold,
    Pause,
    MoveLeft,
    MoveRight
};

public static class GameInput
{
    public static readonly Dictionary<Control, KeyCode[]> KeyCodesForControl
      = new Dictionary<Control, KeyCode[]>
      {
          { Control.RotateClockwise, new KeyCode[] {KeyCode.UpArrow, KeyCode.X, KeyCode.Keypad1,KeyCode.Keypad5,KeyCode.Keypad9}},
          { Control.RotateCounterClockwise, new KeyCode[] {KeyCode.LeftControl, KeyCode.RightControl, KeyCode.Z,KeyCode.Keypad3,KeyCode.Keypad7}},
          { Control.HardDrop, new KeyCode[] {KeyCode.Space,KeyCode.Keypad8}},
          { Control.SoftDrop, new KeyCode[] {KeyCode.DownArrow,KeyCode.Keypad2}},
          { Control.Hold, new KeyCode[] { KeyCode.LeftShift,KeyCode.RightShift,KeyCode.C}},
          { Control.Pause, new KeyCode[] { KeyCode.Escape,KeyCode.F1}},
          { Control.MoveLeft, new KeyCode[] { KeyCode.LeftArrow,KeyCode.Keypad4}},
          { Control.MoveRight, new KeyCode[] { KeyCode.RightArrow,KeyCode.Keypad6}},
      };

    public static bool GetControl(Control control)
    {
        var keyCodes = KeyCodesForControl[control];
        foreach (var keyCode in keyCodes)
        {
            if (Input.GetKey(keyCode)) return true;
        }
        return false;
    }

    public static bool GetControlDown(Control control)
    {
        var keyCodes = KeyCodesForControl[control];
        foreach (var keyCode in keyCodes)
        {
            if (Input.GetKeyDown(keyCode)) return true;
        }
        return false;
    }
}