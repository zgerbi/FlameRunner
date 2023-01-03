/*
 * MazeTiles make up the grid of the maze, handling sprites, animations, and logic
 * such as whether or not a tile is a floor, has been placed, is the target, is fire,
 * and the lifespan of the tile.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MazeTile : MonoBehaviour
{
    private bool floor;                                     // is a floor
    private bool locked;                                    // has been placed
    private bool path;                                      // is part of the current path
    private bool target;                                    // is the target
    private bool fire;                                      // is fire
    private int counter;                                    // lifespan counter
    private int counterReset;                               // default value of counter

    [SerializeField] private List<Sprite> pathSprites;      // list of path sprites

    // references to sprite renderers of all aspects of the MazeTile
    [HideInInspector] public SpriteRenderer floorRender;
    [HideInInspector] public SpriteRenderer pathRender;
    [HideInInspector] public SpriteRenderer wallRender;
    [HideInInspector] public SpriteRenderer targetRender;
    [HideInInspector] public SpriteRenderer fireRender;

    // references to the animators of the MazeTile
    [HideInInspector] public Animator wallAnimator;
    [HideInInspector] public Animator targetAnimator;
    [HideInInspector] public Animator fireAnimator;
    
    private TextMeshProUGUI counterTxt;                     // reference to counter text mesh
    [SerializeField] private Gradient textGradient;         // gradient for the counter text color to follow

    // triggers when the MazeTile is first instantiated, setting up default values
    void Awake()
    {
        // default settings
        floor = false;
        locked = false;
        path = false;
        target = false;
        fire = false;
        counter = 0;

        // get references to sprite renderers
        floorRender = transform.Find("Floor").GetComponent<SpriteRenderer>();
        pathRender = transform.Find("Path").GetComponent<SpriteRenderer>();
        wallRender = transform.Find("Wall").GetComponent<SpriteRenderer>();
        targetRender = transform.Find("Target").GetComponent<SpriteRenderer>();
        fireRender = transform.Find("Fire").GetComponent<SpriteRenderer>();

        // get references to animators
        wallAnimator = transform.Find("Wall").GetComponent<Animator>();
        targetAnimator = transform.Find("Target").GetComponent<Animator>();
        fireAnimator = transform.Find("Fire").GetComponent<Animator>();

        // set default visibility of sprites
        floorRender.enabled = true;
        pathRender.enabled = false;
        wallRender.enabled = true;
        targetRender.enabled = false;
        fireRender.enabled = false;

        // get reference to counter text mesh
        counterTxt = transform.GetChild(6).GetChild(0).GetComponent<TextMeshProUGUI>();
        counterTxt.enabled = false;
    }

    // updates every frame
    void Update()
    {
        // sets visibility of sprites
        pathRender.enabled = path;
        targetRender.enabled = target;
        fireRender.enabled = fire;
    }

    // get floor bool
    public bool isFloor()
    {
        return floor;
    }

    // set MazeTile to a floor
    public void setFloor()
    {
        floor = true;
        wallRender.enabled = false;
    }

    // get locked bool
    public bool isLocked()
    {
        return locked;
    }

    // activates a tile that has been placed
    public void activate()
    {
        // becomes a locked wall
        floor = false;
        locked = true;
        path = false;
        counter = counterReset;

        // resets sprite and animation
        wallRender.enabled = true;
        wallAnimator.SetBool("locked", true);
        wallAnimator.SetBool("crumble", false);

        // resets counter text
        counterTxt.enabled = true;
        counterTxt.SetText(counter.ToString());
        counterTxt.faceColor = textGradient.Evaluate(1);
    }

    // sets the appropriate path sprite according to the current path of the fire
    public void setPath(int i, ref List<Cell> p)
    {
        Sprite pathSprite;
        Cell curr = p[i];

        // if curr is not at an endpoint of the path
        if (i > 0 && i < p.Count - 1)
        {
            Cell next = p[i + 1];                                               // next tile in path
            Cell prev = p[i - 1];                                               // previous tile in path
            int slope = 0;                                                      // slope from prev to next position

            if (prev.x != next.x && prev.y != next.y)
                slope = (next.y - prev.y) / (next.x - prev.x);

            if (prev.x == next.x)                                               // horizontal
                pathSprite = pathSprites[1];
            else if (prev.y == next.y)                                          // vertical
                pathSprite = pathSprites[2];
            else if (slope == -1 && (next.y < curr.y || prev.y < curr.y))       // corner left and down
                pathSprite = pathSprites[3];
            else if (slope == 1 && (next.y < curr.y || prev.y < curr.y))        // corner right and down
                pathSprite = pathSprites[4];
            else if (slope == -1 && (next.y > curr.y || prev.y > curr.y))       // corner right and up
                pathSprite = pathSprites[5];
            else                                                                // corner left and up
                pathSprite = pathSprites[6];
        }

        // if the path is at least 2 tiles long
        else if (p.Count > 1)
        {
            // if curr is starting endpoint
            if (i == 0)
            {
                Cell next = p[i + 1];                   // next tile in path

                if (next.x < curr.x)                    // left
                    pathSprite = pathSprites[7];
                else if (next.y < curr.y)               // down
                    pathSprite = pathSprites[8];
                else if (next.x > curr.x)               // right
                    pathSprite = pathSprites[9];
                else                                    // up
                    pathSprite = pathSprites[10];
            }

            // otherwise if curr is final endpoint
            else
            {
                Cell prev = p[i - 1];                   // previous tile in path

                if (prev.x < curr.x)                    // left
                    pathSprite = pathSprites[7];
                else if (prev.y < curr.y)               // down
                    pathSprite = pathSprites[8];
                else if (prev.x > curr.x)               // right
                    pathSprite = pathSprites[9];
                else                                    // up
                    pathSprite = pathSprites[10];
            }
        }

        // otherwise if the path is only 1 tile long
        else
            pathSprite = pathSprites[0];

        pathRender.sprite = pathSprite;
        path = true;
    }

    // clear path on MazeTile
    public void clearPath()
    {
        path = false;
    }

    // get path bool
    public bool isPath()
    {
        return path;
    }
    
    // get target bool
    public bool isTarget()
    {
        return target;
    }
    
    // set whether or not MazeTile is the target
    public void setTarget(bool _target)
    {
        target = _target;
    }

    // get fire bool
    public bool isFire()
    {
        return fire;
    }

    // set whether or not MazeTile is fire
    public void setFire(bool _fire)
    {
        fire = _fire;
    }

    // returns whether or not MazeTile is open to having a tile placed here
    public bool isOpen()
    {
        return floor && !target && !fire;
    }

    // get counter value
    public int getCounter()
    {
        return counter;
    }

    // decrement counter value and crumble if it reaches 0
    public void decrementCounter()
    {
        counter--;

        // update text mesh value and color
        counterTxt.SetText(counter.ToString());
        counterTxt.faceColor = textGradient.Evaluate((float)counter / counterReset);

        if (counter == 0)
            crumble();
    }

    // crumble MazeTile
    public void crumble()
    {
        // reset tile to a floor
        counter = counterReset;
        floor = true;
        locked = false;

        wallAnimator.SetBool("crumble", true);      // trigger crumbling animation
        counterTxt.enabled = false;                 // hide counter text mesh
    }

    // set up MazeTile with appropriate position, default lifespan, game speed, and path color
    public void setup(int order, int tileCounter, int gameSpeed, Color pathColor)
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, order);
        counterReset = tileCounter;
        fireAnimator.SetInteger("gameSpeed", gameSpeed);
        pathRender.color = pathColor;
    }
}
