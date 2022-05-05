using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TextureData
{
    public static readonly int wNum = 16;
    public static readonly int hNum = 16;
    public static readonly int blockWidth = 16;

    private readonly int front;
    private readonly int back;
    private readonly int left;
    private readonly int right;
    private readonly int top;
    private readonly int bottom;
    private readonly bool isSolid;

    public bool IsSolid
    {
        get
        {
            return this.isSolid;
        }
    }

    /// <summary>
    /// texture map 的對應位置，左上為 0，右下為最大數值
    /// </summary>
    public TextureData(int front, int back, int left, int right, int top, int bottom, bool isSolid = true)
    {
        this.front = front;
        this.back = back;
        this.left = left;
        this.right = right;
        this.top = top;
        this.bottom = bottom;
        this.isSolid = isSolid;
    }

    public int GetTexture(Voxel.Face face)
    {
        switch (face)
        {
            case Voxel.Face.FRONT: return this.front;
            case Voxel.Face.BACK: return this.back;
            case Voxel.Face.LEFT: return this.left;
            case Voxel.Face.RIGHT: return this.right;
            case Voxel.Face.TOP: return this.top;
            case Voxel.Face.BOTTOM: return this.bottom;

            default:
                return 0;
        }
    }
}

public class Air : TextureData
{
    public Air() : base(0, 0, 0, 0, 0, 0, false)
    {
    }
}
public class BedRock : TextureData
{
    public BedRock() : base(17, 17, 17, 17, 17, 17)
    {
    }
}
public class Grass : TextureData
{
    public Grass() : base(3, 3, 3, 3, 0, 2)
    {
    }
}
public class Stone : TextureData
{
    public Stone() : base(1, 1, 1, 1, 1, 1)
    {
    }
}