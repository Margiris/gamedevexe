using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


/// <summary>
/// Map array values
/// 0 - floor (none)
/// 1 - wall (undestroyable)
/// 
/// 
/// </summary>

public class InitiateMap : MonoBehaviour {

    public Grid grid;
    public Tilemap wallmap;

    public Vector2Int MapSize;
    public Vector2Int CoordsOffset;
    private int[,] Map;
    public enum MapTile
    {
        floor,  // 0
        wall    // 1
    }

	void Start () {
        wallmap.CompressBounds();
        MapSize = new Vector2Int(wallmap.size.x, wallmap.size.y);
        CoordsOffset = new Vector2Int(wallmap.origin.x, wallmap.origin.y);
        int[,] Map = new int[MapSize.x, MapSize.y];
        for (int i = 0; i < MapSize.x; i++)
        {
            for (int j = 0; j < MapSize.y; j++)
            {
                if (wallmap.HasTile(new Vector3Int(i+ CoordsOffset.x, j+ CoordsOffset.y, 0)))
                {
                    Map[i, j] = 1;
                }
            }
        }
        //FOR DEBUGING
        //DebugArray(Map);
    }


    public int GetMap(int x, int y)
    {
        return Map[x, y];
    }

    public void SetMap(int x, int y, MapTile value)
    {
        Map[x, y] = (int) value;
    }

    /// <summary>
    /// Gets World to Grid form origin position
    /// </summary>
    /// <param name="position">World position on map</param>
    /// <returns>GRid based position of object</returns>
    public Vector2Int GetCoordinates(Vector3 position)        //   ////////////////////////////////// BUG: if position is out of range error is thrown needs fixing
    {
        Vector3Int pos = grid.WorldToCell(position);
        return new Vector2Int(pos.x + CoordsOffset.x, pos.y + CoordsOffset.y);
    }

    ///FOR DEBUGING
    void DebugArray(int[,] Map)
    {
        string arrayDebug = "";
        for (int j = Map.GetLength(1) - 1; j >= 0; j--)
        {
            for (int i = 0; i < Map.GetLength(0); i++)
            {
                arrayDebug += Map[i, j].ToString();
            }
            arrayDebug += "\n";
        }
        Debug.Log(arrayDebug);
    }
    /////////////////////////
}
