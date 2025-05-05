using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MazeGenerator:MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject WallPrefab;
    public GameObject GroundPrefab;
    public GameObject CoinPrefab;

    [Header("Maze Settings")]
    public int MazeWidth = 21;
    public int MazeHeight = 21;
    public float CellSize = 1f;

    void Start()
    {
        int[,] maze = GenerateMaze(MazeWidth, MazeHeight);
        PrintMaze(maze);
        SpawnMaze(maze);
        SpawnCoin(maze);

    }

    public int[,] GenerateMaze(int width, int height)
    {
        if (width % 2 == 0) width++;
        if (height % 2 == 0) height++;

        int[,] maze = new int[height, width];

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
        AddLoops(maze, rand, 0.05f); // 5% chance to break extra wall

        // Pick entrance
        var entrance = PickRandomEdge(maze, rand);
        maze[entrance.y, entrance.x] = 0;

        // Pick exit (farthest)
        var exit = FindFarthestEdge(maze, entrance);
        maze[exit.y, exit.x] = 0;

        return maze;
    }

    private void AddLoops(int[,] maze, System.Random rand, float loopChance)
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

    private (int x, int y) PickRandomEdge(int[,] maze, System.Random rand)
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

    private (int x, int y) FindFarthestEdge(int[,] maze, (int x, int y) start)
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

    public void PrintMaze(int[,] maze)
    {
        int height = maze.GetLength(0);
        int width = maze.GetLength(1);
        string s = "";

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
                if(maze[y, x] == 1)
                    s += "<";
                else
                    s += "0";
                s += "\n";
        }
        Debug.Log(s);
    }

    
    void SpawnMaze(int[,] maze)
    {
        int height = maze.GetLength(0);
        int width = maze.GetLength(1);

        // Instantiate ground
        Vector3 groundPos = new Vector3((width - 1) * CellSize / 2f, -0.5f, (height - 1) * CellSize / 2f);
        GameObject ground = Instantiate(GroundPrefab, groundPos, Quaternion.identity);

        // Multiply the original prefab's localScale
        Vector3 originalScale = ground.transform.localScale;
        ground.transform.localScale = new Vector3(
            originalScale.x * width * CellSize,
            originalScale.y,
            originalScale.z * height * CellSize
        );

        // Spawn Walls
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (maze[y, x] == 1)
                {
                    Vector3 pos = new Vector3(x * CellSize, 0.5f, y * CellSize);
                    Instantiate(WallPrefab, pos, Quaternion.identity, transform);
                }
            }
        }
    }

    void SpawnCoin(int[,] maze)
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
    Vector3 pos = new Vector3(cx * CellSize, 0.5f, cy * CellSize);

    Instantiate(CoinPrefab, pos, CoinPrefab.transform.rotation, transform);
}

}
