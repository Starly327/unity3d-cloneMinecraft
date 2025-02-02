using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;


public class Chunk
{
    public static readonly byte width = 16, height = 16, airHeight = 64;
    private readonly Terrain terrain;
    private readonly ChunkCoord coord;
    
    GameObject gameObj;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshColider;

    int vertexBase;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    byte[,,] map = new byte[width, height + airHeight, width];

    public bool IsActive
    {
        get { return this.gameObj.activeSelf; }
        set { this.gameObj.SetActive(value); }
    }

    VoxelTextureMap voxelTextureMap{
        get{
            return VoxelTextureMap.getVoxelTextureMap();
        }
    }

    public Vector3 Position
    {
        get { return this.gameObj.transform.position; }
    }

    public Chunk(Terrain terrain, ChunkCoord coord)
    {
        this.terrain = terrain;
        this.coord = coord;

        this.gameObj = new GameObject();
        this.meshFilter = this.gameObj.AddComponent<MeshFilter>();
        this.meshRenderer = this.gameObj.AddComponent<MeshRenderer>();
        this.meshRenderer.material = voxelTextureMap.material;
        this.meshColider = this.gameObj.AddComponent<MeshCollider>();

        this.gameObj.transform.SetParent(this.terrain.Transform);
        this.gameObj.name = $"chunk {this.coord.x}, {this.coord.z}";
        this.gameObj.transform.position = new Vector3(
            this.coord.x * Chunk.width,
            0,
            this.coord.z * Chunk.width
        );

        this.PopulateChunk();

        this.UpdateChunk();
    }

    public bool HasBlock(Vector3 pos)
    {
        int x = (int) pos.x;
        int y = (int) pos.y;
        int z = (int) pos.z;

        int blockId = -1;
        if (!this.ValidArea(x, y, z))
            blockId = this.terrain.GetVoxelType(pos + this.Position);
        else
            blockId = this.map[x, y, z];


        return voxelTextureMap.GetVoxelInfo(blockId).IsSolid;
    }

    public VoxelInfo GetBlock(Vector3 pos)
    {
        int x = (int) pos.x;
        int y = (int) pos.y;
        int z = (int) pos.z;

        int blockId = -1;
        if (!this.ValidArea(x, y, z))
            blockId = this.terrain.GetVoxelType(pos + this.Position);
        else
            blockId = this.map[x, y, z];

         return voxelTextureMap.GetVoxelInfo(blockId);
    }

    public void EditVoxel(Vector3 pos, byte id)
    {
        int x = (int)pos.x;
        int y = (int)pos.y;
        int z = (int)pos.z;

        this.map[x, y, z] = id;

        this.UpdateChunk();

        //TODO 需要同步確認周遭的 chunk 是否會因為這次的更動需要更新整個 chunk
        this.UpdateNearChunk(x, y, z);
    }

    void UpdateNearChunk(int x, int y, int z)
    {
        var pos = new Vector3(x, y, z);
        foreach (int face in Enum.GetValues(typeof(VoxelInfo.Face)))
        {
            var nearVoxelpos = pos + VoxelInfo.faceDirs[face];
            if(!ValidArea(nearVoxelpos)){
                var nearChunk = this.terrain.GetChunk(nearVoxelpos + this.Position);
                nearChunk.UpdateChunk();
            }
        }
    }

    void PopulateChunk()
    {
        for (int x = 0; x < Chunk.width; x++)
        {
            for (int y = 0; y < Chunk.height + Chunk.airHeight; y++)
            {
                for (int z = 0; z < Chunk.width; z++)
                {
                    this.map[x, y, z] = this.terrain.GetVoxelType(new Vector3(x, y, z) + this.Position);
                }
            }
        }
    }

    public void UpdateChunk()
    {
        this.vertexBase = 0;
        this.vertices.Clear();
        this.uvs.Clear();
        this.triangles.Clear();
        for (int x = 0; x < Chunk.width; x++)
        {
            for (int y = 0; y < height + airHeight; y++)
            {
                for (int z = 0; z < Chunk.width; z++)
                {
                    this.AddVoxelData(new Vector3(x, y, z));
                }
            }
        }

        this.CreateMesh();
    }

    private void AddVoxelData(Vector3 pos)
    {
        int blockId = this.map[(int)pos.x, (int)pos.y, (int)pos.z];
        if(blockId == voxelTextureMap.Air.Id) return;

        var voxelInfo = voxelTextureMap.GetVoxelInfo(blockId);

        foreach (int face in Enum.GetValues(typeof(VoxelInfo.Face)))
        {
            if (HasBlock(pos + VoxelInfo.faceDirs[face])) continue;
            if (pos.y == 0 && face != (int)VoxelInfo.Face.TOP) continue;//最下層的 voxel 只需渲染最上面的方塊即可

            vertices.AddRange(VoxelInfo.faceIndexs[face].Select(x => VoxelInfo.vertices[x] + pos));
            uvs.AddRange(voxelInfo.GetTextureUvs((VoxelInfo.Face)face));

            triangles.AddRange(VoxelInfo.triangleIndex.Select(x => vertexBase + x));

            vertexBase += VoxelInfo.verticesPerFace;
        }
    }

    private void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshColider.sharedMesh = mesh;
    }

    bool ValidArea(Vector3 p)
    {
        return this.ValidArea((int)p.x, (int)p.y, (int)p.z);
    }

    bool ValidArea(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0) return false;
        if (x >= width || y >= height + airHeight || z >= width) return false;

        return true;
    }
}

public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public bool Equals(ChunkCoord coord)
    {
        return x == coord.x &&
               z == coord.z;
    }
}