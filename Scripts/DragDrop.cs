/*
 * When a MazeTile is selected, DragDrop allows the tile to be dragged around to other
 * valid tiles to then be dropped and placed. When a tile is dragged, a translucent
 * drag tile that does not affect the game follows the mouse and when the player releases,
 * the original tile wall is removed and a new wall is activated in the drag tile's position.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragDrop : MonoBehaviour
{
    public int dragRadius;                                  // distance the tile can be dragged
    
    private bool isDragged = false;                         // whether or not the tile is being dragged
    private Vector3 mouseDragStartPos;                      // dragging mouse start position
    private Vector3 spriteDragStartPos;                     // dragging sprite start position

    private SpriteRenderer dragRender;                      // reference to sprite renderer of drag tile

    [SerializeField] private Color tileTint;                // tint of original tile when selected
    [SerializeField] private Color regionTint;              // tint of valid drag region

    [SerializeField] private GameObject dragDropPrefab;     // prefab of drag tile

    private Maze maze;                                      // reference to maze
    private MazeTile tile;                                  // original tile
    private GameObject dragTile;                            // drag tile
    private LineRenderer line;                              // outline highlighting valid drag region

    // triggers when the MazeTile is first instantiated, setting up default values
    private void Awake()
    {
        // get references to the original tile and the maze
        tile = GetComponent<MazeTile>();
        maze = MazeStatic.mazeGen.Maze;

        // get reference to line renderer and hide the line
        line = transform.Find("Drag Highlight").GetComponent<LineRenderer>();
        line.enabled = false;
    }

    // triggers when the MazeTile is intially selected, a drag tile is created and the drag process is started
    private void OnMouseDown()
    {
        // only triggers for walls that have not been placed yet
        if (!tile.isFloor() && !tile.isLocked())
        {
            // starts drag process and gets start positions of the mouse and sprite
            isDragged = true;
            mouseDragStartPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            spriteDragStartPos = transform.position;

            // instantiates and sets up the drag tile
            dragTile = Instantiate(dragDropPrefab, transform.position, Quaternion.identity);
            dragRender = dragTile.GetComponent<SpriteRenderer>();
            dragTile.transform.position = new Vector3(transform.position.x, transform.position.y, tile.transform.position.z - 0.5f);

            // colors the original tile and displays the drag region outline
            tile.wallRender.color = tileTint;
            line.enabled = true;
        }
    }

    // triggers when the tile is being dragged, the position of the drag tile is updated to the nearest open tile
    private void OnMouseDrag()
    {
        if (isDragged)
        {
            Vector3 newPos = nearestFloor(Camera.main.ScreenToWorldPoint(Input.mousePosition));     // find nearest open tile to mouse position

            newPos.z = newPos.y + maze.Height / 2 - 0.5f;                                           // sets new drag tile position
            dragTile.transform.position = newPos;
        }
    }

    // triggers when the tile is released, destroys the drag tile and activates the new wall if necessary
    private void OnMouseUp()
    {
        if (isDragged)
        {
            isDragged = false;                                                  // resets values
            tile.wallRender.color = Color.white;

            Vector2 dragTilePos = dragTile.transform.position;                  // gets current drag tile position

            // only activates new tile is the drag tile is not in the original position
            if (dragTilePos != (Vector2)spriteDragStartPos)
            {
                Vector2 dragTarget = pos2Maze(dragTilePos);                     // gets maze coordinates of drag tile

                tile.setFloor();                                                // sets original tile to floor
                maze.Grid[(int)dragTarget.x, (int)dragTarget.y].activate();     // activates new wall
            }
            
            Destroy(dragTile);                                                  // destroys drag tile and hides drag region outline
            line.enabled = false;
        }
    }

    // rounds x and y values of a Vector3 to the nearest integer
    private Vector3 roundVector(Vector3 pos)
    {
        pos.x = Mathf.RoundToInt(pos.x);
        pos.y = Mathf.RoundToInt(pos.y);
        return pos;
    }

    // gets the maze coordinates at a world space position
    private Vector3 pos2Maze(Vector3 pos)
    {
        pos.x += maze.Width / 2;
        pos.y += maze.Height / 2;
        return pos;
    }

    // returns the nearest open tile, including the original tile
    private Vector3 nearestFloor(Vector3 pos)
    {
        // gets rounded position, and maze coordinates of mouse position and original tile
        Vector3 roundPos = roundVector(pos);
        Vector3 mazePos = pos2Maze(roundPos);
        Vector3 mazeDragStart = pos2Maze(spriteDragStartPos);

        // determines whether or not the mouse position is within the maze bounds and within the drag region
        bool inBounds = (int)mazePos.x >= 0 && (int)mazePos.x < maze.Width && (int)mazePos.y >= 0 && (int)mazePos.y < maze.Height;
        bool inRadius = Mathf.Abs((int)mazePos.x - (int)mazeDragStart.x) <= dragRadius && Mathf.Abs((int)mazePos.y - (int)mazeDragStart.y) <= dragRadius;

        // if the current position is the original tile or an open space, return the current position
        if (inBounds && inRadius && ((Vector2)roundPos == (Vector2)spriteDragStartPos || maze.Grid[(int)mazePos.x, (int)mazePos.y].isOpen()))
            return roundPos;
        
        Vector3 res = roundPos;
        float minDist = Mathf.Infinity;             // stores minimum distance between open tile and mouse position

        // finds nearest open tile to the mouse position
        for (int x = Mathf.Max((int)mazeDragStart.x - dragRadius, 0); x < Mathf.Min((int)mazeDragStart.x + dragRadius + 1, maze.Width); x++)
        {
            for (int y = Mathf.Max((int)mazeDragStart.y - dragRadius, 0); y < Mathf.Min((int)mazeDragStart.y + dragRadius + 1, maze.Height); y++)
            {
                MazeTile curr = maze.Grid[x, y];

                if (curr.isOpen())
                {
                    Vector2 currPos = curr.transform.position;
                    Vector2 diff = currPos - (Vector2)pos;
                    float dist = diff.sqrMagnitude;

                    // if curr is closer than any other tile so far, updated minDist and res
                    if (dist < minDist)
                    {
                        minDist = dist;
                        res = currPos;
                    }
                }
            }
        }

        return res;
    }
}