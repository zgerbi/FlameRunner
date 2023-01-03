/*
 * Handles all maze-related functions such as generating the maze, fire movement,
 * decrementing tiles and crumbling the lowest tiles
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Maze : MonoBehaviour
{
    int width;                  // width of maze
    int height;                 // height of maze
    int tileCounter;            // lifespan of tiles when placed
    int gameSpeed;              // stores selected game speed
    Color pathColor;            // stores current path color

    MazeTile[,] grid;           // 2D array of MazeTiles which make up the maze

    System.Random rg;           // random number generator for generating the maze
    MazeTile prefab;            // prefab of a MazeTile

    Cell target;                // stores current location of target
    Cell fire;                  // stores current location of fire

    // get reference to current maze width
    public int Width
    {
        get { return width; }
    }

    // get reference to current maze height
    public int Height
    {
        get { return height; }
    }
    
    // get reference to current grid of MazeTiles
    public MazeTile[,] Grid
    {
        get { return grid; }
    }

    // get reference to current target location
    public Cell Target
    {
        get { return target; }
    }

    // get reference to curren fire location
    public Cell Fire
    {
        get { return fire; }
    }

    // constructor for a new maze
    public Maze(int width, int height, int tileCounter, int gameSpeed, Color pathColor, System.Random rg, MazeTile prefab)
    {
        this.width = width;
        this.height = height;
        this.tileCounter = tileCounter;
        this.gameSpeed = gameSpeed;
        this.pathColor = pathColor;

        this.rg = rg;
        this.prefab = prefab;
    }

    // generates new maze and sets location of target and fire
    public void Generate()
    {
        grid = new MazeTile[width, height];

        // instantiates MazeTiles which appropriate settings and position
        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                Vector3 pos = new Vector3(x - width / 2, y - height / 2);

                MazeTile tile = Instantiate(prefab, pos, Quaternion.identity);
                tile.setup(y, tileCounter, gameSpeed, pathColor);

                grid[x, y] = tile;                          // stores new MazeTile in the grid
            }
        }

        // sets target at the center of the maze
        target = new Cell(width / 2, height / 2);
        grid[target.x, target.y].setFloor();
        grid[target.x, target.y].setTarget(true);

        // recursively generates maze using depth-first traversal starting at target
        MazeDigger(target.x, target.y);

        // randomly sets the fire at one of the 4 corners of the maze, which are guaranteed to all be floors
        int corner = Random.Range(1, 5);

        if (corner == 1)
            fire = new Cell(0, 0);

        else if (corner == 2)
            fire = new Cell(width - 1, 0);

        else if (corner == 3)
            fire = new Cell(0, height - 1);

        else
            fire = new Cell(width - 1, height - 1);

        grid[fire.x, fire.y].setFire(true);
    }

    // recursively sets MazeTiles to floors via a modified depth-first traversal to create the maze
    void MazeDigger(int x, int y)
    {
        // randomly shuffles directions in which the path will go
        int[] directions = new int[] { 1, 2, 3, 4 };
        Tools.Shuffle(directions, rg);

        for (int i = 0; i < directions.Length; i++)
        {
            if (directions[i] == 1)                             // down 2 tiles
            {
                if (y - 2 < 0)
                    continue;

                // if unvisited, set next 2 tiles to floors and continue digging the maze from new position
                if (grid[x, y - 2].isFloor() == false)
                {
                    grid[x, y - 2].setFloor();
                    grid[x, y - 1].setFloor();

                    MazeDigger(x, y - 2);
                }
            }

            if (directions[i] == 2)                             // left 2 tiles
            {
                if (x - 2 < 0)
                    continue;

                // if unvisited, set next 2 tiles to floors and continue digging the maze from new position
                if (grid[x - 2, y].isFloor() == false)
                {
                    grid[x - 2, y].setFloor();
                    grid[x - 1, y].setFloor();

                    MazeDigger(x - 2, y);
                }
            }

            if (directions[i] == 3)                             // right 2 tiles
            {
                if (x + 2 >= width)
                    continue;

                // if unvisited, set next 2 tiles to floors and continue digging the maze from new position
                if (grid[x + 2, y].isFloor() == false)
                {
                    grid[x + 2, y].setFloor();
                    grid[x + 1, y].setFloor();

                    MazeDigger(x + 2, y);
                }
            }

            if (directions[i] == 4)                             // up 2 tiles
            {
                if (y + 2 >= height)
                    continue;

                // if unvisited, set next 2 tiles to floors and continue digging the maze from new position
                if (grid[x, y + 2].isFloor() == false)
                {
                    grid[x, y + 2].setFloor();
                    grid[x, y + 1].setFloor();

                    MazeDigger(x, y + 2);
                }
            }
        }
    }

    // set fire to new location
    public void MoveFire(Cell dest)
    {
        grid[fire.x, fire.y].setFire(false);
        grid[dest.x, dest.y].setFire(true);
        fire = dest;
    }

    // decrement the counter on all placed tiles
    public void DecrementTiles()
    {
        foreach (MazeTile tile in grid)
        {
            if (!tile.isFloor() && tile.isLocked())     // if tile is a wall and has been placed
                tile.decrementCounter();
        }
    }

    // crumble tiles with the lowest value
    public void CrumbleLowest()
    {
        int minCounter = tileCounter;

        // find minimum value for placed tiles
        foreach (MazeTile tile in grid)
        {
            if (!tile.isFloor() && tile.isLocked() && tile.getCounter() < minCounter)
                minCounter = tile.getCounter();
        }

        // crumble tiles with the minimum value
        foreach (MazeTile tile in grid)
        {
            if (!tile.isFloor() && tile.isLocked() && tile.getCounter() == minCounter)
                tile.crumble();
        }
    }
}