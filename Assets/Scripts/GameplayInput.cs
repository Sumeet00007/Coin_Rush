using Fusion;
using UnityEngine;

public enum PlayerInputButton
{
    Jump
}

public struct GameplayInput : INetworkInput
{
    public Vector2 MoveDirection;
    public NetworkButtons Buttons;
}