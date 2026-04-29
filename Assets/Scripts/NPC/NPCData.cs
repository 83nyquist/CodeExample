using Unity.Mathematics;

public struct NPCData
{
    public int2 Position;
    public int2 PreviousPosition;
    public float Timer;
    public int Id;
    public bool IsVisible;
    public bool IsMoving;
}