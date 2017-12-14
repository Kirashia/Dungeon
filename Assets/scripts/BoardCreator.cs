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
    public int maxNumberOfRooms;

    private int[,] tiles;
    private int[,] actual;
    private List<Vector2> borderWalls;
    //private Dictionary<Vector2, Direction> borders;
    private System.Random pseudoRandom;

    private Room mainMap;

    private Vector2 entrancePos;
    private Vector2 endPos;

    private GameObject roomHolder;
    private GameObject mainMapGO;
    
    public enum TileType
    {
        Wall,
        BorderWall,
        Floor,
        Filled,
    }

    public enum Direction
    {
        North,
        East,
        South,
        West,
    }

    class TiledRoom
    {
        public int w;
        public int h;
        public int fP;
        public string seed;

        public TiledRoom(int width, int height, int fillPercent, string roomSeed)
        {
            w = width;
            h = height;
            fP = fillPercent;
            seed = roomSeed;
        }
    }

    public void GenerateMap()
    {
        if (seed == null)
            seed = "";

        if (useRandomSeed)
            seed = unchecked(System.DateTime.Now.Ticks.GetHashCode()).ToString();

        roomHolder = new GameObject("Room Holder");
        pseudoRandom = new System.Random(seed.GetHashCode());

        GenerateMapTemplate();

        //generates the cavernous rooms using the Room script
        MakeRooms();

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
        tiles[(int)start.x, (int)start.y] = 9;
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

            if (((randomDirection == 1 || randomDirection == 2) && (int)pointer.x == 0) || ((randomDirection == 3 || randomDirection == 4) && (int)pointer.x == 3))
            {
                if (pointer.y != 3)
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
                    tiles[(int)pointer.x, (int)pointer.y] = 9;
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

            if (dy == 1 && pointer.y == 3)
            {
                exitPlaced = true;
                tiles[(int)pointer.x, (int)pointer.y] = currentRoom;
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
        tempSeed = seed;
        int pointerX = 0;
        int pointerY = 0;

        for(int x = 0; x < mapWidth * roomWidth; x += roomWidth)
        {
            pointerY = 0;
            print("kjdfnsoj");
            for (int y = 0; y < mapHeight * roomHeight; y += roomHeight)
            {
                Vector2 pos = new Vector2(x, -y);
                GameObject roomGO = Instantiate(roomTemplate, pos, Quaternion.identity, roomHolder.transform) as GameObject;
                Room room = roomGO.GetComponent<Room>();
                room.SetupRoom(pos, roomWidth, roomHeight, tempSeed, randomFillPercent, marchingSquares);
                room.MakeEntranceAndExits(tiles[pointerX, pointerY]);
                room.InstantiateNodes(x,y);

                // Change the seed using the current seed
                // This ensures the same string of rooms will be made from a single starting seed
                tempSeed = tempSeed.GetHashCode().ToString();

                pointerY++;
            }
            pointerX++;
        }
    }

}
