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
    private Dictionary<Vector2, Direction> borders;
    private System.Random pseudoRandom;

    private Room mainMap;

    private Vector2 entrancePos;
    private Vector2 endPos;

    private GameObject wallHolder;
    private GameObject floorHolder;
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

        wallHolder = new GameObject("Wall Holder");
        floorHolder = new GameObject("Floor Holder");
        roomHolder = new GameObject("Room Holder");
        pseudoRandom = new System.Random(seed.GetHashCode());
        borders = new Dictionary<Vector2, Direction>();

        //SetupTileArray();

        GenerateMapTemplate();

        //generates a full (all wall) larger map
        mainMapGO = Instantiate(roomTemplate, Vector3.zero, Quaternion.identity) as GameObject; //gameobject for the main map
        mainMap = mainMapGO.GetComponent<Room>();
        mainMap.SetupRoom(Vector3.zero, mapWidth, mapHeight, seed, 100);
        mainMap.name = "Main map";
        tiles = mainMap.GetRoomNodeLayout();
        borderWalls = mainMap.GetBorderWalls();

        //generates the cavernous rooms using the Room script
        NewMakeRooms();

        mainMap.FindLargestReigon(); //makes sure all parts of the map are reachable

        for (int i = 0; i < smoothness; i++)
            mainMap.SmoothMap();

        //instantiation
        InstantiateNodes();       //for debugging
        //UpdateBorderWalls();
        //InstantiateBorders();
        //InstantiateTiles();       //instantiate the tiles one at a time, until the map is fully instantiated

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
                tiles[(int)pointer.x, (int)pointer.y] = 9;
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

    void UpdateBorderWalls()
    {
        borderWalls = mainMap.GetBorderWalls();
        borders = mainMap.GetBorderDirections();
      
    }
    
    void MakeRooms()
    {
        tempSeed = seed;

        if (maxNumberOfRooms <= 0)
            return;

        int width = 15;
        int height = 15;

        Vector2 pos = new Vector2 (0,0);

        GameObject roomGO = Instantiate(roomTemplate, pos, Quaternion.identity, roomHolder.transform) as GameObject;
        Room room = roomGO.GetComponent<Room>();
        room.SetupRoom(pos, width, height, tempSeed, randomFillPercent);
        AddRoomToMap(room.GetRoomNodeLayout(), pos);

        bool flag = true;

        while (flag)
        {
            tempSeed = tempSeed.GetHashCode().ToString();
            pos += new Vector2(15, 0);
            if (pos.x >= mapWidth)
            {
                pos.x = 0;
                pos.y += 15;
            }

            if (pos.y >= mapHeight - 15)
            {
                flag = false;
                return;
            }

            roomGO = Instantiate(roomTemplate, pos, Quaternion.identity, roomHolder.transform) as GameObject;
            room = roomGO.GetComponent<Room>();
            room.SetupRoom(pos, width, height, tempSeed, randomFillPercent);
            AddRoomToMap(room.GetRoomNodeLayout(), pos);
        }

    }

    void NewMakeRooms()
    {
        tempSeed = seed;
        for(int x = 0; x < mapWidth * roomWidth; x += roomWidth)
        {
            for (int y = 0; y < mapHeight * roomHeight; y += roomHeight)
            {
                Vector2 pos = new Vector2(x, y);
                GameObject roomGO = Instantiate(roomTemplate, pos, Quaternion.identity, roomHolder.transform) as GameObject;
                Room room = roomGO.GetComponent<Room>();
                room.SetupRoom(pos, roomWidth, roomHeight, tempSeed, randomFillPercent);
                room.MakeEntranceAndExits(tiles[x, y]);
            }
        }
    }

    // Need to change map gen. to be more like the one in the prototype

    // works fine except needs to add from the actual floor tiles in the room not the bottom left corner of the room
    void AddRoomToMap(int[,] room, Vector3 tlPosition/*, Direction facingDirection*/)
    {
        int n = 0; //these variables control the pointer position in the room array
        int m = 0; //x and y control the pointer position in the map array

        // loop through map
        for (int x = (int)tlPosition.x; x < tlPosition.x + room.GetLength(0); x++)
        {
            m = 0; // to control iteration through room array
            for (int y = (int)tlPosition.y; y < tlPosition.y + room.GetLength(1); y++)
            {
                //Debug.Log(x + ", " + y);

                if (x < float.Epsilon || x >= mapWidth || y < float.Epsilon || y >= mapHeight)
                    break;

                if (room[n, m] == 0 && tiles[x, y] != 0)
                    tiles[x,y] = 0;

                m++;
            }
            n++;
        }

        return;

        // scan to see if the room is covered in floor tiles
        bool loopFlag = true;
        bool canPlace = false;
        int counter = 0;
        int lastMove = 1;
        int multiplier = 1;

        while (loopFlag)
        {
            n = 0;
        // loop through map
            for (int x = (int)tlPosition.x; x < tlPosition.x + room.GetLength(0); x++)
            {
                m = 0; // to control iteration through room array
                for (int y = (int)tlPosition.y; y < tlPosition.y + room.GetLength(1); y++)
                {
                    if (room[n, m] == 0 && tiles[x,y] != 0)
                        canPlace = true;

                    m++;
                }
                n++;
            }
            if (canPlace)
                loopFlag = false;

            else // Spirals around start point until a satisfactory start point is found
            {
                switch (lastMove)
                {
                    case 1:
                        tlPosition.y += 1 * multiplier;
                        lastMove = 2;
                        break;
                    case 2:
                        tlPosition.x += 1 * multiplier;
                        lastMove = 3;
                        break;
                    case 3:
                        tlPosition.y -= 1 * multiplier;
                        lastMove = 4;
                        break;
                    case 4:
                        tlPosition.x -= 1 * multiplier;
                        lastMove = 1;
                        break;

                }
            multiplier++;
            }
            if (tlPosition.x < float.Epsilon || tlPosition.x >= mapWidth || tlPosition.y < float.Epsilon || tlPosition.y >= mapHeight)
                return;

            counter++;
        }

        //switch (lastMove)
        //{
        //    case 4:
        //        tlPosition.y += 1 * multiplier;
        //        lastMove = 2;
        //        break;
        //    case 1:
        //        tlPosition.x += 1 * multiplier;
        //        lastMove = 3;
        //        break;
        //    case 2:
        //        tlPosition.y -= 1 * multiplier;
        //        lastMove = 4;
        //        break;
        //    case 3:
        //        multiplier++;
        //        tlPosition.x -= 1 * multiplier;
        //        lastMove = 1;
        //        break;

        //}

        n = 0;
        // actually add the room to the map
        for (int x = (int) tlPosition.x; x < tlPosition.x + room.GetLength(0); x++)
        {
            m = 0;
            for (int y = (int)tlPosition.y; y < tlPosition.y + room.GetLength(1); y++)
            {
                if (x >= mapWidth || y >= mapHeight || x < 0 || y < 0)
                    continue;

                if (room[n, m] == 0)
                {//only floor tiles are added
                    int t = room[n, m];
                    //Debug.Log(tiles.GetLength(0) + ", " + tiles.GetLength(1));
                    //Debug.Log(x + ", " + y);
                    tiles[x, y] = t;
                }

                m++;
            }
            n++;
        }
    }

    void SetupTileArray()
    {
        tiles = new int[mapWidth, mapHeight];
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (x == 0 || x == mapWidth - 1 || y == 0 || y == mapHeight - 1)
                {
                    tiles[x, y] = 1;
                }
                else
                {
                    //sets the tile to a 1 or 0 randomly depending on the randomFillPercent chosen
                    tiles[x, y] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }
    }

    void CreateBorderWalls()
    {
        //this loop creates "border walls" which border a floor
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                foreach (Vector2 pos in GetSurroundingTiles(new Vector2(x, y)))
                {
                    //Debug.Log("X:" + pos.x + "\nY:" + pos.y);
                    //walls will be touching a floor tile
     //               if (tiles[(int)pos.x, (int)pos.y] == TileType.Floor && tiles[x, y] == TileType.Wall)
                    {
                        //Debug.Log("border");
       //                 tiles[x, y] = TileType.BorderWall;
                        break;
                    }

                }
            }
        }
    }

    int NeighbourCountOf(Vector2 tilePos)
    {
        int score = 0;
        for (int x = (int)tilePos.x - 1; x <= tilePos.x + 1; x++)
        {
            for (int y = (int)tilePos.y - 1; y <= tilePos.y + 1; y++)
            {
                if (x >= mapWidth || y >= mapHeight || x < 0 || y < 0)
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
         //       score += (tiles[x, y] == TileType.Wall) ? 1 : 0;
            }
        }

        return score;
    }

    Vector2[] GetSurroundingTiles(Vector2 pos)
    {
        List<Vector2> neighbours = new List<Vector2>();
        for (int x = (int)pos.x - 1; x <= pos.x + 1; x++)
        {
            for (int y = (int)pos.y - 1; y <= pos.y + 1; y++)
            {
                if (x >= mapWidth || x < 0 || y >= mapHeight || y < 0 || (x == (int)pos.x && y == (int)pos.y) || !(x == (int)pos.x || y == (int)pos.y))
                    continue;

                neighbours.Add(new Vector2(x, y));
            }
        }
        return neighbours.ToArray();
    }

    void MakeTileArrayFromNodes()
    {
        //List<List<int>> actualTemp = new List<List<int>>();
        actual = new int[mapWidth, mapHeight];
        for (int x = 0; x < tiles.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < tiles.GetLength(1) - 1; y++)
            {
                // -1 because of the called method GetScore()
                Vector2 v2Pos = new Vector2(x, y);
                int tempScore = GetScoreFor(v2Pos);
                actual[x, y] = tempScore;
            }
        }

    }

    int GetScoreFor(Vector2 tlNodePos)
    {
        int bl = tiles[(int)tlNodePos.x, (int)tlNodePos.y];
        int br = tiles[(int)tlNodePos.x + 1, (int)tlNodePos.y];
        int tl = tiles[(int)tlNodePos.x, (int)tlNodePos.y + 1];
        int tr = tiles[(int)tlNodePos.x + 1, (int)tlNodePos.y + 1];

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

    void InstantiateTiles()
    {
        if (endPos != null)
            Instantiate(endTile, endPos, Quaternion.identity, wallHolder.transform);
        if (entrancePos != null)
            Instantiate(startTile, entrancePos, Quaternion.identity, wallHolder.transform);

        actual = mainMap.GetRoomTileLayout();

        for (int x = 0; x < actual.GetLength(0); x++)
        {
            for (int y = 0; y < actual.GetLength(1); y++)
            {
                if (actual[x, y] != 15 && actual[x, y] != 0)
                {
                    GameObject tilem = Instantiate(marchingSquares[0], new Vector3(x, y, 0f), Quaternion.identity, floorHolder.transform) as GameObject;
                    tilem.name = "Compensation";
                }

                GameObject tile = Instantiate(marchingSquares[actual[x,y]], new Vector3(x, y, 0f), Quaternion.identity, mainMapGO.transform) as GameObject;
                //Debug.Log(tile.transform.position);
            }
        }
    }

    void InstantiateNodes()
    {
        //tiles = mainMap.GetRoomNodeLayout();
        for (int x = 0; x < tiles.GetLength(0); x++)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                if (tiles[x, y] == 1)
                    Instantiate(testRooms[0], new Vector3(x, -y, 0f), Quaternion.identity, wallHolder.transform);
                if (tiles[x, y] == 2)
                    Instantiate(testRooms[1], new Vector3(x, - y, 0f), Quaternion.identity, wallHolder.transform);
                if (tiles[x, y] == 3)
                    Instantiate(testRooms[2], new Vector3(x, -y, 0f), Quaternion.identity, wallHolder.transform);
                if (tiles[x, y] == 4)
                    Instantiate(testRooms[3], new Vector3(x, -y, 0f), Quaternion.identity, wallHolder.transform);

            }
        }
    }

    void InstantiateBorders()
    {
        foreach (Vector2 x in borderWalls)
        { 
            Instantiate(filled, x, Quaternion.identity, wallHolder.transform);
        }
    }
}
