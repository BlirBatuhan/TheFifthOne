using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 move;
    public bool jump;
    public bool crouch;
    public bool run;
    public bool attack;
}
