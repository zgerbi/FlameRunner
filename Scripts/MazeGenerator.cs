/*
 * Manages all aspects of running the game including resetting the maze, updating preferences,
 * using and processing pathfinding algorithms, and distributing statistics
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MazeGenerator : MonoBehaviour
{
    private System.Random mazeRG;
    private Maze maze;                                      // stores current maze

    public int mazeWidth;                                   // current maze width
    public int mazeHeight;                                  // current maze height
    public int tileCounter;                                 // lifespan of tiles when placed

    public MazeTile mazeTilePrefab;
    public Camera cam;

    private int stepCounter;                                // total steps taken during session
    [SerializeField] private TextMeshProUGUI textMesh;

    IEnumerator advance;                                    // coroutine for advancing the fire
    IEnumerator gameOver;                                   // coroutine for ending the game
    IEnumerator cascade;                                    // coroutine for cascading explosion after losing

    private int algorithm;                                  // stores selected pathfinding algorithm
    private int gameSpeed;                                  // stores selected game speed
    [SerializeField] private float FireTick;                // speed of fire

    [SerializeField] private GameObject tutorialPopup;      // displays tutorial information

    [SerializeField] private GameObject statTemplate;       // template for a row in the right-side stats feed

    [SerializeField] private GameObject statsPopup;         // displays statistical summary
    private Graph pathLengthGraph;                          // line graph of path lengths vs steps
    private Graph tilesCheckedGraph;                        // line graph of unique tiles checked vs steps
    private Graph calculationTimeGraph;                     // line graph of calculation time vs steps

    // keeps track of all statistics
    private List<Stat> stats;
    private List<float> calculationTimes;
    private List<int> pathLengths;
    private List<int> tileChecks;

    [SerializeField] private GameObject preferences;        // displays dropdowns for game preferences
    private TMP_Dropdown mazeSizeDD;                        // dropdown for selecting maze size
    private TMP_Dropdown algorithmDD;                       // dropdown for selecting pathfinding algorithm
    private TMP_Dropdown gameSpeedDD;                       // dropdown for selecting game speed

    [SerializeField] private Color slowFirePath;            // color of slow fire path
    [SerializeField] private Color fastFirePath;            // color of fast fire path
    [SerializeField] private Color turboFirePath;           // color of turbo fire path
    private Color pathColor;                                // stores current path color

    // get reference to current maze
    public Maze Maze
    {
        get { return maze; }
    }

    // prepares the game when it first starts up
    private void Awake()
    {
        // reset statistic lists
        stats = new List<Stat>();
        calculationTimes = new List<float>();
        pathLengths = new List<int>();
        tileChecks = new List<int>();

        // display tutorial, hide statistical summary
        tutorialPopup.SetActive(true);
        statsPopup.SetActive(false);

        // get references to line graphs
        pathLengthGraph = statsPopup.transform.Find("Path Length Graph").GetComponent<Graph>();
        tilesCheckedGraph = statsPopup.transform.Find("Tiles Checked Graph").GetComponent<Graph>();
        calculationTimeGraph = statsPopup.transform.Find("Calculation Time Graph").GetComponent<Graph>();

        // reset line graphs
        pathLengthGraph.Setup();
        tilesCheckedGraph.Setup();
        calculationTimeGraph.Setup();

        // get references to preference dropdowns
        mazeSizeDD = preferences.transform.Find("Maze Size").GetComponent<TMP_Dropdown>();
        algorithmDD = preferences.transform.Find("Algorithm").GetComponent<TMP_Dropdown>();
        gameSpeedDD = preferences.transform.Find("Game Speed").GetComponent<TMP_Dropdown>();
    }

    // starts a new session, resetting any currently running processes, generating a new maze, and starting the fire movement
    public void StartGame()
    {
        // stop any currently running coroutines
        if (advance != null)
            StopCoroutine(advance);
        if (gameOver != null)
            StopCoroutine(gameOver);
        if (cascade != null)
            StopCoroutine(cascade);

        UpdatePreferences();                                            // get selected preferences

        stepCounter = 0;                                                // reset steps
        textMesh.text = stepCounter.ToString();

        cam.orthographicSize = (mazeHeight + 1) / 2;                    // zoom in or out to show whole maze

        tutorialPopup.SetActive(false);                                 // hide tutorial
        statsPopup.SetActive(false);                                    // hide statistical summary

        GenerateMaze();                                                 // generate new maze
        GetPath(out _);                                                 // draw path from fire to target

        // delete previous stats in right-side feed
        foreach (Transform child in statTemplate.transform.parent)
            if (child.gameObject.activeSelf)
                Destroy(child.gameObject);

        // clear previous statistics
        stats.Clear();
        calculationTimes.Clear();
        pathLengths.Clear();
        tileChecks.Clear();

        // start coroutine for advancing the fire
        advance = Advance();
        StartCoroutine(advance);
    }

    // destroys previous maze and generates a new one
    private void GenerateMaze()
    {
        MazeStatic.mazeGen = this;
        
        mazeRG = new System.Random((int) System.DateTime.Now.Ticks);

        // destroys previous MazeTiles
        foreach (GameObject tile in GameObject.FindGameObjectsWithTag("Tile"))
            Destroy(tile);

        // generates new maze
        maze = new Maze(mazeWidth, mazeHeight, tileCounter, gameSpeed, pathColor, mazeRG, mazeTilePrefab);
        maze.Generate();
    }

    // uses appropriate pathfinding algorithm to find a path to the target, then logs the statistics of running the algorithm
    private void GetPath(out List<Cell> path)
    {
        int tilesChecked;
        
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();        // timer for calculation time

        stopwatch.Start();

        // runs selected algorithm
        if (algorithm == 0)
            Pathfinder.BFS(out path, out tilesChecked, maze.Fire, maze.Target);             // breadth-first search
        else if (algorithm == 1)
            Pathfinder.DFS(out path, out tilesChecked, maze.Fire, maze.Target);             // depth-first search
        else
            Pathfinder.AStarPath(out path, out tilesChecked, maze.Fire, maze.Target);       // A*


        stopwatch.Stop();

        System.TimeSpan timespan = stopwatch.Elapsed;
        float milliseconds = (float)timespan.TotalMilliseconds;                             // gets calculation time

        logStat(milliseconds, path.Count, tilesChecked);                                    // log new statistics

        DrawPath(ref path);                                                                 // draw new path
    }

    // clears previous path and draws new path
    private void DrawPath(ref List<Cell> path)
    {
        // clear previous path
        for (int i = 0; i < maze.Width; i++)
        {
            for (int j = 0; j < maze.Height; j++)
            {
                maze.Grid[i, j].clearPath();
            }
        }

        // draw new path
        for (int i = 0; i < path.Count; i++)
            maze.Grid[path[i].x, path[i].y].setPath(i, ref path);
    }

    // handles fire movement including processing path navigation, tile crumbling, and end conditions
    private IEnumerator Advance()
    {
        List<Cell> path = new List<Cell>();

        // runs until the fire reaches the target
        while (!maze.Fire.equalsXY(maze.Target))
        {
            yield return new WaitForSeconds(FireTick);                  // waits the appropriate amount of time between steps

            // necessary for DFS so that a new path is only calculated if the current path becomes obstructed
            bool obstructed = false;
            foreach (Cell c in path)
            {
                if (!maze.Grid[c.x, c.y].isFloor())
                {
                    obstructed = true;
                    break;
                }
            }

            // if no path is found, not running DFS, or current path is obstructed, get a new path
            if (path.Count == 0 || algorithm != 1 || obstructed)
                GetPath(out path);
            // otherwise move the fire to the next step in the current path
            else
            {
                path.RemoveAt(0);
                DrawPath(ref path);
            }

            // if a path was found, move the fire along that path, increment the step counter, and decrement all MazeTiles
            if (path.Count > 0)
            {
                maze.MoveFire(path[0]);

                stepCounter++;
                textMesh.text = stepCounter.ToString();

                maze.DecrementTiles();
            }
            // otherwise if no path is found, crumble the lowest value tiles
            else
            {
                maze.CrumbleLowest();
            }
        }

        // start coroutine for ending the game
        gameOver = GameOver();
        StartCoroutine(gameOver);
    }

    // ends the game by triggering cascading explosions then displaying the statistical summary
    private IEnumerator GameOver()
    {
        cascade = CascadeExplosions();                                          // triggers cascading explosions
        yield return StartCoroutine(cascade);

        // display appropriate data in line graphs
        pathLengthGraph.ShowGraph(pathLengths);
        tilesCheckedGraph.ShowGraph(tileChecks);
        calculationTimeGraph.ShowGraph(calculationTimes);

        // get median and IQR values for statistics
        ResetStats();

        CanvasGroup popupCanvas = statsPopup.GetComponent<CanvasGroup>();
        popupCanvas.alpha = 0;
        statsPopup.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        // fade in statistical summary after cascading explosions
        while (popupCanvas.alpha < 1)
        {
            popupCanvas.alpha += 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    // sets fire to the whole maze in a cascading fashion starting from the center
    private IEnumerator CascadeExplosions()
    {
        Queue<Cell> q = new Queue<Cell>();                                                  // stores tiles to visit next
        bool[,] visited = new bool[maze.Width, maze.Height];                                // tracks which tiles have already been visited

        q.Enqueue(maze.Fire);                                                               // start at fire's current position

        // triggers barrel exploding animation
        maze.Grid[maze.Fire.x, maze.Fire.y].setFire(false);
        maze.Grid[maze.Target.x, maze.Target.y].targetAnimator.SetBool("explode", true);

        yield return new WaitForSeconds(0.5f);

        // displays barrel explosion above any nearby sprites
        Vector3 targetPos = maze.Grid[maze.Target.x, maze.Target.y].transform.position;
        targetPos.z -= 1;
        maze.Grid[maze.Target.x, maze.Target.y].transform.position = targetPos;

        // runs until the entire maze is covered
        while (q.Count > 0)
        {
            Queue<Cell> prev = new Queue<Cell>(q);
            
            // sets fire to current queue of tiles and crumbles any walls in its path
            while (q.Count > 0)
            {
                Cell curr = q.Dequeue();

                maze.Grid[curr.x, curr.y].setFire(true);

                if (!maze.Grid[curr.x, curr.y].isFloor())
                {
                    maze.Grid[curr.x, curr.y].activate();
                    maze.Grid[curr.x, curr.y].crumble();
                }

                visited[curr.x, curr.y] = true;                                             // mark these tiles as visited
            }

            yield return new WaitForSeconds(0.1f);

            q = CascadeHelper(prev, ref visited);                                           // get next batch of tiles to explode
        }
    }

    // returns the next batch of adjacent tiles
    private Queue<Cell> CascadeHelper(Queue<Cell> q, ref bool[,] visited)
    {
        Queue<Cell> res = new Queue<Cell>();

        // removes fire for current queue of tiles while adding unvisited, adjacent tiles to res
        while (q.Count > 0)
        {
            Cell curr = q.Dequeue();

            maze.Grid[curr.x, curr.y].setFire(false);

            // checks for unvisited adjacent tiles
            foreach (Cell c in adjTiles(curr))
            {
                if (!visited[c.x, c.y])
                {
                    res.Enqueue(c);
                    visited[c.x, c.y] = true;
                }
            }
        }

        return res;
    }

    // returns adjacent tiles in 4 directions
    private List<Cell> adjTiles(Cell c)
    {
        List<Cell> neighbors = new List<Cell>();
        int x = c.x;
        int y = c.y;

        if (x > 0)                                  // left
            neighbors.Add(new Cell(x - 1, y));

        if (y > 0)                                  // down
            neighbors.Add(new Cell(x, y - 1));

        if (x < mazeWidth - 1)                      // right
            neighbors.Add(new Cell(x + 1, y));

        if (y < mazeHeight - 1)                     // up
            neighbors.Add(new Cell(x, y + 1));

        return neighbors;
    }

    // sets median and IQR values for statistical summary
    private void ResetStats()
    {
        // sorts all statistic lists
        pathLengths.Sort();
        tileChecks.Sort();
        calculationTimes.Sort();

        // sets median and IQR for path lengths
        Transform pathLengthStats = statsPopup.transform.Find("Path Length Stats");
        pathLengthStats.Find("Median").GetComponent<TextMeshProUGUI>().text = pathLengths[pathLengths.Count / 2].ToString();
        pathLengthStats.Find("IQR").GetComponent<TextMeshProUGUI>().text = (pathLengths[pathLengths.Count * 3 / 4] - pathLengths[pathLengths.Count / 4]).ToString();

        // sets median and IQR for tiles checked
        Transform tilesCheckedStats = statsPopup.transform.Find("Tiles Checked Stats");
        tilesCheckedStats.Find("Median").GetComponent<TextMeshProUGUI>().text = tileChecks[tileChecks.Count / 2].ToString();
        tilesCheckedStats.Find("IQR").GetComponent<TextMeshProUGUI>().text = (tileChecks[tileChecks.Count * 3 / 4] - tileChecks[tileChecks.Count / 4]).ToString();

        // sets median and IQR for calclation times
        Transform calculationTimeStats = statsPopup.transform.Find("Calculation Time Stats");
        calculationTimeStats.Find("Median").GetComponent<TextMeshProUGUI>().text = calculationTimes[calculationTimes.Count / 2].ToString("0.000");
        calculationTimeStats.Find("IQR").GetComponent<TextMeshProUGUI>().text = (calculationTimes[calculationTimes.Count * 3 / 4] - calculationTimes[calculationTimes.Count / 4]).ToString("0.000");
    }

    // toggles visibility of tutorial
    public void TutorialVisible()
    {
        tutorialPopup.SetActive(!tutorialPopup.activeSelf);
    }

    // updates selected preferences
    private void UpdatePreferences()
    {
        // sets maze size
        int size = 0;
        if (mazeSizeDD.value == 0)
            size = 13;
        else if (mazeSizeDD.value == 1)
            size = 25;
        else if (mazeSizeDD.value == 2)
            size = 37;

        mazeWidth = size;
        mazeHeight = size;

        // sets pathfinding algorithm
        algorithm = algorithmDD.value;

        // sets game speed, which dictates fire speed and path color
        if (gameSpeedDD.value == 0)
        {
            FireTick = 0.8f;
            pathColor = slowFirePath;
        }
        else if (gameSpeedDD.value == 1)
        {
            FireTick = 0.4f;
            pathColor = fastFirePath;
        }
        else if (gameSpeedDD.value == 2)
        {
            FireTick = 0.1f;
            pathColor = turboFirePath;
        }

        gameSpeed = gameSpeedDD.value;
    }

    // logs a new statistic, adding it to the right-side feed as well as statistic lists for the statistical summary at the end
    private void logStat(float calculationTime, int pathLength, int tilesChecked)
    {
        // adds stat to the feed
        GameObject newStat = Instantiate(statTemplate, statTemplate.transform.parent);
        newStat.SetActive(true);
        newStat.GetComponent<Stat>().Setup(calculationTime, pathLength, tilesChecked);

        // adds stat to lists
        stats.Add(newStat.GetComponent<Stat>());
        calculationTimes.Add(calculationTime);
        pathLengths.Add(pathLength);
        tileChecks.Add(tilesChecked);
    }
}
