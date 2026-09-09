using UnityEngine;
using UnityEngine.UI;

public class PeopleImageManager : MonoBehaviour
{
    [SerializeField] PeopleData[] people;
    [SerializeField] Animator PeopleAnimator;
    [SerializeField] private GameObject bodyPart;
    private Image peopleImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        peopleImage = GetComponent<Image>();
        ChangeImageAndAnimations(0);
        bodyPart.SetActive(false);
    }

    public void ChangeImageAndAnimations(int imageIndex)
    {
        PeopleData person = people[imageIndex];

        peopleImage.sprite = person.sprite;

        RectTransform rectPerson = peopleImage.rectTransform;
        rectPerson.pivot = person.pivot;
        rectPerson.sizeDelta = person.sizeDelta;
        rectPerson.anchoredPosition = person.anchoredPosition;

        bodyPart.SetActive(person.bodyPartActive);

        if (person.bodyPartSprite != null)
        {
            bodyPart.GetComponent<Image>().sprite = person.bodyPartSprite;

            RectTransform rectBodyPart = bodyPart.GetComponent<Image>().rectTransform;
            rectBodyPart.pivot = person.bodyPartPivot;
            rectBodyPart.sizeDelta = person.bodyPartSizeDelta;
            rectBodyPart.anchoredPosition = person.bodyPartAnchoredPosition;
        }

        if (!string.IsNullOrEmpty(person.animationTrigger))
        {
            PeopleAnimator.SetTrigger(person.animationTrigger);
        }
    }
}
