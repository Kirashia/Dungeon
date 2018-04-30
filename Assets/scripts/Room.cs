using System;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

    public int roomHeight;
    public int roomWidth;
    public string seed;
    public Vector3 startPos;
    public GameObject entranceTile;
    public GameObject exitTile;

    [Range(0, 100)] public int randomFillPercent;
    public bool isStartRoom;
    public bool isEndRoom;
    public Vector3 entrancePos;
    public Vector3 endPos;

    public GameObject[] walls;

    private int[,] tiles;
    private int[,] actual;
    private List<List<Vector2>> floorTiles;
    private List<Vector2> exitTiles;
    public List<Vector2[]> connections;
    private HashSet<Vector2> checkedTiles;
    private System.Random pseudoRandom;

    private int constant;

    public void MakeStartRoom()
    {
        isStartRoom = true;
        isEndRoom = false;
    }

    public void MakeEndRoom()
    {
        isEndRoom = true;
        isStartRoom = false;
    }

    public void MakeEntranceAndExitsToReigons(int direction)
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
                DrawCircleAround(new Vector2(0, midHeight), 1, true);
                DrawCircleAround(new Vector2(roomWidth - 1, midHeight), 1, true);

                break;

            case 2:
                // Sides
                DrawCircleAround(new Vector2(0, midHeight), 1, true);
                DrawCircleAround(new Vector2(roomWidth - 1, midHeight), 1, true);

                // Top
                DrawCircleAround(new Vector2(midWidth, 0), 1, true);

                // Bottom
                DrawCircleAround(new Vector2(midWidth, roomHeight - 1), 1, true);

                break;

            case 3:
                // Sides 
                DrawCircleAround(new Vector2(0, midHeight), 1, true);
                DrawCircleAround(new Vector2(roomWidth - 1, midHeight), 1, true);

                // Top
                DrawCircleAround(new Vector2(midWidth, 0), 1, true);

                break;

        }
    }

    private void MakeEntranceAndExitsToRoom()
    {
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                if (isStartRoom && tiles[x, y] == 0)
                {
                    startPos = new Vector3(x, 0, y); // In 3D space the "y" is replaced with "z" otherwise pathfinding doesn't work
                    GameObject startTile = Instantiate(entranceTile, startPos, Quaternion.identity, transform) as GameObject;
                    startTile.name = "Start";
                    return; // No further action is required so return to the calling function
                }
                else if (isEndRoom && tiles[x, y] == 0)
                {
                    endPos = new Vector3(x, 0, y);
                    Debug.Log("Making end room at: " + endPos);
                    GameObject endTile = Instantiate(exitTile, endPos, Quaternion.identity,transform) as GameObject;
                    endTile.name = "End";
                    return;
                }
            }
        }
    }

    public void SetupRoom(Vector3 pos, int width, int height, string tempSeed, int fillPercent, GameObject[] wallTextures, int direciton)
    {
        //prelim variable setup
        {
            roomHeight = height;
            roomWidth = width;
            seed = tempSeed;
            startPos = pos;
            randomFillPercent = fillPercent;
            pseudoRandom = new System.Random(seed.GetHashCode());
            checkedTiles = new HashSet<Vector2>();
            walls = wallTextures;
            floorTiles = new List<List<Vector2>>();
            connections = new List<Vector2[]>();
            exitTiles = new List<Vector2>();
        }

        //map generation
        SetupTileArray();

        for (int n = 0; n < 5; n++)
        {
            SmoothMap();
        }

        // Adds a 2x1 hole to each side corresponding to the correct direction
        MakeEntranceAndExitsToReigons(direciton);
        FindReigons();
        ConnectReigons();
        DrawConnections();

        // A couple more smoothing iterations to hopefully remove random obstacles
        for (int n = 0; n < 2; n++)
        {
            SmoothMap();
        }
    }

    // Extra overload in case a map is pre-provided
    public void SetupMap(Vector3 pos, int[,] nodeMap, string tempSeed, GameObject[] wallTextures)
    {
        //prelim variable setup

        tiles = nodeMap;
        roomHeight = nodeMap.GetLength(0);
        roomWidth = nodeMap.GetLength(1);
        seed = tempSeed;
        startPos = pos;
        pseudoRandom = new System.Random(seed.GetHashCode());
        checkedTiles = new HashSet<Vector2>();
        walls = wallTextures;
        floorTiles = new List<List<Vector2>>();
        connections = new List<Vector2[]>();
        exitTiles = new List<Vector2>();

        FindReigons();

        ConnectReigons();
        DrawConnections();
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
        int boundaryScore = (exitTiles.Contains(tlNodePos)) ? 0 : 1;
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
        catch (System.Exception)
        {
            switch (attemptScore)
            {
                case 0:
                    // The coordinate provided was incorrect / out of bounds
                    return -1;

                case 1:
                    br = boundaryScore;
                    tr = boundaryScore;

                    try
                    {
                        // The "y + 1" is still unknown if in bounds
                        tl = tiles[(int)tlNodePos.x, (int)tlNodePos.y + 1];
                    }
                    catch (System.Exception)
                    {
                        tl = boundaryScore;
                    }

                    break;

                case 2:

                    tl = boundaryScore;
                    tr = boundaryScore;
                    break;
            }
        }

        int[] nodeArray = new int[4] { bl, br, tr, tl };

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

    // Function finds all the reigons on the map
    // Then puts them in a 2D list
    public void FindReigons()
    {
        checkedTiles = new HashSet<Vector2>();
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                if (tiles[x, y] != 0)
                    checkedTiles.Add(new Vector2(x, y));
                else if (!checkedTiles.Contains(new Vector2(x, y)))
                {
                    UpdateReigonDictionary(x, y);
                }
            }
        }
    }

    void UpdateReigonDictionary(int x, int y)
    {
        Vector2 startPos = new Vector2(x, y);
        Queue<Vector2> tileQueue = new Queue<Vector2>();
        List<Vector2> tileList = new List<Vector2>();

        //loop randomly selects a tile which is a floor tile (a value of 0)
        while (tiles[(int)startPos.x, (int)startPos.y] != 0)
        {
            if (CoordInMapBounds(startPos))
            {
                startPos.x++;
            }
            else
            {
                startPos.y++;
                startPos.x = 0;
            }
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
        name = name;
        // Adds reigon to tile list
        floorTiles.Add(tileList);
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
                if (x == y || x == -y || !CoordInMapBounds(new Vector2(n, m)))
                    continue;

                //will only return if the value matches the specified value
                if (tiles[n, m] == value)
                    surrounding.Add(new Vector2(n, m));
            }
        }

        return surrounding.ToArray();
    }

    void ConnectReigons()
    {
        List<List<Vector2>> reigons = floorTiles;

        // Loop finds the shortest distance between two tiles in different reigons
        // Then adds all the tiles in one reigon to the first index
        // And removes the added reigons from the list
        Vector2 closestTile;

        while (reigons.Count > 1)
        {
            float minLength = -1f;
            float[] minIndex = new float[] { 0, 0, 0 }; // A blank slate
            Vector2 startTile = reigons[0][0];


            // Finds tile in connected reigon that's closest to an unconnected reigon
            foreach (Vector2 tile in reigons[0])
            {
                float[] index = FindClosestTileTo(tile, reigons);
                if (minLength == -1f || index[2] < minLength)
                {
                    startTile = tile;
                    minIndex = index;
                    minLength = index[2];
                }
            }

            // Assign variables to closest tile's values
            closestTile = reigons[(int)minIndex[0]][(int)minIndex[1]];
            minLength = minIndex[2];

            reigons[0].AddRange(reigons[(int)minIndex[0]]); // Adds all the selected reigon to the connected part of the 2D array
            reigons.RemoveAt((int)minIndex[0]);             // Remove the reigon from the unconnected part of the 2D array

            connections.Add(new Vector2[] { (startTile), closestTile });
        }
    }

    void DrawConnections()
    {
        foreach (Vector2[] pair in connections)
        {
            DrawCorridor(pair[0], pair[1]);
        }
    }

    List<Vector2> GetTilesOnLine(Vector2 start, Vector2 end)
    {
        List<Vector2> line = new List<Vector2>();

        int x = (int)start.x;
        int y = (int)start.y;

        int dx = (int)end.x - x;
        int dy = (int)end.y - y;

        int step = (int)Mathf.Sign(dx);
        int gradientStep = (int)Mathf.Sign(dy);

        bool inverted = false;
        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        // Sometimes the line will be more vertical, this adapts the algorithm to make it easier
        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = (int)Mathf.Sign(dy);
            gradientStep = (int)Mathf.Sign(dx);
        }

        int gradientAccumulation = longest / 2;

        for (int n = 0; n < longest; n++)
        {
            line.Add(new Vector2(x, y));

            if (inverted)
                y += step;

            else
                x += step;

            gradientAccumulation += shortest;

            if (gradientAccumulation >= longest)
            {
                if (inverted)
                    x += gradientStep;

                else
                    y += gradientStep;

                gradientAccumulation -= longest;
            }

        }

        return line;
    }

    void DrawCorridor(Vector2 start, Vector2 end)
    {
        List<Vector2> line = GetTilesOnLine(start, end);
        foreach (Vector2 tile in line)
        {
            DrawCircleAround(tile, 2);
            //if (name == "Map")
            //    print("y");
        }
    }

    void DrawCircleAround(Vector2 point, int radius)
    {
        List<Vector2> debugList = new List<Vector2>();
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                // Lies in or on the circle
                if (x * x + y * y <= radius * radius)
                {
                    int drawX = (int)point.x + x;
                    int drawY = (int)point.y + y;

                    if (CoordInMapBounds(new Vector2(drawX, drawY)))
                    {
                        // Make the point a floor tile
                        tiles[drawX, drawY] = 0;
                        debugList.Add(new Vector2(drawX, drawY));
                    }
                }
            }
        }

        floorTiles.Add(debugList);
    }

    // Overload specifically for exits to the room
    void DrawCircleAround(Vector2 point, int radius, bool isExit)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                // Lies in or on the circle
                if (x * x + y * y <= radius * radius)
                {
                    int drawX = (int)point.x + x;
                    int drawY = (int)point.y + y;

                    if (CoordInMapBounds(new Vector2(drawX, drawY)))
                    {
                        // Make the point a floor tile
                        tiles[drawX, drawY] = 0;
                        exitTiles.Add(new Vector2(drawX, drawY));
                    }
                }
            }
        }
    }


    float[] FindClosestTileTo(Vector2 pos, List<List<Vector2>> array)
    {
        // Key:
        //      0 - x coord
        //      1 - y coord
        //      2 - distance using sqrMagnitude

        float[] index = new float[3];
        index[2] = -1;

        // For loop starts from 0 so as not to include the main reigon
        // That are already conencted
        for (int x = 1; x < array.Count; x++)
        {
            for (int y = 0; y < array[x].Count; y++)
            {
                Vector2 s = array[x][y];
                if (index[2] == -1 || (pos - s).sqrMagnitude < index[2])
                {
                    index[0] = x;
                    index[1] = y;
                    index[2] = (pos - s).sqrMagnitude;
                }
            }
        }

        return index;
    }

    public void InstantiateNodes(int offsetX, int offsetY)
    {
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                if (tiles[x, y] == 1)
                {
                    GameObject tile = Instantiate(walls[16], new Vector3(x + offsetX, 0f, (y + offsetY)), Quaternion.identity, transform) as GameObject;
                    tile.name = name + name.GetHashCode();
                }

            }
        }
    }

    public void InstantiateTiles(int offsetX, int offsetY)
    {
        // Compensation to make all areas accessible
        WidenBorders();
        // Makes sure the tile array is up to date
        MakeTileArrayFromNodes();

        Quaternion angleCompensation = Quaternion.Euler(90, 0, 0);

        for (int x = 0; x < actual.GetLength(0); x++)
        {
            for (int y = 0; y < actual.GetLength(1); y++)
            {
                if (/*actual[x,y] != 15 &&*/ actual[x,y] != 0)
                {
                    GameObject tile = Instantiate(walls[16], new Vector3(x + offsetX, 0f, (y + offsetY)), Quaternion.identity, transform) as GameObject;
                    tile.name = name + actual[x,y];
                }

                if (actual[x, y] == 0)
                {
                    GameObject tilem = Instantiate(walls[0], new Vector3(x + offsetX, 0f, (y + offsetY)), angleCompensation, transform) as GameObject;
                    tilem.name = "Compensation";
                }
            }
        }
    }

    public void WidenBorders()
    {
        MakeTileArrayFromNodes();
        //Debug.Log("Boop");

        for (int x = 0; x < actual.GetLength(0); x++)
        {
            for (int y = 0; y < actual.GetLength(1); y++)
            {
                if (actual[x, y] != 15 && actual[x, y] != 0)
                {
                    DrawCircleAround(new Vector2(x, y), 1);
                }
            }
        }
    }
}
