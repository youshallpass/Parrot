using UnityEngine;
using UnityEngine.UI;

public class PeopleImageManager : MonoBehaviour
{
    [SerializeField] Animator PeopleAnimator;
    [SerializeField] Sprite[] peopleSprites;
    private Image peopleImage;
    private GameObject[] bodyParts;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        peopleImage = GetComponent<Image>();
        ChangeImageAndAnimations(0);
        bodyParts = GameObject.FindGameObjectsWithTag("BodyPart");
        foreach (GameObject go in bodyParts)
        {
            go.SetActive(false);
        }
    }

    public void ChangeImageAndAnimations(int imageIndex)
    {
        switch (imageIndex)
        {
            case 0:
                peopleImage.sprite = peopleSprites[imageIndex];
                peopleImage.rectTransform.sizeDelta = new Vector2(510, 807);
                peopleImage.rectTransform.position = new Vector3(700, 720);
                break;

            case 1:
                peopleImage.sprite = peopleSprites[imageIndex];
                peopleImage.rectTransform.sizeDelta = new Vector2(374, 992);
                peopleImage.rectTransform.transform.position = new Vector3(700, 620);
                foreach (GameObject go in bodyParts)
                {
                    go.SetActive(true);
                }
                PeopleAnimator.SetTrigger("Case1");
                break;

            case 2:
                peopleImage.sprite = peopleSprites[imageIndex];
                peopleImage.rectTransform.pivot = new Vector2(0.5f, 0f);
                peopleImage.rectTransform.sizeDelta = new Vector2(471, 1200);
                peopleImage.rectTransform.anchoredPosition = new Vector3(-900, -650);
                foreach (GameObject go in bodyParts)
                {
                    go.SetActive(false);
                }
                PeopleAnimator.SetTrigger("Case2");
                break;
        }
    }
}
