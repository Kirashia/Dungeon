using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

    public int roomHeight;
    public int roomWidth;
    public string seed;
    public Vector3 startPos;
    public GameObject filled;
    [Range(0, 100)] public int randomFillPercent;
    public bool isStartRoom;
    public bool isEndRoom;
    public Vector2 entrancePos;
    public Vector2 endPos;

    public GameObject[] walls;

    private int[,] tiles;
    private int[,] actual;
    private bool isStartRoomFlag;
    private bool isEndRoomFlag;
    private HashSet<Vector2> checkedTiles;
    private Dictionary<int, HashSet<Vector2>> reigons;
    private System.Random pseudoRandom;

    public void MakeStartRoom()
    {
        isStartRoom = true;
        isStartRoomFlag = true;
        isEndRoom = false;
    }

    public void MakeEndRoom()
    {
        isEndRoom = true;
        isEndRoomFlag = true;
        isStartRoom = false;
    }

    public void MakeEntranceAndExits(int direction)
    {
        // 1 - entrance and exit on both sides of map
        // 2 - 1 with a bottom and a top exit
        // 3 - 1 with a top exit
        // 4 - 1 with only a bottom exit (unused)

        // 0 - random

        if (direction == 0)
        {
            // Adds a random exit shape to a non-solution-path room
            direction = pseudoRandom.Next(1, 4);
        }

        int midHeight = roomHeight / 2;
        int midWidth = roomWidth / 2;

        switch (direction)
        {
            case 1:
                // Sides
                tiles[0, midHeight] = 0;
                tiles[0, midHeight + 1] = 0;
                tiles[roomWidth - 1, midHeight] = 0;
                tiles[roomWidth - 1, midHeight + 1] = 0;
                break;

            case 2:
                // Sides
                tiles[0, midHeight] = 0;
                tiles[0, midHeight + 1] = 0;
                tiles[roomWidth - 1, midHeight] = 0;
                tiles[roomWidth - 1, midHeight + 1] = 0;

                // Top
                tiles[midWidth, 0] = 0;
                tiles[midWidth + 1, 0] = 0;

                // Bottom
                tiles[midWidth, roomHeight - 1] = 0;
                tiles[midWidth + 1, roomHeight - 1] = 0;

                break;

            case 3:
                // Sides 
                tiles[0, midHeight] = 0;
                tiles[0, midHeight + 1] = 0;
                tiles[roomWidth - 1, midHeight] = 0;
                tiles[roomWidth - 1, midHeight + 1] = 0;

                // Top
                tiles[midWidth, 0] = 0;
                tiles[midWidth + 1, roomHeight - 1] = 0;
                break;

        }
    }

    public void SetupRoom(Vector3 pos, int width, int height, string tempSeed, int fillPercent, GameObject[] wallTextures)
    {
        //prelim variable setup
        {
            roomHeight = height;
            roomWidth = width;
            seed = tempSeed;
            startPos = pos;
            randomFillPercent = fillPercent;
            pseudoRandom = new System.Random(seed.GetHashCode());
            reigons = new Dictionary<int, HashSet<Vector2>>();
            checkedTiles = new HashSet<Vector2>();
            walls = wallTextures;
        }

        //map generation
        SetupTileArray();

        for (int n = 0; n < 5; n++)
        {
            SmoothMap();
        }

        //FindLargestReigon();

        MakeTileArrayFromNodes();
    }

    public int[,] GetRoomNodeLayout()
    {
        return tiles;
    }

    public int[,] GetRoomTileLayout()
    {
        MakeTileArrayFromNodes();
        return actual;
    }

    public Vector3 GetPosition()
    {
        return startPos;
    }

    void SetupTileArray()
    {
        tiles = new int[roomWidth, roomHeight];
        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomHeight; y++)
            {
                // Sets the tile to a 1 or 0 randomly depending on the randomFillPercent chosen
                tiles[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
            }
        }
    }

    public void SmoothMap()
    {
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                int score = NeighbourCountOf(new Vector2(x, y));
                //Debug.Log(score+" of "+ new Vector2(x, y));
                if (score > 4)
                {
                    tiles[x, y] = 1;
                }
                else if (score < 4)
                {
                    tiles[x, y] = 0;
                }
            }
        }
    }

    void MakeTileArrayFromNodes()
    {
        //List<List<int>> actualTemp = new List<List<int>>();
        actual = new int[roomWidth, roomHeight];
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                // -1 because of the called method GetScore()
                Vector2 v2Pos = new Vector2(x, y);
                int tempScore = GetScoreFor(v2Pos);
                actual[x, y] = tempScore;
            }
        }

    }

    // Needs to be updated since each room is being instantiated "upside-down" (-y)
    int GetScoreFor(Vector2 tlNodePos)
    {
        // Used to tell me how far in the try the program got (for the catch)
        int attemptScore = 0;

        // Var setup
        
        int tr = 0;
        int tl = 0;
        int br = 0;
        int bl = 0;
        
        try
        {
            bl = tiles[(int)tlNodePos.x, (int)tlNodePos.y];

            attemptScore++;
            br = tiles[(int)tlNodePos.x + 1, (int)tlNodePos.y];

            attemptScore++;
            tl = tiles[(int)tlNodePos.x, (int)tlNodePos.y + 1];

            attemptScore++;
            tr = tiles[(int)tlNodePos.x + 1, (int)tlNodePos.y + 1];

        }
        catch (Exception)
        {
            switch (attemptScore)
            {
                case 0:
                    // The coordinate provided was incorrect / out of bounds
                    return -1;

                case 1:
                    br = 1;
                    tr = 1;

                    try
                    {
                        // The "y + 1" is still unknown if in bounds
                        tl = tiles[(int)tlNodePos.x, (int)tlNodePos.y + 1];
                    }
                    catch (Exception)
                    {
                        tl = 1;
                    }

                    break;

                case 2:

                    tl = 1;
                    tr = 1;
                    break;
            }
        }

        int[] nodeArray = new int[4] { tl, tr, br, bl };

        int score = 0;

        for (int n = 0; n < nodeArray.GetLength(0); n++)
        {
            int s = nodeArray[n];
            if (s == 1)
            {
                score += (int)Mathf.Pow(2f, n); //adds the binary value of the node to the score for the tile
            }

        }

        return score;
    }

    bool CoordInMapBounds(Vector2 c)
    {
        if (c.x < 0 || c.y < 0)
            return false;

        if (c.x > roomWidth - 1 || c.y > roomHeight - 1)
            return false;

        return true;
    }

    int NeighbourCountOf(Vector2 tilePos)
    {
        int score = 0;
        for (int x = (int)tilePos.x - 1; x <= tilePos.x + 1; x++)
        {
            for (int y = (int)tilePos.y - 1; y <= tilePos.y + 1; y++)
            {
                if (x >= roomWidth || y >= roomHeight || x < 0 || y < 0)
                {
                    //selecting a square that hasn't been instantiated because it is out of bounds
                    score++;
                    continue;
                }
                else if (x == tilePos.x && y == tilePos.y)
                {
                    //selecting itself
                    continue;
                }

                //increments return value if there is a wall there
                score += tiles[x, y];
            }
        }

        return score;
    }

    //function finds the largest reigon on the map 
    //then fills the smaller ones with wall tiles 
    //to make all parts of the map reachable
    public void FindLargestReigon()
    {
        reigons = new Dictionary<int, HashSet<Vector2>>();
        checkedTiles = new HashSet<Vector2>();
        int highestReigonSize = 0;
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                //Debug.Log(checkedTiles.Contains(new Vector2(x, y)));
                if (tiles[x, y] != 0)
                    checkedTiles.Add(new Vector2(x, y));
                else if (!checkedTiles.Contains(new Vector2(x, y)))
                {
                    UpdateReigonDictionary(x, y);
                }
            }
        }

        //loop finds the largest reigon by size (key = size of reigon)
        foreach (KeyValuePair<int, HashSet<Vector2>> selectedReigon in reigons)
        {
            //if (name == "Main map")
            //    Debug.Log("reigon[" + selectedReigon.Key + "] = " + selectedReigon.Value.ToString());

            if (selectedReigon.Key > highestReigonSize)
                highestReigonSize = selectedReigon.Key;
        }

        if (highestReigonSize == 0)
        {
            return;
        }



        //places wall nodes everywhere except on tiles in the largest reigon size
        HashSet<Vector2> largestReigon = reigons[highestReigonSize];

        //if (isStartRoom)
        //    entrancePos =  pseudoRandom.Next(0, largestReigon.Count - 1);

        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                Vector2 tempPos = new Vector2(x, y);
                if (!largestReigon.Contains(tempPos)) //not in the largest reigon
                {
                    tiles[x, y] = 1; //changes the node array
                } else if (isStartRoomFlag)
                {
                    entrancePos = new Vector2(x, y);
                    isStartRoomFlag = false;
                } else if (isEndRoomFlag)
                {
                    endPos = new Vector2(x, y);
                    isEndRoomFlag = false;
                }

            }
        }

    }

    void UpdateReigonDictionary(int x, int y)
    {
        Vector2 startPos = new Vector2(x, y);
        Queue<Vector2> tileQueue = new Queue<Vector2>();
        HashSet<Vector2> tileList = new HashSet<Vector2>(); //hashset used because it is faster for .Contains() operations

        //loop randomly selects a tile which is a floor tile (a value of 0)
        while (tiles[(int) startPos.x, (int) startPos.y] != 0)
        {
            startPos = new Vector3(pseudoRandom.Next(1, roomWidth - 1), pseudoRandom.Next(1, roomHeight - 1), 0);
        }

        //creates queue with start tile inside
        tileQueue.Enqueue(startPos);
        tileList.Add(startPos);

        //loops through queue, adding all adjacent tiles to the topmost item into the queue
        while (tileQueue.Count > 0)
        {
            //Debug.Log("control: " + tileQueue.Count);
            Vector2 tilePos = tileQueue.Dequeue();
            Vector2[] neighbors = GetAdjacentTilesOf(tilePos, 0); //looking for floor tiles (value 0)
            foreach (Vector2 neighborPos in neighbors)
            {
                if (checkedTiles.Contains(neighborPos))
                    continue; //won't check the same tile twice

                checkedTiles.Add(neighborPos);
                tileQueue.Enqueue(neighborPos);
                tileList.Add(neighborPos);
            }
        }
        //adds list to an array which holds 
        reigons[tileList.Count] = tileList;
    }

    Vector2[] GetAdjacentTilesOf(Vector2 tilePos, int value)
    {
        List<Vector2> surrounding = new List<Vector2>();

        //loops through all positions around the tile
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {

                //the actual position on the map
                int n = (int)tilePos.x + x;
                int m = (int)tilePos.y + y;

                //if the tile is on a diagonal or is selected tile
                if (x == y || x == -y)
                    continue;

                //will only return if the value matches the specified value
                if (tiles[n, m] == value)
                    surrounding.Add(new Vector2(n, m));
            }
        }

        return surrounding.ToArray();
    }

    public void InstantiateNodes(int offsetX, int offsetY)
    {
        //tiles = mainMap.GetRoomNodeLayout();
        Debug.Log("x");
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                if (tiles[x, y] == 1)
                {
                    GameObject tile = Instantiate(filled, new Vector3(x + offsetX, -(y + offsetY), 0f), Quaternion.identity, transform) as GameObject;
                    tile.name = name + name.GetHashCode();
                }

            }
        }
    }

    public void InstantiateTiles(int offsetX, int offsetY)
    {
        for (int x = 0; x < actual.GetLength(0); x++)
        {
            for (int y = 0; y < actual.GetLength(1); y++)
            {
                if (actual[x, y] != 15 && actual[x, y] != 0)
                {
                    GameObject tilem = Instantiate(walls[0], new Vector3(x + offsetX, -(y + offsetY), 0f), Quaternion.identity, transform) as GameObject;
                    tilem.name = "Compensation";
                }

                if (actual[x, y] != 15)
                {
                    GameObject tile = Instantiate(walls[actual[x, y]], new Vector3(x + offsetX, -(y + offsetY), 0f), Quaternion.identity, transform) as GameObject;
                    tile.name = actual[x,y].ToString();
                }
                //Debug.Log(tile.transform.position);
            }
        }
    }
}
