using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManger : MonoBehaviour
{
    [Header("Narration")]
    [SerializeField] AudioSource narrationAudioSource;
    [SerializeField] Sprite pauseSprite;
    [SerializeField] Sprite resumeSprite;
    [SerializeField] AudioClip danish_Narration;
    [SerializeField] AudioClip english_Narration;
    [SerializeField] float pauseDuration = 5.0f;

    [Header("Transition")]
    [SerializeField] Animator transition;
    [SerializeField] Screenshot screenshotScript;

    [Header("People")]
    [SerializeField] PeopleImageManager peopleImageManager;

    [Header("Coloring")]
    [SerializeField] ColoringManager coloringManager;
    [SerializeField] ColorManager colorManager;

    [Header("SettingsMenu")]
    [SerializeField] GameObject settingsMenuCanvas;
    [SerializeField] GameObject menuTitle;
    [SerializeField] Sprite titleDanish;
    [SerializeField] Sprite titleEnglish;
    [SerializeField] GameObject danishButton;
    [SerializeField] GameObject englishButton;
    [SerializeField] GameObject languageText;
    [SerializeField] GameObject resetGameButton;

    private GameObject[] paintingObjects;
    private GameObject[] uiObjects;
    private GameObject[] mainMenuObjects;
    private GameObject[] settingsMenuObjects;
    private bool NextPictureRunning;
    private int pictureIndex = 0;
    private bool narrationOn = false;
    private Coroutine narrationCoroutine;

    private static GameManger gameManagerInstance;

    /// <summary>
    /// Singleton to prevent there being two GameManagers at the same time
    /// </summary>
    private void Awake()
    {
        if (gameManagerInstance != null && gameManagerInstance != this)
        {
            Destroy(gameObject);
            return;
        }

        gameManagerInstance = this;
        DontDestroyOnLoad(gameObject);

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        paintingObjects = GameObject.FindGameObjectsWithTag("Painting");
        uiObjects = GameObject.FindGameObjectsWithTag("UI");
        mainMenuObjects = GameObject.FindGameObjectsWithTag("MainMenu");
        settingsMenuObjects = GameObject.FindGameObjectsWithTag("SettingsMenu");

        foreach (GameObject go in paintingObjects)
        {
            go.SetActive(false);
        }

        foreach (GameObject go in uiObjects)
        {
            DontDestroyOnLoad(go);
            go.SetActive(false);
        }

        foreach (GameObject go in mainMenuObjects)
        {
            DontDestroyOnLoad(go);
        }

        foreach (GameObject go in settingsMenuObjects)
        {
            DontDestroyOnLoad(go);
            go.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// The buttonpress for starting the game
    /// </summary>
    public void StartGamePress()
    {
        StartCoroutine(StartGame());
    }

    /// <summary>
    /// Closes the main menu and starts the game
    /// </summary>
    /// <returns></returns>
    IEnumerator StartGame()
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(1);

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            foreach (GameObject go in paintingObjects)
            {
                go.SetActive(true);
            }

            coloringManager.paintmode = true;

            StartCoroutine(peopleImageManager.AnimationLoop());
        }

        PlayNarration();

        foreach (GameObject go in uiObjects)
        {
            go.SetActive(true);
        }

        foreach (GameObject go in mainMenuObjects)
        {
            go.SetActive(false);
        }

        transition.SetTrigger("End");
    }

    /// <summary>
    /// Function to prevent there being two narrations at the same time
    /// </summary>
    private void PlayNarration()
    {
        if (narrationCoroutine != null)
        {
            StopCoroutine(narrationCoroutine);
        }

        narrationCoroutine = StartCoroutine(StartNarration());
    }

    /// <summary>
    /// Starts the narration while also allowing for it to pause for a bit before resetting if it finishes
    /// </summary>
    /// <returns></returns>
    IEnumerator StartNarration()
    {
        yield return new WaitForSeconds(1);

        narrationOn = true;

        while (true)
        {
            float timer = 0f;

            while (!narrationOn)
            {
                yield return null;
            }

            narrationAudioSource.Play();

            while (narrationAudioSource.isPlaying || narrationAudioSource.time > 0)
            {
                yield return null;
            }

            while (timer < pauseDuration)
            {
                if (narrationOn)
                {
                    timer += Time.deltaTime;
                }
                yield return null;
            }
        }
    }

    /// <summary>
    /// allows the pause and play button to toggle the narration on and off
    /// </summary>
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
            narrationAudioSource.UnPause();
            narrationOn = true;
            UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<Image>().sprite = pauseSprite;
        }
    }

    /// <summary>
    /// Prepares to go to the next parrot unless the its the final parrot where it goes to the final scene instead
    /// </summary>
    public void LoadNextPicture()
    {
        if (!NextPictureRunning)
        {
            NextPictureRunning = true;
            TakeScreenshot();
            pictureIndex++;
            if (pictureIndex >= coloringManager.pictures.Length)
            {
                StartCoroutine(NextScene());
                return;
            }
            StartCoroutine(NextPicture());
        }
    }

    /// <summary>
    /// Goes to the next parrot and running the necessary functions
    /// </summary>
    /// <returns></returns>
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
    }

    /// <summary>
    /// Goes to the final scene
    /// </summary>
    /// <returns></returns>
    IEnumerator NextScene()
    {
        //Fade transition
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(1);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(1);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Object.Destroy(GameObject.Find("NextDrawingButton"));
        Object.Destroy(GameObject.Find("ResetButton"));

        coloringManager.paintmode = false;

        FindAllParrotObjects();

        transition.SetTrigger("End");

        NextPictureRunning = false;
    }

    /// <summary>
    /// Opens or closes the settingsmenu via buttonpress
    /// </summary>
    /// <param name="active">bool to check if we need to open or close the settings</param>
    public void OpenSetting(bool active)
    {
        settingsMenuCanvas.SetActive(active);
    }

    /// <summary>
    /// Sets the language and swaps the neccessary objects and text
    /// </summary>
    /// <param name="index">Index to decide what language to swap to</param>
    public void SetLanguage(int index)
    {
        switch (index)
        {
            //English
            case 1:
                Debug.Log("Language set to English");
                menuTitle.GetComponent<Image>().sprite = titleEnglish;
                narrationAudioSource.clip = english_Narration;
                englishButton.GetComponent<Outline>().enabled = true;
                danishButton.GetComponent<Outline>().enabled = false;
                resetGameButton.GetComponentInChildren<TextMeshProUGUI>().text = "Reset Game";
                languageText.GetComponent<TextMeshProUGUI>().text = "Language";
                break;

            //Danish
            case 2:
                Debug.Log("Language set to Danish");
                menuTitle.GetComponent<Image>().sprite = titleDanish;
                narrationAudioSource.clip = danish_Narration;
                danishButton.GetComponent<Outline>().enabled = true;
                englishButton.GetComponent<Outline>().enabled = false;
                resetGameButton.GetComponentInChildren<TextMeshProUGUI>().text = "Nulstil Spil";
                languageText.GetComponent<TextMeshProUGUI>().text = "Sprog";
                break;
        }
    }

    /// <summary>
    /// Button press for resetting the paint of a parrot
    /// </summary>
    public void ResetPainting()
    {
        coloringManager.ClearCanvas();
    }

    /// <summary>
    /// Button press for opening the main menu
    /// </summary>
    public void ClickOpenMainMenu()
    {
        if (narrationCoroutine != null)
        {
            StopCoroutine(narrationCoroutine);
            narrationCoroutine = null;
        }
        narrationAudioSource.Stop();
        narrationOn = false;

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            pictureIndex = 0;
            Screenshot.screenshots.Clear();
        }

        StartCoroutine(OpenMainMenu());
    }

    /// <summary>
    /// Opens the main menu
    /// </summary>
    /// <returns></returns>
    IEnumerator OpenMainMenu()
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(1);

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            coloringManager.NextPicture(pictureIndex);
            colorManager.UpdateColors(pictureIndex);
            peopleImageManager.ChangeImageAndAnimations(pictureIndex);
            coloringManager.paintmode = false;
        }

        foreach (GameObject go in mainMenuObjects)
        {
            go.SetActive(true);
        }

        transition.SetTrigger("End");

        NextPictureRunning = false;
    }

    /// <summary>
    /// Destroys all objects and completely resets the game
    /// </summary>
    public void ResetGame()
    {
        foreach (GameObject go in uiObjects)
        {
            Destroy(go);
        }

        foreach (GameObject go in mainMenuObjects)
        {
            Destroy(go);
        }

        foreach (GameObject go in settingsMenuObjects)
        {
            Destroy(go);
        }
        Destroy(gameObject);
        SceneManager.LoadScene(0);
    }

    /// <summary>
    /// Takes a screenshot to use for the final scene
    /// </summary>
    public void TakeScreenshot()
    {
        coloringManager.ClearMaskPaint();
        if (coloringManager.Branchless[pictureIndex] != null)
        {
            coloringManager.picture.sprite = coloringManager.Branchless[pictureIndex];
        }
        screenshotScript.takeScreenshot = true;
    }

    /// <summary>
    /// Gets all parrot placements in the final scene and places the screenshots in those positions
    /// </summary>
    private void FindAllParrotObjects()
    {
        GameObject parrent = GameObject.Find("Parrots");
        GameObject[] children = new GameObject[parrent.transform.childCount];

        for (int i = 0; i < children.Length; i++)
        {
            children[i] = parrent.transform.GetChild(i).gameObject;
            children[i].GetComponent<SpriteRenderer>().sprite = Screenshot.screenshots[i];
        }
    }
}
