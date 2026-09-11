using UnityEngine;

[CreateAssetMenu(fileName = "PeopleData", menuName = "Scriptable Objects/PeopleData")]
public class PeopleData : ScriptableObject
{
    [Header("Main Body")]
    public Sprite sprite;

    public Vector2 pivot;
    public Vector2 sizeDelta;
    public Vector2 anchoredPosition;

    [Header("Body Part 1")]
    public bool bodyPartActive1;

    public Sprite bodyPartSprite1;

    public Vector2 bodyPartPivot1;
    public Vector2 bodyPartSizeDelta1;
    public Vector2 bodyPartAnchoredPosition1;

    [Header("Body Part 2")]
    public bool bodyPartActive2;

    public Sprite bodyPartSprite2;

    public Vector2 bodyPartPivot2;
    public Vector2 bodyPartSizeDelta2;
    public Vector2 bodyPartAnchoredPosition2;

    [Header("Animation")]
    public string animationTrigger;
    public bool multipleAnimations;
}
