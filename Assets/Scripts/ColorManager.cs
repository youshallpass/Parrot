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

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateColors(int index)
    {
        ColorData color = colorData[index];

        for (int i = 0; i < sortedColorObjects.Length; i++)
        {
            sortedColorObjects[i].GetComponent<Image>().color = color.color[i];
        }
    }
}
