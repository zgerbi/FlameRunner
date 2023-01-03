/*
 * Stats are used to store algorithm performance data, such as path lengths, tiles checked,
 * and calculation times. These are primarily used within the right-side feed.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Stat : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pathText;      // reference to path length text mesh
    [SerializeField] private TextMeshProUGUI tilesText;     // reference to tiles checked text mesh
    [SerializeField] private TextMeshProUGUI timeText;      // reference to calculation time text mesh
    
    private int pathLength;                                 // stores path length
    private int tilesChecked;                               // stores tiles checked
    private float calculationTime;                          // stores calculation time

    // sets the values of the Stat and sets the text of text meshes
    public void Setup(float time, int path, int tiles)
    {
        calculationTime = time;
        pathLength = path;
        tilesChecked = tiles;

        timeText.text = time.ToString("0.000");
        pathText.text = path.ToString();
        tilesText.text = tiles.ToString();
    }
}
