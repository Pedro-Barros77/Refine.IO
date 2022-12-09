using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldBuilder : MonoBehaviour
{
    public enum CellType
    {
        Blank = 0,
        Grass = 1,
        DarkGrass = 2,
        Dirt = 3,
        PoligonBrick = 4,
        Water = 5,
        WaterGlass = 6
    }
    public enum EntityType
    {
        Tree = 0,
        Rock = 1
    }

    [SerializeField] Transform fxInstances;
    public static Transform FXInstances;

    [SerializeField] TileBase[] Tiles;

    [SerializeField] private Tilemap map_0, map_01, map_02, map_03;
    [SerializeField] Transform Player;

    [SerializeField] GameObject treePrefab, rockPrefab;

    [SerializeField] private int renderDistance, chunkSize;
    private float scale = 1, xOffset, yOffset;
    [SerializeField]
    private float grassLevel = 0.35f, darkGrassLevel = 0.2f, dirtLevel = 0.35f, waterLevel = 0.57f, treeLevel, rockLevel;

    float startXOffset, startYOffset;

    int ChunkX = 0, ChunkY = 0;
    int lastChunkX = -1, lastChunkY = -1;

    List<int[]> Chunks = new List<int[]>();
    public static List<EntityObject> WorldEntities = new List<EntityObject>();

    Transform treesHolder, rocksHolder;

    void Start()
    {
        //Posição do mundo gerado
        xOffset = Random.Range(-10000f, 10000f);
        yOffset = Random.Range(-10000f, 10000f);
        startXOffset = xOffset;
        startYOffset = yOffset;

        FXInstances = fxInstances;
        treesHolder = transform.parent.Find("Trees");
        rocksHolder = transform.parent.Find("Rocks");
    }

    // Update is called once per frame
    void Update()
    {
        //Armazena a posição do chunk em que o player está
        ChunkX = Mathf.RoundToInt(Player.transform.position.x / chunkSize);
        ChunkY = Mathf.RoundToInt(Player.transform.position.y / chunkSize);

        //Cria uma lista de chunks que devem ser gerados, de acordo com a distância
        for (int x = -(renderDistance / 2); x < (renderDistance / 2) + 1; x++)
        {
            for (int y = -(renderDistance / 2); y < (renderDistance / 2) + 1; y++)
            {
                if (!Chunks.Any(c => c[0] == ChunkX + x && c[1] == ChunkY + y))
                    Chunks.Add(new int[] { ChunkX + x, ChunkY + y });
            }
        }


        //Se o chunk atual for diferente do chunk anterior (player se moveu), carregar novos chunks
        if (lastChunkX != ChunkX || lastChunkY != ChunkY)
        {
            foreach (var chunk in Chunks)
            {
                if (!HasTile(chunk))
                {
                    LoadChunk(chunk[0], chunk[1]);
                }
            }
        }
        lastChunkX = ChunkX;
        lastChunkY = ChunkY;

        //Lista de chunks que estão longe para descarregar
        var farChunks = Chunks.Where(c => Mathf.Abs(ChunkX - c[0]) > renderDistance / 2 || Mathf.Abs(ChunkY - c[1]) > renderDistance / 2);
        foreach (var chunk in farChunks)
        {
            UnloadChunk(chunk[0], chunk[1]);
        }
        Chunks.RemoveAll(c => farChunks.Contains(c));
    }

    void LoadChunk(int chunkX, int chunkY)
    {
        xOffset = startXOffset + chunkX;
        yOffset = startYOffset + chunkY;

        //Plano cartesiano de valores aleatórios
        float[,] noiseMap = new float[chunkSize, chunkSize];
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                float noiseValue = Mathf.Clamp(Mathf.PerlinNoise(x * (scale / 10) + xOffset, y * (scale / 10) + yOffset), 0, 1);
                noiseMap[x, y] = noiseValue;
            }
        }

        //Preenche o mapa com as células, de acordo com os valores do plano cartesiano
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                //Posição da célula a ser pintada
                Vector3Int pos = new Vector3Int(x - (chunkSize / 2) + (chunkX * chunkSize), y - (chunkSize / 2) + (chunkY * chunkSize), 0);

                float noiseValue = noiseMap[x, y];

                //TileMap_01
                CellType cellType = GetCellType(noiseValue, 1);
                Cell cell = new Cell(cellType, Tiles[cellType.GetHashCode()]);
                map_01.SetTile(pos, cell.Tiles[0]);

                //TileMap_02
                cellType = GetCellType(noiseValue, 2);
                cell.CellTypes[1] = cellType;
                cell.Tiles[1] = Tiles[cellType.GetHashCode()];
                map_02.SetTile(pos, cell.Tiles[1]);

                //TileMap_03
                cellType = GetCellType(noiseValue, 3);
                cell.CellTypes[2] = cellType;
                cell.Tiles[2] = Tiles[cellType.GetHashCode()];
                map_03.SetTile(pos, cell.Tiles[2]);

                //TileMap_0 (Camada acima do Player)
                cellType = GetCellType(noiseValue, 4);
                cell.CellTypes[3] = cellType;
                cell.Tiles[3] = Tiles[cellType.GetHashCode()];
                map_0.SetTile(pos, cell.Tiles[3]);


                foreach (var entity in WorldEntities.Where(
                    e => (e.type == EntityType.Tree || e.type == EntityType.Rock) &&
                    Vector3.Distance(pos, e.transform.position) < 3))
                {
                    entity.gameObject.SetActive(true);
                }


                if (noiseValue < treeLevel && !ExistsEntityNearby(pos))
                {
                    GameObject newTree = Instantiate(treePrefab, pos, Quaternion.identity, treesHolder);
                    WorldEntities.Add(newTree.GetComponent<EntityObject>());
                }
                else if (noiseValue > waterLevel - rockLevel && noiseValue < waterLevel - rockLevel / 1.04f && !ExistsEntityNearby(pos))
                {
                    GameObject newRock = Instantiate(rockPrefab, pos, Quaternion.identity, rocksHolder);
                    WorldEntities.Add(newRock.GetComponent<EntityObject>());
                }

            }
        }

        DrawChunks(chunkX, chunkY);
    }

    void UnloadChunk(int chunkX, int chunkY)
    {
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                Vector3Int pos = new Vector3Int(x - (chunkSize / 2) + (chunkX * chunkSize), y - (chunkSize / 2) + (chunkY * chunkSize), 0);
                map_01.SetTile(pos, null);
                map_02.SetTile(pos, null);
                map_03.SetTile(pos, null);
                map_0.SetTile(pos, null);

                foreach (var tree in WorldEntities.Where(
                    t => t.type == EntityType.Tree &&
                    Vector3.Distance(pos, t.transform.position) < 3))
                {
                    tree.gameObject.SetActive(false);
                }
            }
        }
    }

    bool ExistsEntityNearby(Vector3Int pos)
    {
        return WorldEntities.Any(e => Vector3.Distance(pos, e.transform.position) < 5
                     && Vector3.Distance(Player.position, e.transform.position) > 2);
    }

    //Determina que célula será selecionada de acordo com o valor
    CellType GetCellType(float noiseLevel, int targetMap)
    {
        if (noiseLevel >= waterLevel + (waterLevel / 3.5f) && targetMap == 1)
        {
            return CellType.Blank;
        }
        else if (noiseLevel >= waterLevel + (waterLevel / 3.5f) && targetMap == 4)
        {
            return CellType.WaterGlass;
        }
        else if (noiseLevel >= waterLevel && targetMap == 2)
        {
            return CellType.Water;
        }
        else if (noiseLevel >= grassLevel && targetMap == 1)
        {
            return CellType.Grass;
        }
        else if (noiseLevel >= darkGrassLevel && targetMap == 2)
        {
            return CellType.DarkGrass;
        }
        else if (noiseLevel <= dirtLevel && targetMap == 3)
        {
            return CellType.Dirt;
        }
        else
        {
            return CellType.Blank;
        }
    }

    bool HasTile(int[] chunk)
    {
        return
        map_01.GetTile(new Vector3Int(chunk[0] * chunkSize, chunk[1] * chunkSize, 0)) != null ||
        map_02.GetTile(new Vector3Int(chunk[0] * chunkSize, chunk[1] * chunkSize, 0)) != null ||
        map_03.GetTile(new Vector3Int(chunk[0] * chunkSize, chunk[1] * chunkSize, 0)) != null;
    }

    void DrawChunks(int chunkX, int chunkY)
    {
        Vector3 topLeft = new Vector3(chunkX + (chunkSize / 2), chunkY + (chunkSize / 2), 0);
        Vector3 topRight = new Vector3(chunkX * chunkSize + (chunkSize / 2), chunkY + (chunkSize / 2), 0);
        Vector3 bottomRight = new Vector3(chunkX * chunkSize + (chunkSize / 2), chunkY * chunkSize + (chunkSize / 2), 0);
        Vector3 bottomLeft = new Vector3(chunkX + (chunkSize / 2), chunkY * chunkSize + (chunkSize / 2), 0);

        Debug.DrawLine(topLeft, topRight, Color.red, 10, false);
        Debug.DrawLine(topRight, bottomRight, Color.red, 10, false);
        Debug.DrawLine(bottomRight, bottomLeft, Color.red, 10, false);
        Debug.DrawLine(bottomLeft, topLeft, Color.red, 10, false);
    }
}
