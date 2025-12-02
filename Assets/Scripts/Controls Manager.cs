using UnityEngine;
using System.Collections.Generic;

public enum PlayerAction
{
    MoveLeft,
    MoveRight,
    ReelUp,
    ReelDown,
    Jump,
    Slide,
    Shrink,
    Interact,
    Torch,
    Grapple
}

public static class ControlsManager
{
    private static Dictionary<PlayerAction, KeyCode> keyBindings = new Dictionary<PlayerAction, KeyCode>();
    private static Dictionary<PlayerAction, KeyCode> defaultBindings = new Dictionary<PlayerAction, KeyCode>();

    static ControlsManager()
    {
        // Initialize defaults
        defaultBindings[PlayerAction.MoveLeft] = KeyCode.A;
        defaultBindings[PlayerAction.MoveRight] = KeyCode.D;
        defaultBindings[PlayerAction.ReelUp] = KeyCode.W;
        defaultBindings[PlayerAction.ReelDown] = KeyCode.S;
        defaultBindings[PlayerAction.Jump] = KeyCode.Space;
        defaultBindings[PlayerAction.Slide] = KeyCode.LeftControl;
        defaultBindings[PlayerAction.Shrink] = KeyCode.LeftShift;
        defaultBindings[PlayerAction.Interact] = KeyCode.E;
        defaultBindings[PlayerAction.Torch] = KeyCode.F;
        defaultBindings[PlayerAction.Grapple] = KeyCode.Mouse0;

        foreach (var kv in defaultBindings)
            keyBindings[kv.Key] = kv.Value;

        LoadKeys(); // Load saved keys
    }

    public static KeyCode GetKey(PlayerAction action) => keyBindings[action];

    public static void SetKey(PlayerAction action, KeyCode key)
    {
        keyBindings[action] = key;
        PlayerPrefs.SetInt(action.ToString(), (int)key);
        PlayerPrefs.Save();
    }

    public static void LoadKeys()
    {
        foreach (PlayerAction action in System.Enum.GetValues(typeof(PlayerAction)))
        {
            if (PlayerPrefs.HasKey(action.ToString()))
            {
                keyBindings[action] = (KeyCode)PlayerPrefs.GetInt(action.ToString());
            }
        }
    }

    public static void ResetToDefaults()
    {
        foreach (var kv in defaultBindings)
        {
            keyBindings[kv.Key] = kv.Value;
            PlayerPrefs.SetInt(kv.Key.ToString(), (int)kv.Value);
        }
        PlayerPrefs.Save();
    }
}
