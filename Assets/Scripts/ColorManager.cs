using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ColorManager : MonoBehaviour
{
    [SerializeField] ColoringManager coloringManager;
    [SerializeField] ColorData[] colorData;
    private GameObject[] sortedColorObjects;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject[] colorObjects = GameObject.FindGameObjectsWithTag("Colors");
        sortedColorObjects = colorObjects.OrderBy(go => go.name).ToArray();

        UpdateColors(0);

        //Sets the current color of the paintbrush using the color of the pressed button
        List<Button> allButtons = new List<Button>();
        foreach (var go in sortedColorObjects)
        {
            allButtons.Add(go.GetComponent<Button>());
        }
        foreach (Button btn in allButtons)
        {
            Image img = btn.GetComponent<Image>();

            btn.onClick.AddListener(() =>
            {
                coloringManager.SetColor(img.color);
                Debug.Log($"Color changed to {img.color} from button {btn.name}");
            });
        }
    }

    /// <summary>
    /// Updated the colors of the palette based on the scriptable object ColorData
    /// </summary>
    /// <param name="index">Current index so it matches the current parrot</param>
    public void UpdateColors(int index)
    {
        ColorData color = colorData[index];

        for (int i = 0; i < sortedColorObjects.Length; i++)
        {
            sortedColorObjects[i].GetComponent<Image>().color = color.color[i];
        }

        coloringManager.SetColor(color.color[0]);
    }
}
