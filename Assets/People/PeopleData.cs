using UnityEngine;

[CreateAssetMenu(fileName = "PeopleData", menuName = "Scriptable Objects/PeopleData")]
public class PeopleData : ScriptableObject
{
    [Header("Main Body")]
    public Sprite sprite;

    public Vector2 pivot;
    public Vector2 sizeDelta;
    public Vector2 anchoredPosition;

    [Header("Body Part")]
    public bool bodyPartActive;

    public Sprite bodyPartSprite;

    public Vector2 bodyPartPivot;
    public Vector2 bodyPartSizeDelta;
    public Vector2 bodyPartAnchoredPosition;

    [Header("Animation")]
    public string animationTrigger;
}
