using UnityEngine;

[CreateAssetMenu(fileName = "ColorData", menuName = "Scriptable Objects/ColorData")]
public class ColorData : ScriptableObject
{
    [Header("ColorPalette")]
    [Tooltip("Select the color for the palette for a particular parrot")]
    public Color[] color = new Color[10];
}
