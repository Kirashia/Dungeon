using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCreator : MonoBehaviour {

    public int mapWidth;
    public int mapHeight;
    public string seed;
    public string tempSeed;
    public bool useRandomSeed;
    public int smoothness;
    [Range (0, 100)] public int randomFillPercent;

    public GameObject wall;
    public GameObject floor;
    public GameObject filled;
    public GameObject[] marchingSquares;
    public GameObject[] testRooms;
    public GameObject roomTemplate;
    public GameObject startTile;
    public GameObject endTile;

    public int roomHeight;
    public int roomWidth;

    private int[,] tiles;
    public Vector2 startLocation;
    public Vector2 endLocation;
    private System.Random pseudoRandom;

    private GameObject roomHolder;
    private List<Room> rooms;

    public void GenerateMap()
    {
        if (seed == null)
            seed = "";

        if (useRandomSeed)
            seed = unchecked(System.DateTime.Now.Ticks.GetHashCode()).ToString();

        roomHolder = new GameObject("Room Holder");
        pseudoRandom = new System.Random(seed.GetHashCode());
        rooms = new List<Room>();

        GenerateMapTemplate();

        //generates the cavernous rooms using the Room script
        MakeRooms();
        MakeBorders();

        ConnectRooms();
    }

    public Vector2 GetPlayerStartLocation()
    {
        return Vector2.Scale(startLocation, new Vector2(roomWidth, roomHeight));
    }

    void GenerateMapTemplate()
    {
        tiles = new int[mapWidth, mapHeight];

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                tiles[x, y] = 0;
            }
        }

        System.Random r = pseudoRandom;

        Vector2 start = new Vector2(r.Next(0, tiles.GetLength(0)), 0);
        tiles[(int)start.x, (int)start.y] = 1;
        startLocation = start;
        bool exitPlaced = false;
        int randomDirection;
        int prevRoom = 0;
        int currentRoom = 1;
        int prevDir = 9;
        Vector2 pointer = start;

        bool startingDir = true;

        int dy = 0;
        int dx = 0;

        // Loop generates a solution path
        while (!exitPlaced)
        {
            dy = 0;
            dx = 0;

            if (!startingDir)
                randomDirection = r.Next(1, 6);
            else
            {
                startingDir = false;
                randomDirection = r.Next(1, 5); // won't drop unless hits edge
            }

            // Loop to make sure it doesn't backtrack
            while (((randomDirection == 2 || randomDirection == 1) && (prevDir == 3 || prevDir == 4)) || ((prevDir == 2 || prevDir == 1) && (randomDirection == 3 || randomDirection == 4)))
            {
                randomDirection = r.Next(1, 6);
            }

            if (((randomDirection == 1 || randomDirection == 2) && (int)pointer.x == 0) || ((randomDirection == 3 || randomDirection == 4) && (int)pointer.x == mapWidth - 1))
            {
                if (pointer.y < mapHeight - 1)
                {
                    dy = 1;
                    currentRoom = (prevRoom == 2) ? 4 : 2;
                    tiles[(int)pointer.x, (int)pointer.y] = currentRoom;
                    pointer += new Vector2(dx, dy);
                    continue;
                }
                else
                {
                    exitPlaced = true;
                    endLocation = pointer;
                    tiles[(int)pointer.x, (int)pointer.y] = currentRoom;
                    break;
                }
            }

            else if (prevRoom == 2)
            {
                currentRoom = 3;
            }
            else
            {
                currentRoom = 1;
            }

            switch (randomDirection)
            {
                case 1:
                case 2:
                    dx = -1;
                    break;
                case 3:
                case 4:
                    dx = 1;
                    break;
                case 5:
                    dy = 1;
                    currentRoom = 2;
                    break;
            }

            if (dy == 1 && pointer.y >= mapHeight - 1)
            {
                exitPlaced = true;
                tiles[(int)pointer.x, (int)pointer.y] = currentRoom;
                endLocation = pointer;
                break;
            }

            try
            {
                tiles[(int)pointer.x, (int)pointer.y] = currentRoom;
            }
            catch (System.Exception)
            {
                print("an error occurred");
            }
            pointer += new Vector2(dx, dy);
            prevDir = randomDirection;
            prevRoom = currentRoom;
        
        }

    }  

    void MakeRooms()
    {
        // Debugging

        tempSeed = seed;
        int pointerX = 0;
        int pointerY = 0;

        for(int x = 0; x < mapWidth * roomWidth; x += roomWidth)
        {
            pointerY = 0;
            for (int y = 0; y < mapHeight * roomHeight; y += roomHeight)
            {
                Vector2 pos = new Vector2(x, y);
                GameObject roomGO = Instantiate(roomTemplate, new Vector2(pos.x, -pos.y), Quaternion.identity, roomHolder.transform) as GameObject;
                roomGO.name = tiles[pointerX, pointerY].ToString();
                Room room = roomGO.GetComponent<Room>();

                // Sets flag variables which slightly change the setup of the room
                if (new Vector2(pointerX,pointerY) == startLocation)
                {
                    room.MakeStartRoom();
                }
                else if (new Vector2(pointerX, pointerY) == endLocation)
                {
                    room.MakeEndRoom();
                }

                room.SetupRoom(pos, roomWidth, roomHeight, tempSeed, randomFillPercent, marchingSquares, tiles[pointerX, pointerY]);

                rooms.Add(room);

                //room.InstantiateTiles(x, y);
                //room.InstantiateNodes(x, y);

                // Change the seed using the current seed
                // This ensures the same string of rooms will be made from a single starting seed
                tempSeed = tempSeed.GetHashCode().ToString();

                pointerY++;
            }
            pointerX++;
        }
    }

    void ConnectRooms()
    {
        int[,] map = new int[mapWidth * roomWidth, mapHeight * roomHeight];
        int count = 0;

        for (int n = 0; n < mapWidth * roomWidth; n += roomWidth)
        {
            for (int m = 0; m < mapHeight * roomHeight; m += roomHeight)
            {
                int[,] roomLayout = rooms[count].GetRoomNodeLayout();

                for (int x = 0; x < roomLayout.GetLength(0); x++)
                {
                    for (int y = 0; y < roomLayout.GetLength(1); y++)
                    {
                        map[x + n, y + m] = roomLayout[x, y];
                    }
                }
                count++;
            }
        }

        GameObject roomGO = Instantiate(roomTemplate, new Vector3(0, 0, 0), Quaternion.identity, roomHolder.transform) as GameObject;
        roomGO.name = "Map";
        Room room = roomGO.GetComponent<Room>();

        // Overload should find and connect reigons automatically without creating a new map
        room.SetupMap(new Vector3(0, 0, 0), map, seed, marchingSquares);
        map = room.GetRoomTileLayout();

        room.InstantiateTiles(0, 0);

        //InstantiateTiles(map);
    }

    private void InstantiateTiles(int[,] map)
    {
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] != 15 && map[x, y] != 0)
                {
                    GameObject tilem = Instantiate(marchingSquares[0], new Vector3(x, -y, 0f), Quaternion.identity, transform) as GameObject;
                    tilem.name = "Compensation";
                }

                if (map[x, y] != 15)
                {
                    GameObject tile = Instantiate(marchingSquares[map[x, y]], new Vector3(x, -y, 0f), Quaternion.identity, transform) as GameObject;
                    tile.name = map[x, y].ToString();
                }
                //Debug.Log(tile.transform.position);
            }
        }
    }

    // Makes sure the map has no exits to the outside of the map (void space)
    void MakeBorders()
    {
        Quaternion angleCompensation = Quaternion.Euler(90,0,0);

        for (int x = -1; x < mapWidth * roomWidth; x++)
        {
            // Top
            GameObject tileA = Instantiate(wall, new Vector3(x, 0f, -1), angleCompensation, transform) as GameObject;
            tileA.name = "Border: " + tileA.GetHashCode();

            // Bottom
            GameObject tileB = Instantiate(wall, new Vector3(x, 0f, (mapHeight * roomHeight)), angleCompensation, transform) as GameObject;
            tileB.name = "Border: " + tileB.GetHashCode();
        }

        for (int y = -1; y < mapHeight * roomHeight; y++)
        {
            // Left
            GameObject tileA = Instantiate(wall, new Vector3(-1, 0f, y), angleCompensation, transform) as GameObject;
            tileA.name = "Border: " + tileA.GetHashCode();

            // Right
            GameObject tileB = Instantiate(wall, new Vector3(mapWidth * roomWidth, 0f, y), angleCompensation, transform) as GameObject;
            tileB.name = "Border: " + tileB.GetHashCode();
        }
    }

}
