/*
 * A Graph is used to generate a line graph to show a statistical summary of
 * algorithm performance throughout a session.
 * 
 * Credit: Most of this class is derived from the following YouTube video by Code Monkey
 * Source: https://www.youtube.com/watch?v=CmU5-v-v1Qo
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Graph : MonoBehaviour
{
    [SerializeField] private Sprite pointSprite;                // sprite for a point on the graph
    [SerializeField] private Color connectionColor;             // color for point connections

    [SerializeField] private int separatorCountX;               // number of x-axis dividers
    [SerializeField] private int separatorCountY;               // number of y-axis dividers

    // references to various objects within the graph
    private RectTransform graphContainer;
    private RectTransform labelTemplateX;
    private RectTransform labelTemplateY;
    private RectTransform lineTemplateX;
    private RectTransform lineTemplateY;

    private List<GameObject> gameObjects;                       // list of all objects generated

    // sets default values
    public void Setup()
    {
        graphContainer = transform.Find("Graph Contents").GetComponent<RectTransform>();
        labelTemplateX = graphContainer.Find("LabelTemplateX").GetComponent<RectTransform>();
        labelTemplateY = graphContainer.Find("LabelTemplateY").GetComponent<RectTransform>();
        lineTemplateX = graphContainer.Find("LineTemplateX").GetComponent<RectTransform>();
        lineTemplateY = graphContainer.Find("LineTemplateY").GetComponent<RectTransform>();

        gameObjects = new List<GameObject>();
    }

    // generates all necessary objects, sets their values, and displays the final line graph
    // (intended for line graph of path lengths and tiles checked)
    public void ShowGraph(List<int> values)
    {
        // destroys previous graph
        foreach (GameObject obj in gameObjects)
            Destroy(obj);
        gameObjects.Clear();
        
        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;

        float yMax = values[0];
        float yMin = values[0];

        // finds minimum and maximum values within list
        foreach (int v in values)
        {
            if (v > yMax)
                yMax = v;
            if (v < yMin)
                yMin = v;
        }

        yMax += (yMax - yMin) * 0.2f;
        yMin -= (yMax - yMin) * 0.2f;

        yMin = Mathf.Max(yMin, 0);
        
        float xSize = graphWidth / (values.Count + 1);

        GameObject lastPointObj = null;

        // generates necessary points, connections, and dividers for graph
        for (int i = 0; i < values.Count; i++)
        {
            // creates point
            float xPos = i * xSize;
            float yPos = ((values[i] - yMin) / (yMax - yMin)) * graphHeight;
            GameObject pointObj = CreatePoint(new Vector2(xPos, yPos));
            gameObjects.Add(pointObj);

            // creates connection
            if (lastPointObj != null)
            {
                GameObject pointConnectionObj = CreatePointConnection(
                    lastPointObj.GetComponent<RectTransform>().anchoredPosition,
                    pointObj.GetComponent<RectTransform>().anchoredPosition
                );
                gameObjects.Add(pointConnectionObj);
            }
            lastPointObj = pointObj;

            // creates x-axis dividers and axis labels
            if (separatorCountX > values.Count || i % (values.Count / separatorCountX) == 0)
            {
                // creates labels
                RectTransform labelX = Instantiate(labelTemplateX);
                labelX.SetParent(graphContainer, false);
                labelX.gameObject.SetActive(true);
                labelX.anchoredPosition = new Vector2(xPos, -5f);
                labelX.GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
                gameObjects.Add(labelX.gameObject);

                // creates dividers
                RectTransform lineX = Instantiate(lineTemplateX);
                lineX.SetParent(graphContainer, false);
                lineX.gameObject.SetActive(true);
                lineX.anchoredPosition = new Vector2(xPos, 0);
                gameObjects.Add(lineX.gameObject);
            }
        }

        // creates y-axis dividers and axis labels
        for (int i = 0; i <= separatorCountY; i++)
        {
            // creates labels
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer, false);
            labelY.gameObject.SetActive(true);
            float normalizedValue = (float)i / separatorCountY;
            labelY.anchoredPosition = new Vector2(-5f, normalizedValue * graphHeight);
            labelY.GetComponent<TextMeshProUGUI>().text = Mathf.RoundToInt(yMin + (normalizedValue * (yMax - yMin))).ToString();
            gameObjects.Add(labelY.gameObject);

            // creates dividers
            RectTransform lineY = Instantiate(lineTemplateY);
            lineY.SetParent(graphContainer, false);
            lineY.gameObject.SetActive(true);
            lineY.anchoredPosition = new Vector2(0, normalizedValue * graphHeight);
            gameObjects.Add(lineY.gameObject);
        }
    }

    // overloaded function for a list of float values (intended for line graph of calculation times)
    public void ShowGraph(List<float> values)
    {
        // destroys previous graph
        foreach (GameObject obj in gameObjects)
            Destroy(obj);
        gameObjects.Clear();

        float graphWidth = graphContainer.sizeDelta.x;
        float graphHeight = graphContainer.sizeDelta.y;

        float yMax = values[0];
        float yMin = values[0];

        // finds minimum and maximum values within list
        foreach (float v in values)
        {
            if (v > yMax)
                yMax = v;
            if (v < yMin)
                yMin = v;
        }

        yMax += (yMax - yMin) * 0.2f;
        yMin -= (yMax - yMin) * 0.2f;

        yMin = Mathf.Max(yMin, 0);

        float xSize = graphWidth / (values.Count + 1);

        GameObject lastPointObj = null;

        // generates necessary points, connections, and dividers for graph
        for (int i = 0; i < values.Count; i++)
        {
            // creates point
            float xPos = i * xSize;
            float yPos = ((values[i] - yMin) / (yMax - yMin)) * graphHeight;
            GameObject pointObj = CreatePoint(new Vector2(xPos, yPos));
            gameObjects.Add(pointObj);

            // creates connection
            if (lastPointObj != null)
            {
                GameObject pointConnectionObj = CreatePointConnection(
                    lastPointObj.GetComponent<RectTransform>().anchoredPosition,
                    pointObj.GetComponent<RectTransform>().anchoredPosition
                );
                gameObjects.Add(pointConnectionObj);
            }
            lastPointObj = pointObj;

            // creates x-axis dividers and axis labels
            if (separatorCountX > values.Count || i % (values.Count / separatorCountX) == 0)
            {
                // creates labels
                RectTransform labelX = Instantiate(labelTemplateX);
                labelX.SetParent(graphContainer, false);
                labelX.gameObject.SetActive(true);
                labelX.anchoredPosition = new Vector2(xPos, -5f);
                labelX.GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
                gameObjects.Add(labelX.gameObject);

                // creates dividers
                RectTransform lineX = Instantiate(lineTemplateX);
                lineX.SetParent(graphContainer, false);
                lineX.gameObject.SetActive(true);
                lineX.anchoredPosition = new Vector2(xPos, 0);
                gameObjects.Add(lineX.gameObject);
            }
        }
        
        // creates y-axis dividers and labels
        for (int i = 0; i <= separatorCountY; i++)
        {
            // creates labels
            RectTransform labelY = Instantiate(labelTemplateY);
            labelY.SetParent(graphContainer, false);
            labelY.gameObject.SetActive(true);
            float normalizedValue = (float)i / separatorCountY;
            labelY.anchoredPosition = new Vector2(-5f, normalizedValue * graphHeight);
            labelY.GetComponent<TextMeshProUGUI>().text = (yMin + (normalizedValue * (yMax - yMin))).ToString("0.000");
            gameObjects.Add(labelY.gameObject);

            // creates dividers
            RectTransform lineY = Instantiate(lineTemplateY);
            lineY.SetParent(graphContainer, false);
            lineY.gameObject.SetActive(true);
            lineY.anchoredPosition = new Vector2(0, normalizedValue * graphHeight);
            gameObjects.Add(lineY.gameObject);
        }
    }

    // creates a point at the given position
    private GameObject CreatePoint(Vector2 anchoredPos)
    {
        GameObject gameObject = new GameObject("Point", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().sprite = pointSprite;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = anchoredPos;
        rectTransform.sizeDelta = new Vector2(11, 11);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        return gameObject;
    }

    // creates a connection between 2 given points
    private GameObject CreatePointConnection(Vector2 pos1, Vector2 pos2)
    {
        GameObject gameObject = new GameObject("Point Connection", typeof(Image));
        gameObject.transform.SetParent(graphContainer, false);
        gameObject.GetComponent<Image>().color = connectionColor;
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector2 dir = (pos2 - pos1).normalized;
        float distance = Vector2.Distance(pos1, pos2);
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.sizeDelta = new Vector2(distance, 2f);
        rectTransform.anchoredPosition = pos1 + dir * distance * 0.5f;
        rectTransform.localEulerAngles = new Vector3(0, 0, vectorAngle(dir));
        return gameObject;
    }

    // calculates the angle towards which a vector is pointing in degrees
    private float vectorAngle(Vector2 dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }
}
