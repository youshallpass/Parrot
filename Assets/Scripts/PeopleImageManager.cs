using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PeopleImageManager : MonoBehaviour
{
    [SerializeField] PeopleData[] people;
    [SerializeField] Animator PeopleAnimator;
    [SerializeField] GameObject bodyPart1;
    [SerializeField] GameObject bodyPart2;
    [SerializeField] RectTransform peoplePosition;
    [SerializeField] RectTransform bodypartPosition1;
    [SerializeField] RectTransform bodypartPosition2;
    private Image peopleImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        peopleImage = GetComponent<Image>();
        ChangeImageAndAnimations(0);
        bodyPart1.SetActive(false);
        bodyPart2.SetActive(false);
    }

    public void ChangeImageAndAnimations(int imageIndex)
    {
        PeopleData person = people[imageIndex];

        peopleImage.sprite = person.sprite;

        RectTransform rectPerson = peopleImage.rectTransform;
        rectPerson.pivot = person.pivot;
        rectPerson.sizeDelta = person.sizeDelta;
        peoplePosition.anchoredPosition = person.anchoredPosition;

        bodyPart1.SetActive(person.bodyPartActive1);

        if (person.bodyPartSprite1 != null)
        {
            bodyPart1.GetComponent<Image>().sprite = person.bodyPartSprite1;

            RectTransform rectBodyPart1 = bodyPart1.GetComponent<Image>().rectTransform;
            rectBodyPart1.pivot = person.bodyPartPivot1;
            rectBodyPart1.sizeDelta = person.bodyPartSizeDelta1;
            bodypartPosition1.anchoredPosition = person.bodyPartAnchoredPosition1;
        }

        bodyPart2.SetActive(person.bodyPartActive2);

        if (person.bodyPartSprite2 != null)
        {
            bodyPart2.GetComponent<Image>().sprite = person.bodyPartSprite2;

            RectTransform rectBodyPart2 = bodyPart2.GetComponent<Image>().rectTransform;
            rectBodyPart2.pivot = person.bodyPartPivot2;
            rectBodyPart2.sizeDelta = person.bodyPartSizeDelta2;
            bodypartPosition2.anchoredPosition = person.bodyPartAnchoredPosition2;

        }

        if (!string.IsNullOrEmpty(person.animationTrigger))
        {
            PeopleAnimator.SetTrigger(person.animationTrigger);
        }

        if (person.multipleAnimations)
        {
            PeopleAnimator.SetLayerWeight(1, 1f);
        }
        else
        {
            PeopleAnimator.SetLayerWeight(1, 0f);
        }

        StopCoroutine(AnimationLoop());
        
        StartCoroutine(AnimationLoop());
    }

    public IEnumerator AnimationLoop()
    {
        while (true)
        {
            PeopleAnimator.enabled = true;
            yield return new WaitForSeconds(10f);

            PeopleAnimator.enabled = false;
            yield return new WaitForSeconds(10f);
        }
    }
}
