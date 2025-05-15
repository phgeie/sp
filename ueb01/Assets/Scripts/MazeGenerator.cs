using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MazeGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject ExtraroomPrefab;
    public GameObject WallPrefab;
    public GameObject MirrorPrefab;
    public GameObject TransparentPrefab;
    public GameObject GroundPrefab;
    public GameObject CeilingPrefab;
    public GameObject CoinPrefab;
    public GameObject ExitPrefab;
    public GameObject LightPrefab;
    public GameObject Player;

    [Header("Maze Settings")]
    private int MazeWidth = 10;
    private int MazeHeight = 10;
    private float CellSize = 1f;

    private int[,] maze;

    private (int x, int y) entrance;

    void Start()
    {
        GenerateMaze(MazeWidth, MazeHeight);
        PlaceLightsInMaze();
        ReplaceSomeWalls();
        SpawnMaze();
        PrintMaze();
        SpawnCoin();
        BuildEntranceRoom(new Vector2Int(entrance.x, entrance.y), 3, WallPrefab);

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
        AddLoops(rand, 0.05f); // 5% chance to break extra wall


        // Pick exit 
        // Set exit at bottom-right
(int x, int y) exit = (width - 1, height - 2);

// Ensure adjacent cell is path
if (maze[exit.y, exit.x - 1] == 0 || maze[exit.y - 1, exit.x] == 0)
{
    maze[exit.y, exit.x] = 2;
}
else
{
    // Carve a path if not already connected
    maze[height - 2, width - 2] = 0;
    maze[height - 2, width - 1] = 2;
}


entrance = FindFarthestEdge(exit);
maze[entrance.y, entrance.x] = 0;

SpawnPrefabAtExit(exit);
    }

    public void ReplaceSomeWalls()
    {
        float mirrorChance = 0.1f;
        float transparentChance = 0.05f;
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
                    maze[x, y] = 4; // transparent wall
                }
                else if (rng.NextDouble() < mirrorChance)
                {
                    maze[x, y] = 3; // mirror wall
                }
            }
        }
    }

    private void AddLoops(System.Random rand, float loopChance)
    {
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

    private (int x, int y) PickRandomEdge(System.Random rand)
    {
        int height = maze.GetLength(0);
        int width = maze.GetLength(1);
        List<(int x, int y)> candidates = new();

        for (int x = 1; x < width - 1; x += 2)
        {
            if (maze[1, x] == 0) candidates.Add((x, 0));             // top
            if (maze[height - 2, x] == 0) candidates.Add((x, height - 1)); // bottom
        }

        for (int y = 1; y < height - 1; y += 2)
        {
            if (maze[y, 1] == 0) candidates.Add((0, y));             // left
            if (maze[y, width - 2] == 0) candidates.Add((width - 1, y)); // right
        }

        return candidates[rand.Next(candidates.Count)];
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

        // Snap farthest cell to nearest outer wall
        List<(int x, int y)> exits = new()
        {
            (farthest.x, 0), (farthest.x, height - 1),
            (0, farthest.y), (width - 1, farthest.y)
        };

        foreach (var (x, y) in exits)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
            {
                if (maze[Math.Clamp(y, 1, height - 2), Math.Clamp(x, 1, width - 2)] == 0)
                    return (x, y);
            }
        }

        return farthest; // fallback
    }

public void SpawnPrefabAtExit((int x, int y) exit)
{
    Vector3 exitWorldPos = new Vector3(exit.x, 0f, exit.y);
    Vector3 entryOffsetInPrefab = new Vector3(-1f, 0f, 4f); // prefab entry relative to its origin
    Vector3 prefabOrigin = exitWorldPos - entryOffsetInPrefab;

    GameObject instance = Instantiate(ExtraroomPrefab, prefabOrigin, Quaternion.identity);
}


    public void PrintMaze()
    {
        int height = maze.GetLength(0);
        int width = maze.GetLength(1);
        string s = "";

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                if (maze[y, x] == 1)
                    s += "<";
                else if (maze[y, x] == 5)
                {
                    s += "-";
                }
                else if (maze[y, x] == 0)
                {
                    s += "0";
                }
                else if (maze[y, x] == 3)
                {
                    s += "M";
                }
                else if (maze[y, x] == 4)
                {
                    s += "T";
                }
                else
                {
                    s += "E";

                }
            s += "\n";
        }
        Debug.Log(s);
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
        Vector3 ceilingPos = new Vector3((width - 1) * CellSize / 2f, 1.5f, (height - 1) * CellSize / 2f); // Y = height above walls
        GameObject ceiling = Instantiate(CeilingPrefab, ceilingPos, Quaternion.Euler(180f, 0f, 0f)); // Flip upside down

        Vector3 ceilingScale = ceiling.transform.localScale;
        ceiling.transform.localScale = new Vector3(
            ceilingScale.x * width * CellSize,
            ceilingScale.y,
            ceilingScale.z * height * CellSize
        );

        // Multiply the original prefab's localScale
        Vector3 originalScale = ground.transform.localScale;
        ground.transform.localScale = new Vector3(
            originalScale.x * width * CellSize,
            originalScale.y,
            originalScale.z * height * CellSize
        );


        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (maze[y, x] == 4)
                {
                    Vector3 pos = new Vector3(x * CellSize, 0.5f, y * CellSize);
                    Instantiate(TransparentPrefab, pos, Quaternion.identity, transform);
                }
                else if (maze[y, x] == 3)
                {
                    Vector3 pos = new Vector3(x * CellSize, 0.5f, y * CellSize);
                    Instantiate(MirrorPrefab, pos, Quaternion.identity, transform);
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
                else if (maze[y, x] == 2)
                {
                    Vector3 pos = new Vector3(x * CellSize, 0.5f, y * CellSize);

                    Vector3 mazeCenter = new Vector3((width - 1) * CellSize / 2f, 0f, (height - 1) * CellSize / 2f);
                    Vector3 dir = (mazeCenter - pos).normalized;

                    Quaternion rotation;

                    // Snap direction to cardinal axis
                    if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
                    {
                        rotation = dir.x > 0 ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, -90, 0); // East or West
                    }
                    else
                    {
                        rotation = dir.z > 0 ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0); // North or South
                    }

                    Instantiate(ExitPrefab, pos, rotation, transform);

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
        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                if (maze[y, x] == 0)
                    emptyCells.Add((x, y));
            }
        }

        if (emptyCells.Count == 0)
            return;

        // Randomly choose one
        System.Random rand = new System.Random();
        var (cx, cy) = emptyCells[rand.Next(emptyCells.Count)];
        Vector3 pos = new Vector3(cx * CellSize, 0f, cy * CellSize);

        Instantiate(CoinPrefab, pos, CoinPrefab.transform.rotation, transform);
    }


    public void BuildEntranceRoom(Vector2Int entranceVector, int roomSize, GameObject wallPrefab)
    {
        Vector2Int dir = GetRoomOffsetDirection(entranceVector); // Direction facing maze
        Vector2Int roomCenter = entranceVector + dir * 2;         // One tile further from entrance

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
                Instantiate(wallPrefab, pos, Quaternion.identity);
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

        return Vector2Int.zero; // fallback
    }


    void SpawnPlayer(Vector2Int roomCell, Vector2Int lookDirection)
    {
        Vector3 playerPos = new Vector3(roomCell.x * CellSize, 0f, roomCell.y * CellSize);
        Player.transform.position = playerPos;

        Vector3 forward = new Vector3(lookDirection.x, 0, lookDirection.y);
        Player.transform.rotation = Quaternion.LookRotation(forward);
    }




}
