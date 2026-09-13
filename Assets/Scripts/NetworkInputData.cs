using Fusion;
using UnityEngine;

public enum PlayerInputButton
{
    Jump
}

public struct NetworkInputData : INetworkInput
{
    public Vector2 MoveInput;
    public NetworkButtons Buttons;
}