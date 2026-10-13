using UnityEngine;

[CreateAssetMenu(fileName = "PeopleData", menuName = "Scriptable Objects/PeopleData")]
public class PeopleData : ScriptableObject
{
    [Header("Main Body")]
    [Tooltip("Sprite of the person")]
    public Sprite sprite;

    [Tooltip("Transform of the person")]
    public Vector2 pivot;
    [Tooltip("Transform of the person")]
    public Vector2 sizeDelta;
    [Tooltip("Transform of the person")]
    public Vector2 anchoredPosition;

    [Header("Body Part 1")]
    [Tooltip("Activate if body part should move independently")]
    public bool bodyPartActive1;

    [Tooltip("Sprite of the body part")]
    public Sprite bodyPartSprite1;

    [Tooltip("Transform of the body part")]
    public Vector2 bodyPartPivot1;
    [Tooltip("Transform of the body part")]
    public Vector2 bodyPartSizeDelta1;
    [Tooltip("Transform of the body part")]
    public Vector2 bodyPartAnchoredPosition1;

    [Header("Body Part 2")]
    [Tooltip("Activate if body part should move independently")]
    public bool bodyPartActive2;

    [Tooltip("Sprite of the body part")]
    public Sprite bodyPartSprite2;

    [Tooltip("Transform of the body part")]
    public Vector2 bodyPartPivot2;
    [Tooltip("Transform of the body part")]
    public Vector2 bodyPartSizeDelta2;
    [Tooltip("Transform of the body part")]
    public Vector2 bodyPartAnchoredPosition2;

    [Header("Animation")]
    [Tooltip("Trigger word for the animations")]
    public string animationTrigger;

    [Tooltip("Activate if there are multiple sprites that need to move independently, For example if both arms need to wave different directions")]
    public bool multipleAnimations;
}
