using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class MazeGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject ExtraroomPrefab;
    public GameObject WallPrefab;
    public GameObject MirrorPrefab;
    public GameObject AbsorbPrefab;
    public GameObject TransparentPrefab;
    public GameObject GroundPrefab;
    public GameObject CeilingPrefab;
    public GameObject CoinPrefab;
    public GameObject ExitPrefab;
    public GameObject LightPrefab;
    public GameObject Player;

    [Header("Maze Settings")]
    public int MazeWidth = 3;
    public int MazeHeight = 3;
    public bool AlwaysGenerate = true;

    private float CellSize = 1f;
    private int[,] maze;
    private (int x, int y) entrance;
    private (int x, int y) exit;

    void Start()
    {
        string path = Path.Combine(Application.dataPath, "Data/maze.txt");

        int width = 0;
        int height = 0;
        if (!AlwaysGenerate){
            try{
                maze = ReadArrayFromFile(path);
                width = maze.GetLength(0);
                height = maze.GetLength(1);
        
                entrance =  FindSingleBorderValue(0);
                exit =  FindSingleBorderValue(6);
            }catch (Exception ex)
            {
                Debug.LogError($"Error reading maze file: {ex.Message}.");
            }
        }

        if (width < 3 || height < 3 || entrance.x == -1 || exit.y != height - 2 || exit.x != width -1 || width % 2 == 0 || height % 2 == 0){
            Debug.Log("Generating new maze.");
            if (MazeWidth < 3 || MazeHeight < 3){
                MazeWidth = 3;
                MazeHeight = 3;
            }
            GenerateMaze(MazeWidth, MazeHeight);
            PlaceLightsInMaze();
            ReplaceSomeWalls();
            WriteIntArrayToFile();
        }
        
        PrintMaze();
        SpawnMaze();
        SpawnCoin();
        BuildEntranceRoom();
        SpawnPrefabAtExit();
    }

    public void GenerateMaze(int width, int height)
    {
        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;

        maze = new int[height, width];

        // Fill with walls
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                maze[y, x] = 1;

        var rand = new System.Random();
        Stack<(int x, int y)> stack = new Stack<(int x, int y)>();
        maze[1, 1] = 0;
        stack.Push((1, 1));

        int[] dx = { 0, 0, -2, 2 };
        int[] dy = { -2, 2, 0, 0 };

        while (stack.Count > 0)
        {
            var (x, y) = stack.Peek();
            List<int> directions = new List<int> { 0, 1, 2, 3 };
            bool carved = false;

            while (directions.Count > 0)
            {
                int i = rand.Next(directions.Count);
                int dir = directions[i];
                directions.RemoveAt(i);

                int nx = x + dx[dir];
                int ny = y + dy[dir];

                if (nx > 0 && ny > 0 && nx < width - 1 && ny < height - 1 && maze[ny, nx] == 1)
                {
                    maze[ny, nx] = 0;
                    maze[y + dy[dir] / 2, x + dx[dir] / 2] = 0;
                    stack.Push((nx, ny));
                    carved = true;
                    break;
                }
            }

            if (!carved)
                stack.Pop();
        }

        // Add loops for complexity
        AddLoops();

        exit = (width - 1, height - 2);

        maze[exit.y, exit.x] = 6;

        entrance = FindFarthestEdge(exit);
        maze[entrance.y, entrance.x] = 0;
    }

    public void ReplaceSomeWalls()
    {
        float mirrorChance = 0.1f;
        float absorbChance = 0.1f;
        float transparentChance = 0.1f;
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);
        System.Random rng = new System.Random();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (maze[x, y] != 1) continue; // only replace solid walls

                bool isBorder = x == 0 || y == 0 || x == width - 1 || y == height - 1;

                if (!isBorder && rng.NextDouble() < transparentChance)
                {
                    maze[x, y] = 3; // transparent wall
                }
                else if (rng.NextDouble() < mirrorChance)
                {
                    maze[x, y] = 4; // mirror wall
                }
                else if (rng.NextDouble() < absorbChance)
                {
                    maze[x, y] = 2; // absorb wall
                }
            }
        }
    }

    private void AddLoops()
    {
        System.Random rand = new System.Random();
        float loopChance = 0.05f;
        int height = maze.GetLength(0);
        int width = maze.GetLength(1);

        for (int y = 1; y < height - 1; y += 2)
        {
            for (int x = 1; x < width - 1; x += 2)
            {
                if (maze[y, x] == 0)
                {
                    // Try to knock down wall between two paths
                    foreach ((int dx, int dy) in new[] { (1, 0), (0, 1) })
                    {
                        int nx = x + dx * 2, ny = y + dy * 2;
                        int wallX = x + dx, wallY = y + dy;

                        if (nx < width - 1 && ny < height - 1 && maze[ny, nx] == 0 && maze[wallY, wallX] == 1)
                        {
                            if (rand.NextDouble() < loopChance)
                                maze[wallY, wallX] = 0;
                        }
                    }
                }
            }
        }
    }

    private (int x, int y) FindFarthestEdge((int x, int y) start)
    {
        int height = maze.GetLength(0);
        int width = maze.GetLength(1);
        int[,] dist = new int[height, width];
        Queue<(int x, int y)> queue = new();
        bool[,] visited = new bool[height, width];

        queue.Enqueue((start.x, start.y));
        visited[start.y, start.x] = true;
        dist[start.y, start.x] = 0;

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        (int x, int y) farthest = (start.x, start.y);
        int maxDist = 0;

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dx[i], ny = y + dy[i];

                if (nx >= 0 && ny >= 0 && nx < width && ny < height && !visited[ny, nx] && maze[ny, nx] == 0)
                {
                    visited[ny, nx] = true;
                    dist[ny, nx] = dist[y, x] + 1;
                    queue.Enqueue((nx, ny));

                    if (dist[ny, nx] > maxDist)
                    {
                        maxDist = dist[ny, nx];
                        farthest = (nx, ny);
                    }
                }
            }
        }

        List<(int x, int y)> farth = new()
        {
            (farthest.x, 0), (farthest.x, height - 1),
            (0, farthest.y), (width - 1, farthest.y)
        };

        foreach (var (x, y) in farth)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                if (maze[Math.Clamp(y, 1, height - 2), Math.Clamp(x, 1, width - 2)] == 0)
                    return (x, y);
            }
        }

        return farthest;
    }

    public void SpawnPrefabAtExit()
    {
        Vector3 exitWorldPos = new Vector3(exit.x, 0f, exit.y);
        Vector3 entryOffsetInPrefab = new Vector3(-1f, 0f, 4f);
        Vector3 prefabOrigin = exitWorldPos - entryOffsetInPrefab;

        GameObject instance = Instantiate(ExtraroomPrefab, prefabOrigin, Quaternion.identity);
    }

    public void PlaceLightsInMaze()
    {
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);

        // Horizontal pass
        for (int y = 0; y < height; y++)
        {
            int counter = 0;
            for (int x = 0; x < width; x++)
            {
                if (maze[x, y] == 0)
                {
                    counter++;
                    if (counter % 2 == 0)
                    {
                        maze[x, y] = 5;
                    }
                }
                else
                {
                    counter = 0;
                }
            }
        }

        // Vertical pass
        for (int x = 0; x < width; x++)
        {
            int counter = 0;
            for (int y = 0; y < height; y++)
            {
                if (maze[x, y] == 0)
                {
                    counter++;
                    if (counter % 2 == 0)
                    {
                        maze[x, y] = 5;
                    }
                }
                else
                {
                    counter = 0;
                }
            }
        }
    }

    void SpawnMaze()
    {
        int height = maze.GetLength(0);
        int width = maze.GetLength(1);

        // Instantiate ground
        Vector3 groundPos = new Vector3((width - 1) * CellSize / 2f, -0.5f, (height - 1) * CellSize / 2f);
        GameObject ground = Instantiate(GroundPrefab, groundPos, Quaternion.identity);

        // Instantiate ceiling
        Vector3 ceilingPos = new Vector3((width - 1) * CellSize / 2f, 1.5f, (height - 1) * CellSize / 2f);
        GameObject ceiling = Instantiate(CeilingPrefab, ceilingPos, Quaternion.Euler(180f, 0f, 0f));

        Vector3 ceilingScale = ceiling.transform.localScale;
        ceiling.transform.localScale = new Vector3(
            ceilingScale.x * width * CellSize,
            ceilingScale.y,
            ceilingScale.z * height * CellSize
        );

        Vector3 groundScale = ground.transform.localScale;
        ground.transform.localScale = new Vector3(
            groundScale.x * width * CellSize,
            groundScale.y,
            groundScale.z * height * CellSize
        );


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (maze[y, x] == 3)
                {
                    Vector3 pos = new Vector3(x * CellSize, 0.5f, y * CellSize);
                    Instantiate(TransparentPrefab, pos, Quaternion.identity, transform);
                }
                else if (maze[y, x] == 4)
                {
                    Vector3 pos = new Vector3(x * CellSize, 0.5f, y * CellSize);
                    Instantiate(MirrorPrefab, pos, Quaternion.identity, transform);
                }
                else if (maze[y, x] == 2)
                {
                    Vector3 pos = new Vector3(x * CellSize, 0.5f, y * CellSize);
                    Instantiate(AbsorbPrefab, pos, Quaternion.identity, transform);
                }
                else if (maze[y, x] == 5)
                {
                    Vector3 lightPos = new Vector3(x * CellSize, 1.5f, y * CellSize);
                    Instantiate(LightPrefab, lightPos, Quaternion.Euler(90f, 0f, 0f), transform);

                }
                else if (maze[y, x] == 1)
                {
                    Vector3 pos = new Vector3(x * CellSize, 0.5f, y * CellSize);
                    Instantiate(WallPrefab, pos, Quaternion.identity, transform);
                }
                else if (maze[y, x] == 6)
                {
                    Vector3 pos = new Vector3(x * CellSize, 0.5f, y * CellSize);
                    Instantiate(ExitPrefab, pos, Quaternion.identity, transform);
                }
            }
        }
    }

    void SpawnCoin()
    {
        int height = maze.GetLength(0);
        int width = maze.GetLength(1);
        List<(int x, int y)> emptyCells = new();

        // Collect all empty path cells
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (maze[y, x] == 0 || maze[y, x] == 5)
                    emptyCells.Add((x, y));
            }
        }
        if (emptyCells.Count == 0)
            return;

        System.Random rand = new System.Random();
        var (cx, cy) = emptyCells[rand.Next(emptyCells.Count)];
        Vector3 pos = new Vector3(cx * CellSize, 0f, cy * CellSize);

        Instantiate(CoinPrefab, pos, CoinPrefab.transform.rotation, transform);
    }

    public void BuildEntranceRoom()
    {
        Vector2Int entranceVector = new Vector2Int(entrance.x, entrance.y);
        int roomSize = 3;
        Vector2Int dir = GetRoomOffsetDirection(entranceVector);
        Vector2Int roomCenter = entranceVector + dir * 2;

        int half = roomSize / 2;
        Vector3 pos = new Vector3(0, 0, 0);
        for (int dx = -half; dx <= half; dx++)
        {
            for (int dy = -half; dy <= half; dy++)
            {
                Vector2Int current = roomCenter + new Vector2Int(dx, dy);

                // Skip center for player space
                if (dx == 0 && dy == 0) continue;

                // Leave corridor hole facing maze entrance
                if (dx == -dir.x && dy == -dir.y) continue;

                pos = new Vector3(current.x * CellSize, 0.5f, current.y * CellSize);
                Instantiate(WallPrefab, pos, Quaternion.identity);
            }
        }

        Vector3 groundPos = new Vector3(roomCenter.x, -0.5f, roomCenter.y);
        GameObject ground = Instantiate(GroundPrefab, groundPos, Quaternion.identity);

        Vector3 originalScale = ground.transform.localScale;
        ground.transform.localScale = new Vector3(
            originalScale.x * roomSize * CellSize,
            originalScale.y,
            originalScale.z * roomSize * CellSize
        );

        Vector3 ceilingPos = new Vector3(roomCenter.x, 1.5f, roomCenter.y);
        GameObject ceiling = Instantiate(CeilingPrefab, ceilingPos, Quaternion.Euler(180f, 0f, 0f)); // Flip upside down

        // Scale ceiling to match room size
        Vector3 ceilingScale = ceiling.transform.localScale;
        ceiling.transform.localScale = new Vector3(
            ceilingScale.x * roomSize * CellSize,
            ceilingScale.y,
            ceilingScale.z * roomSize * CellSize
        );

        // Spawn player inside the room
        SpawnPlayer(roomCenter, entranceVector - roomCenter);
    }

    Vector2Int GetRoomOffsetDirection(Vector2Int entrance)
    {
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);

        if (entrance.x == 0) return new Vector2Int(-1, 0);
        if (entrance.x == width - 1) return new Vector2Int(1, 0);
        if (entrance.y == 0) return new Vector2Int(0, -1);
        if (entrance.y == height - 1) return new Vector2Int(0, 1);

        return Vector2Int.zero;
    }

    void SpawnPlayer(Vector2Int roomCell, Vector2Int lookDirection)
    {
        Vector3 playerPos = new Vector3(roomCell.x * CellSize, 0f, roomCell.y * CellSize);
        Player.transform.position = playerPos;

        Vector3 forward = new Vector3(lookDirection.x, 0, lookDirection.y);
        Player.transform.rotation = Quaternion.LookRotation(forward);
    }

    
    public void WriteIntArrayToFile()
    {
        string path = Path.Combine(Application.dataPath, "Data/maze.txt");

        using (StreamWriter writer = new StreamWriter(path))
        {
            int width = maze.GetLength(0);
            int height = maze.GetLength(1);

            for (int i = 0; i < width; i++)
            {
                string line = "";
                for (int j = 0; j < height; j++)
                {
                    line += maze[i, j].ToString();
                    if (j < height - 1) line += "\t";
                }
                writer.WriteLine(line);
            }
        }

        Debug.Log($"Array written to {path}");
    }

    int[,] ReadArrayFromFile(string path)
    {
        List<int[]> rows = new List<int[]>();

        foreach (var line in File.ReadAllLines(path))
        {
            string[] tokens = line.Split('\t');
            int[] intRow = new int[tokens.Length];
            for (int i = 0; i < tokens.Length; i++)
            {
                intRow[i] = int.Parse(tokens[i]);
            }
            rows.Add(intRow);
        }

        int rowCount = rows.Count;
        int colCount = rows[0].Length;
        int[,] array = new int[rowCount, colCount];

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < colCount; j++)
            {
                array[i, j] = rows[i][j];
            }
        }

        Debug.Log($"Array read from {path}");
        return array;
    }

    void PrintMaze()
    {
        string debugOutput = "Array contents:\n";
        int rows = maze.GetLength(0);
        int cols = maze.GetLength(1);
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                debugOutput += maze[i, j] + "\t";
            }
            debugOutput += "\n";
        }
        Debug.Log(debugOutput);
    }

    public (int x, int y) FindSingleBorderValue(int target)
    {
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);
        (int x, int y) foundPos = (-1, -1); 
        int count = 0;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                bool isBorder = i == 0 || i == width - 1 || j == 0 || j == height - 1;
                if (isBorder && maze[i, j] == target)
                {
                    count++;
                    if (count > 1)
                    {
                        return (-1, -1);
                    }
                    foundPos = (j, i);
                }
            }
        }

        return foundPos;
    }
}