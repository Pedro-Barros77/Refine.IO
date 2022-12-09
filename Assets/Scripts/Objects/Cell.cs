using UnityEngine;
using UnityEngine.Tilemaps;

public class Cell
{
    public WorldBuilder.CellType[] CellTypes = new WorldBuilder.CellType[4];
    public TileBase[] Tiles = new TileBase[4];
    public Cell(WorldBuilder.CellType cellType, TileBase tile)
    {
        this.CellTypes[0] = cellType;
        this.Tiles[0] = tile;
    }
}
