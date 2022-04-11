using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TileType
{
    Empty = 0,
    DesertPlains,
    DesertBush,
    DesertForest,
    TemperatePlains,
    TemperateBush,
    TemperateForest,
    SnowPlains,
    SnowBush,
    SnowForest,
    Blue
}

public class HexagonalWorldGeneration : MonoBehaviour
{

    int mapXSize = 50;
    int mapZSize = 50;
    float terrainNoiseFrequency = 40;
    float terrainNoiseAmplitude = 1.2f;

    float vegatationNoiseFrequency = 10;
    float vegatationNoiseAmplitude = 1.2f;

    float waterNoiseFrequency = 10;
    float waterNoiseAmplitude = 2.2f;

    float xDistance = 1.732f;

    public GameObject[] tiles;

    public GameObject mesh;

    // Start is called before the first frame update
    void Start()
    {
        //Initializing the 2d array with map width and height
        TileType[,] grid = new TileType[mapXSize + 1, mapZSize + 1];
        HexCell[,] cells = new HexCell[mapXSize + 1, mapZSize + 1];

        //Filling the 2d enum array with noise
        GenerateWorld(grid);

        //Instantiating the tiles from the 2d tiletype array
        CreateTilesFromArray(grid, cells);

        //Combine the tiles for batching
        StaticBatchingUtility.Combine(gameObject);              
   }

    //This function uses noise to set the tiletypes in the 2darray
    public void GenerateWorld(TileType[,] grid)
    {
        //Make the Seed
        int temperatureSeed = Random.Range(0, 100);
        int heavyVegatationSeed = Random.Range(0, 100);
        int lightVegatationSeed = Random.Range(0, 100);
        int waterSeed = Random.Range(0, 100);

        for (int z = 0; z <= mapZSize; z++)
        {
            for (int x = 0; x <= mapXSize; x++)
            {
                //Single layered noise
                //Temperature Noise
                float xTemperatureNoiseCord = ((float)x / terrainNoiseFrequency) * terrainNoiseAmplitude + temperatureSeed;
                float zTemperatureNoiseCord = ((float)z / terrainNoiseFrequency) * terrainNoiseAmplitude + temperatureSeed;
                float temperatureNoise = Mathf.PerlinNoise(xTemperatureNoiseCord, zTemperatureNoiseCord);

                if (temperatureNoise >= 0.6f)
                {
                    FillGrid(grid, TileType.DesertPlains, x, z);
                }
                else if (temperatureNoise > 0.3f && temperatureNoise < 0.6f)
                {
                    FillGrid(grid, TileType.TemperatePlains, x, z);
                }
                else if (temperatureNoise <= 0.3f)
                {
                    FillGrid(grid, TileType.SnowPlains, x, z);
                }

                //Heavy Vegatation
                float xVegatationNoiseCord = ((float)x / vegatationNoiseFrequency) * vegatationNoiseAmplitude + heavyVegatationSeed;
                float zVegatationNoiseCord = ((float)z / vegatationNoiseFrequency) * vegatationNoiseAmplitude + heavyVegatationSeed;
                float vegatationNoise = Mathf.PerlinNoise(xVegatationNoiseCord, zVegatationNoiseCord);

                //Temperate
                if(grid[x,z] == TileType.TemperatePlains)
                {
                    if (vegatationNoise >= 0.6f)
                    {
                        FillGrid(grid, TileType.TemperateForest, x, z);
                    }
                }

                //Desert
                if(grid[x, z] == TileType.DesertPlains && vegatationNoise >= 0.6f)
                {
                    FillGrid(grid, TileType.DesertForest, x, z);
                }
                
                //Snow
                if (grid[x, z] == TileType.SnowPlains && vegatationNoise >= 0.6f)
                {
                    FillGrid(grid, TileType.SnowForest, x, z);
                }
                

                //Light Vegatation
                float xBushNoiseCord = ((float)x / vegatationNoiseFrequency) * vegatationNoiseAmplitude + lightVegatationSeed;
                float zBushNoiseCord = ((float)z / vegatationNoiseFrequency) * vegatationNoiseAmplitude + lightVegatationSeed;
                float bushNoise = Mathf.PerlinNoise(xBushNoiseCord, zBushNoiseCord);

                //Temperate
                if (grid[x, z] == TileType.TemperatePlains)
                {
                    if (bushNoise >= 0.6f)
                    {
                        FillGrid(grid, TileType.TemperateBush, x, z);
                    }
                }


                //Desert
                if (grid[x, z] == TileType.DesertPlains && bushNoise >= 0.6f)
                {
                    FillGrid(grid, TileType.DesertBush, x, z);
                }

                //Snow
                if (grid[x, z] == TileType.SnowPlains && bushNoise >= 0.6f)
                {
                    FillGrid(grid, TileType.SnowBush, x, z);
                }
                
                //Water
                /*
                float xWaterNoiseCord = ((float)x / waterNoiseFrequency) * waterNoiseAmplitude + waterSeed;
                float zWaterNoiseCord = ((float)z / waterNoiseFrequency) * waterNoiseAmplitude + waterSeed;
                float waterNoise = Mathf.PerlinNoise(xWaterNoiseCord, zWaterNoiseCord);

                if (waterNoise >= 0.7f)
                {
                    FillGrid(grid, TileType.Blue, x, z);
                }
                */
            }
        }
    }

    //This function sets the tiletype of a cell in the 2DArray 
    private void FillGrid(TileType[,] grid, TileType fillType, int x, int z)
    {
        grid[x, z] = fillType;
    }


    private void CreateTilesFromArray(TileType[,] grid, HexCell[,] cells)
    {
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        for (int z = 0; z < width; z++)
        {
            for (int x = 0; x < height; x++)
            {
                TileType tile = grid[x, z];
                if (tile != TileType.Empty)
                {
                    CreateTile(x, z, tile, grid, cells);
                }
            }
        }

        //Filling neighbours and filling gaps 
        for (int z = 0; z < width; z++)
        {
            for (int x = 0; x < height; x++)
            {
                //This function fills the neighbour array of a tile
                FillNeighboursArray(grid, x, z, cells[x, z], cells);
                //This function fills the gaps between tiles to be used for color blending
                FillGaps(cells[x, z], x, z);
            }
        }

        //Making tile objects denser/less denser
        MakeDenser(cells);

    }

    //This function instantiates a tile
    private void CreateTile(int x, int z, TileType type, TileType[,] grid, HexCell[,] cells)
    {
        int tileID = ((int)type) - 1;
        if (tileID >= 0 && tileID < tiles.Length)
        {
            GameObject tilePrefab = tiles[tileID];
            if (tilePrefab != null)
            {
                //Random Hex Rotation
                int rotIndex = Random.Range(0, 6);
                Quaternion instantiateRot = Quaternion.Euler(-90, 0, rotIndex * 60);

                //Hex pos different for even or oneven
                Vector3 instantiatePos = Vector3.zero;
                if (z % 2 == 0)
                {
                    instantiatePos = new Vector3(x * xDistance + xDistance / 2 + x * 0.5f, 0, z * 1.5f + z * 0.48f);
                }
                else
                {
                    instantiatePos = new Vector3(x * xDistance + x * 0.5f - 0.25f, 0, z * 1.5f + z * 0.48f);
                }
                //Instantiating tile
                GameObject newTile = Instantiate(tilePrefab, instantiatePos, instantiateRot);
                newTile.transform.SetParent(transform);
                
                //Making Temperate Tiles Random
                if (newTile.tag == "TemperateForest" || newTile.tag == "SnowBush")
                {
                    int toDisableObjects = Random.Range(3, 8);

                    for (int i = 0; i < toDisableObjects; i++)
                    {
                        DisableActiveChild(newTile);
                    }
                }
                else if (newTile.tag == "TemperateBush" || newTile.tag == "DesertBush" || newTile.tag == "SnowForest")
                {
                    int toDisableObjects = Random.Range(2, 6);

                    for (int i = 0; i < toDisableObjects; i++)
                    {
                        DisableActiveChild(newTile);
                    }
                }
                else if (newTile.tag == "DesertForest")
                {
                    int toDisableObjects = Random.Range(1, 3);

                    for (int i = 0; i < toDisableObjects; i++)
                    {
                        DisableActiveChild(newTile);
                    }
                }
                

                //Fill the 2d array with the tile gameobjects
                HexCell hexCell = newTile.GetComponent<HexCell>();
                cells[x,z] = hexCell;
            }
        }
    }

    private void DisableActiveChild(GameObject tile)
    {
        int totalObjects = tile.transform.childCount;
        int toDisableObject = Random.Range(0, totalObjects);

        if (tile.transform.GetChild(toDisableObject).gameObject.active)
        {
            tile.transform.GetChild(toDisableObject).gameObject.SetActive(false);
        }
        else
        {
            DisableActiveChild(tile);
        }
    }

    private void FillGaps(HexCell cell, int x, int z)
    {
        //East Quad
        if (x != mapXSize)
        {
            FillMesh fill = Instantiate(mesh, transform).GetComponent<FillMesh>();
            fill.AddQuad(
                new Vector3(cell.transform.position.x + 0.86f, 0.00f, cell.transform.position.z - 0.5f),
                new Vector3(cell.transform.position.x + 0.86f, 0.00f, cell.transform.position.z + 0.5f),
                new Vector3(cell.transform.position.x + 0.86f + 0.51f, 0.00f, cell.transform.position.z + 0.5f),
                new Vector3(cell.transform.position.x + 0.86f + 0.51f, 0.00f, cell.transform.position.z - 0.5f));
            fill.AddQuadColor(
                cell.GetComponent<Renderer>().material.color, 
                cell.GetComponent<Renderer>().material.color, 
                cell.neighbors[1].GetComponent<Renderer>().material.color,
                cell.neighbors[1].GetComponent<Renderer>().material.color);       
        }

        //North East Quad
        if (z != mapZSize && x != mapXSize || z % 2 == 1 && z != mapZSize)
        {
            FillMesh fill = Instantiate(mesh, transform).GetComponent<FillMesh>();
            fill.AddQuad(
                new Vector3(cell.transform.position.x + 0.86f, 0.00f, cell.transform.position.z + 0.5f),
                new Vector3(cell.transform.position.x + 0.0f, 0.00f, cell.transform.position.z + 1f),
                new Vector3(cell.transform.position.x + 0.25f, 0.00f, cell.transform.position.z + 1.48f ),
                new Vector3(cell.transform.position.x + 1.1165f, 0.00f, cell.transform.position.z + 0.98f));
            fill.AddQuadColor(
                cell.GetComponent<Renderer>().material.color,
                cell.GetComponent<Renderer>().material.color,
                cell.neighbors[0].GetComponent<Renderer>().material.color,
                cell.neighbors[0].GetComponent<Renderer>().material.color);
        }

        //South East Quad
        if (z != 0 && x != mapXSize || z % 2 == 1 && x == mapXSize)
        {
            FillMesh fill = Instantiate(mesh, transform).GetComponent<FillMesh>();
            fill.AddQuad(
                new Vector3(cell.transform.position.x + 0.0f, 0.00f, cell.transform.position.z - 1f),
                new Vector3(cell.transform.position.x + 0.86f, 0.00f, cell.transform.position.z - 0.5f),
                new Vector3(cell.transform.position.x + 1.1165f, 0.00f, cell.transform.position.z - 0.98f),
                new Vector3(cell.transform.position.x + 0.25f, 0.00f, cell.transform.position.z - 1.48f));
            fill.AddQuadColor(
                cell.GetComponent<Renderer>().material.color,
                cell.GetComponent<Renderer>().material.color,
                cell.neighbors[2].GetComponent<Renderer>().material.color,
                cell.neighbors[2].GetComponent<Renderer>().material.color);
        }
        
        //North triangle
        if (z != mapZSize && x != mapXSize && x != 0 || z % 2 == 0 && x == 0 && z!= mapZSize|| z % 2 == 1 && x == mapXSize && z != mapZSize)
        {
            FillMesh fill = Instantiate(mesh, transform).GetComponent<FillMesh>();
            fill.AddTriangle(
                new Vector3(cell.transform.position.x + 0.0f, 0.00f, cell.transform.position.z + 1f),
                new Vector3(cell.transform.position.x - 0.26f, 0.00f, cell.transform.position.z + 1.48f),
                new Vector3(cell.transform.position.x + 0.25f, 0.00f, cell.transform.position.z + 1.48f));
            fill.AddTriangleColor(
                cell.GetComponent<Renderer>().material.color,
                cell.neighbors[5].GetComponent<Renderer>().material.color,
                cell.neighbors[0].GetComponent<Renderer>().material.color);
        }

        //North East triangle
        if (x != mapXSize && z != mapZSize)
        {
            FillMesh fill = Instantiate(mesh, transform).GetComponent<FillMesh>();
            fill.AddTriangle(
                new Vector3(cell.transform.position.x + 0.86f, 0.00f, cell.transform.position.z + 0.5f),
                new Vector3(cell.transform.position.x + 1.1165f, 0.00f, cell.transform.position.z + 0.98f),
                new Vector3(cell.transform.position.x + 0.86f + 0.51f, 0.00f, cell.transform.position.z + 0.5f));
            fill.AddTriangleColor(
                cell.GetComponent<Renderer>().material.color,
                cell.neighbors[0].GetComponent<Renderer>().material.color,
                cell.neighbors[1].GetComponent<Renderer>().material.color);
        }

    }

    private void FillNeighboursArray(TileType[,] grid, int x, int z, HexCell cell, HexCell[,] cells)
    {
        if (z % 2 == 0)
        {            
            if (z != 0)
            {
                cell.SetNeighbor(HexDirection.SW, cells[x, z - 1]);
            }           
            if (x != 0)
            {
                cell.SetNeighbor(HexDirection.W, cells[x - 1, z]);
            }
            if (z != mapZSize)
            {
                cell.SetNeighbor(HexDirection.NW, cells[x, z + 1]);
            }
            if (z != mapZSize && x != mapXSize)
            {
                cell.SetNeighbor(HexDirection.NE, cells[x + 1, z + 1]);
            }
            if (x != mapXSize)
            {
                cell.SetNeighbor(HexDirection.E, cells[x + 1, z]);
            }
            if (x != mapXSize && z != 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[x + 1, z - 1]);
            }
        }        
        else
        {
            if (z != 0 && x != 0)
            {
                cell.SetNeighbor(HexDirection.SW, cells[x - 1, z - 1]);
            }
            if (x != 0)
            {
                cell.SetNeighbor(HexDirection.W, cells[x - 1, z]);
            }
            if (x != 0 && z != mapZSize)
            {
                cell.SetNeighbor(HexDirection.NW, cells[x - 1, z + 1]);
            }
            if (z != mapZSize)
            {
                cell.SetNeighbor(HexDirection.NE, cells[x, z + 1]);
            }
            if (x != mapXSize)
            {
                cell.SetNeighbor(HexDirection.E, cells[x + 1, z]);
            }
            if (z != 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[x, z - 1]);
            }                      
        }            
    }

    private void MakeDenser(HexCell[,] cells)
    {
        foreach (HexCell cell in cells)
        {
            int amountOfSameNeighbours = 0;
            for (int i = 0; i < cell.neighbors.Length; i++)
            {
                if (cell.neighbors[i] != null && cell.neighbors[i].tag == cell.tag)
                {
                    amountOfSameNeighbours++;
                }
            }
            if (cell.tag == "TemperateForest" || cell.tag == "SnowBush")
            {
                if (amountOfSameNeighbours > 4)
                {
                    EnableInActiveChild(cell.gameObject);
                    EnableInActiveChild(cell.gameObject);
                    EnableInActiveChild(cell.gameObject);
                }
                else if (amountOfSameNeighbours < 5)
                {
                    DisableActiveChild(cell.gameObject);
                    DisableActiveChild(cell.gameObject);
                    DisableActiveChild(cell.gameObject);
                }
            }
            else if (cell.tag == "TemperateBush" || cell.tag == "SnowForest" || cell.tag == "DesertBush")
            {
                if (amountOfSameNeighbours > 4)
                {
                    EnableInActiveChild(cell.gameObject);
                }
                else if (amountOfSameNeighbours < 5)
                {
                    DisableActiveChild(cell.gameObject);
                    DisableActiveChild(cell.gameObject);
                }
            }
            else if (cell.tag == "DesertForest")
            {
                if (amountOfSameNeighbours > 4)
                {
                    EnableInActiveChild(cell.gameObject);
                }
                else if (amountOfSameNeighbours < 5)
                {
                    DisableActiveChild(cell.gameObject);
                }
            }
        }
    }

    private void EnableInActiveChild(GameObject tile)
    {
        int totalObjects = tile.transform.childCount;
        int toEnableObject = Random.Range(0, totalObjects);

        if (tile.transform.GetChild(toEnableObject).gameObject.active == false)
        {
            tile.transform.GetChild(toEnableObject).gameObject.SetActive(true);
            Debug.Log("Enabled" + tile.transform.GetChild(toEnableObject).gameObject);
        }
        else
        {
            EnableInActiveChild(tile);
        }
    }
}
