/*
 * Creates singleton reference to MazeGenerator so that any class can easily access the maze
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MazeStatic
{
    public static MazeGenerator mazeGen;
}
