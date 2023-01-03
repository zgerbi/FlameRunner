/*
 * Cells are used to hold basic x and y coordinates within the maze. They are also used
 * by the A* algorithm with the f, g, and parent variables to determine the shortest path to the target
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    // maze x coordinate
    public int x
    {
        get;
        set;
    }

    // maze y coordinate
    public int y
    {
        get;
        set;
    }

    //The value used to determine the direction of the pathfinding; a summation of g and h (manhattan)
    public int f
    {
        get;
        set;
    }

    //The distance from the start of the maze to this cell following the A* path
    public int g
    {
        get;
        set;
    }

    //the cell that comes before this cell in the path
    public Cell parent
    {
        get;
        set;
    }

    // constructor for x and y
    public Cell(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    // constructor for x, y, parent, and g
    public Cell(int x, int y, Cell parent, int g)
    {
        this.x = x;
        this.y = y;
        this.parent = parent;
        this.g = g;
    }

    // returns whether or not this cell has the same maze location as another Cell
    public bool equalsXY(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            Cell c = (Cell)obj;
            return (x == c.x) && (y == c.y);
        }
    }

    //sets the weight of the cell
    public void setF(Cell dest)
    {
        f = g + manhattan(dest);
    }

    //sets and returns the weight of a given cell
    public int getF(Cell dest)
    {
        setF(dest);
        return f;
    }

    /* The heuristic, h, for determining the distance of a cell from the goal.
    *   This is done simply by taking the magnitude of the horizontal distance to the goal, 
    *   and summing with the magnitude of the vertical distance to the goal.
    *   The true magnitude of the distance is not relevant, since diagonal moves are impossible.
    */
    private int manhattan(Cell dest)
    {
        return Mathf.Abs(x - dest.x) + Mathf.Abs(y - dest.y);
    }
}
