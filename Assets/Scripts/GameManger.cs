using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManger : MonoBehaviour
{
    [SerializeField] AudioSource narrationAudioSource;
    private GameObject[] paintingObjects;
    private GameObject[] uiObjects;
    private GameObject[] mainMenuObjects;
    private bool narrationOn = false;
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

    public void StartGame()
    {
        foreach(GameObject go in paintingObjects)
        {
            go.SetActive(true);
        }

        foreach(GameObject go in uiObjects)
        {
            go.SetActive(true);
        }

        foreach (GameObject go in mainMenuObjects)
        {
            go.SetActive(false);
        }

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
}
