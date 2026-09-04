using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManger : MonoBehaviour
{
    [SerializeField] AudioSource narrationAudioSource;
    [SerializeField] ColoringManager coloringManager;
    [SerializeField] Animator transition;
    [SerializeField] Image whereToShowScreenshot;
    public float transitionTime = 2f;
    private GameObject[] paintingObjects;
    private GameObject[] uiObjects;
    private GameObject[] mainMenuObjects;
    private bool narrationOn = false;
    private bool NextPictureRunning;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        paintingObjects = GameObject.FindGameObjectsWithTag("Painting");
        uiObjects = GameObject.FindGameObjectsWithTag("UI");
        mainMenuObjects = GameObject.FindGameObjectsWithTag("MainMenu");

        foreach (GameObject go in paintingObjects)
        {
            go.SetActive(false);
        }

        foreach (GameObject go in uiObjects)
        {
            go.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OpenInfoScreen()
    {

    }

    public void StartGamePress()
    {
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(1);

        foreach (GameObject go in paintingObjects)
        {
            go.SetActive(true);
        }

        foreach (GameObject go in uiObjects)
        {
            go.SetActive(true);
        }

        foreach (GameObject go in mainMenuObjects)
        {
            go.SetActive(false);
        }

        transition.SetTrigger("End");

        StartCoroutine(StartNarration());
    }

    IEnumerator StartNarration()
    {
        yield return new WaitForSeconds(5);
        narrationAudioSource.Play();
        narrationOn = true;
    }

    public void ToggleNarration()
    {
        if (narrationOn)
        {
            narrationAudioSource.Pause();
            narrationOn = false;
            UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Image>().color = Color.red;

        }
        else
        {
            narrationAudioSource.Play();
            narrationOn = true;
            UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Image>().color = Color.blue;
        }
    }

    public void LoadNextPicture()
    {
        if (!NextPictureRunning)
        {
            NextPictureRunning = true;
            StartCoroutine(NextPicture());
        }
    }

    IEnumerator NextPicture()
    {
        //Fade transition
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(1);
        coloringManager.NextPicture();

        transition.SetTrigger("End");

        NextPictureRunning = false;


        //PageFlip Transition
        //foreach (GameObject go in uiObjects)
        //{
        //    go.SetActive(false);
        //}

        //yield return new WaitForEndOfFrame();

        //Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();

        //Texture2D newScreenshot = new Texture2D(screenshot.width, screenshot.height, TextureFormat.ARGB32, false);
        //newScreenshot.SetPixels(screenshot.GetPixels());
        //newScreenshot.Apply();

        //Destroy(screenshot);

        //Sprite screenshotSprite = Sprite.Create(newScreenshot, new Rect(0, 0, newScreenshot.width, newScreenshot.height), new Vector2(0.5f, 0.5f));

        //whereToShowScreenshot.enabled = true;
        //whereToShowScreenshot.sprite = screenshotSprite;

        //coloringManager.NextPicture();

        //transition.SetTrigger("Start");

        //transition.SetTrigger("End");

        //yield return new WaitForSeconds(1);

        //whereToShowScreenshot.enabled = false;

        //foreach (GameObject go in uiObjects)
        //{
        //    go.SetActive(true);
        //}

        //NextPictureRunning = false;
    }
}
