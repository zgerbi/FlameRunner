/*
 * All shortest-path algorithms used by the game are accessed here, including
 * Breadth-First Search, Depth-First Search, and A*
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Pathfinder
{
    private static Maze maze;               // reference to the maze
    
    // updates to the current maze state
    private static void UpdateMaze()
    {
        maze = MazeStatic.mazeGen.Maze;
    }

    // finds the shortest path from src to dest via a modified BFS, which also stores predecessors in order to return the path
    public static void BFS(out List<Cell> path, out int tilesChecked, Cell src, Cell dest)
    {
        UpdateMaze();

        Queue<Cell> q = new Queue<Cell>();                          // stores queue of tiles to search

        //an empty List to hold the path. This will be filled if a path to the dest is found, and left empty otherwise
        path = new List<Cell>();

        tilesChecked = 0;                                           // tracks unique tiles checked

        bool[,] visited = new bool[maze.Width, maze.Height];        // tracks visited tiles
        Cell[,] pred = new Cell[maze.Width, maze.Height];           // stores predecessors

        // resets arrays before pathfinding
        Cell empty = new Cell(-1, -1);
        for (int i = 0; i < maze.Width; i++)
        {
            for (int j = 0; j < maze.Height; j++)
            {
                visited[i,j] = false;
                pred[i,j] = empty;
            }
        }

        // starts at src
        visited[src.x, src.y] = true;
        q.Enqueue(src);

        bool found = false;

        // runs until the entire maze has been searched or until dest has been found
        while (q.Count != 0 && !found)
        {
            Cell u = q.Dequeue();                       // stores the first tile in the queue

            // iterates through adjacent floor tiles
            foreach (Cell c in adj(u))
            {
                int x = c.x;
                int y = c.y;

                // if unvisited, mark it as visited, store its predecessor, and add it to the queue
                if (visited[x, y] == false)
                {
                    tilesChecked++;
                    
                    visited[x, y] = true;
                    pred[x, y] = u;
                    q.Enqueue(c);

                    // if dest is found, break foreach and while loops
                    if (c.equalsXY(dest))
                    {
                        found = true;
                        break;
                    }
                }
            }
        }

        // backtracks through the predecessors of dest to create path
        if (found)
        {
            path.Add(dest);

            while (!pred[dest.x, dest.y].equalsXY(empty))
            {
                path.Add(pred[dest.x, dest.y]);
                dest = pred[dest.x, dest.y];
            }

            // reverse path and remove the first tile, which is the current position of the fire
            path.Reverse();
            path.RemoveAt(0);
        }
    }

    // finds the shortest path from src to dest via a modified DFS, which also stores predecessors in order to return the path
    public static void DFS(out List<Cell> path, out int tilesChecked, Cell src, Cell dest)
    {
        UpdateMaze();

        Stack<Cell> s = new Stack<Cell>();                          // stores stack of tiles to search

        //an empty List to hold the path. This will be filled if a path to the dest is found, and left empty otherwise
        path = new List<Cell>();

        tilesChecked = 0;                                           // tracks unique tiles checked

        bool[,] visited = new bool[maze.Width, maze.Height];        // tracks visited tiles
        Cell[,] pred = new Cell[maze.Width, maze.Height];           // stores predecessors

        // resets arrays before pathfinding
        Cell empty = new Cell(-1, -1);
        for (int i = 0; i < maze.Width; i++)
        {
            for (int j = 0; j < maze.Height; j++)
            {
                visited[i, j] = false;
                pred[i, j] = empty;
            }
        }

        // starts at src
        visited[src.x, src.y] = true;
        s.Push(src);

        bool found = false;

        // runs until the entire maze has been searched or until dest has been found
        while (s.Count != 0 && !found)
        {
            Cell u = s.Pop();                           // stores the first tile in the stack

            // iterates through adjacent floor tiles
            foreach (Cell c in adj(u))
            {
                int x = c.x;
                int y = c.y;

                // if unvisited, mark it as visited, store its predecessor, and add it to the stack
                if (visited[x, y] == false)
                {
                    tilesChecked++;

                    visited[x, y] = true;
                    pred[x, y] = u;
                    s.Push(c);

                    // if dest is found, break foreach and while loops
                    if (c.equalsXY(dest))
                    {
                        found = true;
                        break;
                    }
                }
            }
        }

        // backtracks through the predecessors of dest to create path
        if (found)
        {
            path.Add(dest);

            while (!pred[dest.x, dest.y].equalsXY(empty))
            {
                path.Add(pred[dest.x, dest.y]);
                dest = pred[dest.x, dest.y];
            }

            // reverse path and remove the first tile, which is the current position of the fire
            path.Reverse();
            path.RemoveAt(0);
        }
    }

    // returns a list of the adjacent floor tiles
    private static List<Cell> adj(Cell c)
    {
        List<Cell> neighbors = new List<Cell>();
        int x = c.x;
        int y = c.y;

        if (x > 0 && maze.Grid[x - 1, y].isFloor())                   // left
            neighbors.Add(new Cell(x - 1, y));

        if (y > 0 && maze.Grid[x, y - 1].isFloor())                   // down
            neighbors.Add(new Cell(x, y - 1));

        if (x < maze.Width - 1 && maze.Grid[x + 1, y].isFloor())      // right
            neighbors.Add(new Cell(x + 1, y));

        if (y < maze.Height - 1 && maze.Grid[x, y + 1].isFloor())     // up
            neighbors.Add(new Cell(x, y + 1));

        return neighbors;
    }

    public static void AStarPath(out List<Cell> path, out int tilesChecked, Cell src, Cell dest)
    {
        UpdateMaze();

        //an empty List to hold the path. This will be filled if a path to the dest is found, and left empty otherwise
        path = new List<Cell>();
        
        tilesChecked = 0;                                           // tracks unique tiles checked

        List<Cell> openList = new List<Cell>();		                //list of cells that need to be checked
        List<Cell> closedList = new List<Cell>();	                //list of cells that have been visited and checked

        src.g = 0;
        openList.Add(src);

        //while there are still viable, unchecked cells...
        while (openList.Any())
        {
            //minF is set to the cell on the open list with the lowest weight
            Cell minF = openList.OrderBy(temp => temp.getF(dest)).First();

            //checking if minF is the destination tile; if so, return the path
            if (minF.equalsXY(dest))
            {
                path = getRevPath(minF);
                path.RemoveAt(0);
                return;
            }
            else
            {
                //if not, move minF from the open list to the closed list since it has been checked
                closedList.Add(minF);
                openList.Remove(minF);

                //retrieve a list of all passable cells surrounding minF
                List<Cell> adj = AStarAdj(minF, dest);

                //checking each adjacent cell
                foreach (Cell curr in adj)
                {
                    //if it has already been visited, do nothing and check the next cell
                    if (closedList.Any(temp => temp.equalsXY(curr)))
                    {
                        continue;
                    }

                    //if not, the cell will be checked
                    tilesChecked++;

                    /* 	if there is a cell with the same position on the open list, check if curr has a lower weight.
                    *  	if it does, replace the cell on the open list with the cell on the adj list.
                    *	this updates the weight of a cell if a better path to that cell has been found.
                    */
                    if (openList.Any(temp => temp.equalsXY(curr)))
                    {
                        Cell swap = openList.First(temp => temp.equalsXY(curr));
                        if (swap.getF(dest) > minF.getF(dest))
                        {
                            openList.Remove(swap);
                            openList.Add(curr);
                        }
                    }
                    else	//if none of the above, add it to the tiles that need their surroundings checked
                    {
                        openList.Add(curr);
                    }
                }
            }
        }
    }

    //returns a list of all valid "walkable" adjacent tiles to curr and calculates their weights
    private static List<Cell> AStarAdj(Cell curr, Cell dest)
    {
        //initializing a list of four cells, adjacent to curr to the north, east, south, and west, and initializes their "g" values
        List<Cell> adj = new List<Cell>()
        {new Cell(curr.x, curr.y + 1, curr, curr.g + 1), new Cell(curr.x + 1, curr.y, curr, curr.g + 1), new Cell(curr.x, curr.y - 1, curr, curr.g + 1), new Cell(curr.x - 1, curr.y, curr, curr.g + 1), };
        
        //setting each of the weights of the cells in the list
        foreach (Cell cell in adj)
        {
            cell.setF(dest);
        }

        /*	checks that each cell on the adjacent list
         *		1. is within the horizontal bounds of the maze
         *		2. is within the vertical bounds of the maze
         *		3. is a "walkable" tile, A.K.A. is not a wall
         */
        int maxDim = maze.Width - 1;
        return adj.Where(cell => cell.x >= 0 && cell.x <= maxDim).Where(cell => cell.y >= 0 && cell.y <= maxDim).Where(cell => maze.Grid[cell.x, cell.y].isFloor() || (cell.x == dest.x && cell.y == dest.y)).ToList();
    }

    /*	
     *	Performs a reverse trace of the path, given that the algorithm has reached the destination.
     *	Each cell's parent is defined as the cell that precedes it on the path from the source to the destination,
     *	so these parents are pushed to a stack from last to first then pushed to the final List to reverse their order.
     */
    private static List<Cell> getRevPath(Cell curr)
    {
        Stack<Cell> s = new Stack<Cell>();
        List<Cell> ordered = new List<Cell>();
        Cell prev = curr;                           //prev will "walk" backwards up the path, from dest to src
        s.Push(prev);
        
        //stacking each of the parents
        for (int i = 0; i < curr.g; i++)
        {
            prev = prev.parent;
            s.Push(prev);
        }
        
        //reversing the stack
        while (s.Count > 0)
        {
            ordered.Add(s.Peek());
            s.Pop();
        }
        return ordered;
    }
}
