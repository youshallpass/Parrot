using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManger : MonoBehaviour
{
    [Header("Narration")]
    [SerializeField] AudioSource narrationAudioSource;
    [SerializeField] Sprite pauseSprite;
    [SerializeField] Sprite resumeSprite;
    [SerializeField] AudioClip danish_Narration;
    [SerializeField] AudioClip english_Narration;

    [Header("Transition")]
    [SerializeField] Animator transition;
    [SerializeField] Image whereToShowScreenshot;

    [Header("People")]
    [SerializeField] PeopleImageManager peopleImageManager;

    [Header("Coloring")]
    [SerializeField] ColoringManager coloringManager;
    [SerializeField] ColorManager colorManager;

    [Header("SettingsMenu")]
    [SerializeField] GameObject settingsMenuCanvas;
    [SerializeField] GameObject menu_Title;
    [SerializeField] Sprite title_Danish;
    [SerializeField] Sprite title_English;
    [SerializeField] GameObject danish_Button;
    [SerializeField] GameObject english_Button;

    private GameObject[] paintingObjects;
    private GameObject[] uiObjects;
    private GameObject[] mainMenuObjects;
    private bool NextPictureRunning;
    private int pictureIndex = 0;
    private bool narrationOn = false;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

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
            DontDestroyOnLoad(go);
            go.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
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

        coloringManager.paintmode = true;

        transition.SetTrigger("End");

        StartCoroutine(peopleImageManager.AnimationLoop());

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
            UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Image>().sprite = resumeSprite;

        }
        else
        {
            narrationAudioSource.Play();
            narrationOn = true;
            UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Image>().sprite = pauseSprite;
        }
    }

    public void LoadNextPicture()
    {
        if (!NextPictureRunning)
        {
            NextPictureRunning = true;
            pictureIndex++;
            if (pictureIndex >= coloringManager.pictures.Length)
            {
                StartCoroutine(NextScene());
                return;
            }
            StartCoroutine(NextPicture());
        }
    }

    IEnumerator NextPicture()
    {
        //Fade transition
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(1);
        coloringManager.NextPicture(pictureIndex);
        colorManager.UpdateColors(pictureIndex);
        peopleImageManager.ChangeImageAndAnimations(pictureIndex);

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

    IEnumerator NextScene()
    {
        //Fade transition
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(1);
        Object.Destroy(GameObject.Find("NextDrawingButton"));

        coloringManager.paintmode = false;

        transition.SetTrigger("End");

        NextPictureRunning = false;
    }

    public void OpenSetting(bool active)
    {
        settingsMenuCanvas.SetActive(active);
    }

    public void SetLanguage(int index)
    {
        switch (index)
        {
            //English
            case 1:
                Debug.Log("Language set to English");
                menu_Title.GetComponent<Image>().sprite = title_English;
                narrationAudioSource.clip = english_Narration;
                english_Button.GetComponent<Outline>().enabled = true;
                danish_Button.GetComponent<Outline>().enabled = false;
                break;

            //Danish
            case 2:
                Debug.Log("Language set to Danish");
                menu_Title.GetComponent<Image>().sprite = title_Danish;
                narrationAudioSource.clip = danish_Narration;
                danish_Button.GetComponent<Outline>().enabled = true;
                english_Button.GetComponent<Outline>().enabled = false;
                break;
        }
    }
}
