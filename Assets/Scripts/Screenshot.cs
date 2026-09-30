using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Screenshot : MonoBehaviour
{
    public static List<Sprite> screenshots = new List<Sprite>();

    public SpriteRenderer targetSprite;
    public Texture2D lineart;

    public bool takeScreenshot = false;

    [Range(0f, 1f)]
    public float whiteThreshold = 0.95f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        screenshots.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        if (takeScreenshot)
        {
            StartCoroutine(CaptureScreenshot());
            takeScreenshot = false;
        }
    }

    IEnumerator CaptureScreenshot()
    {
        yield return new WaitForEndOfFrame();

        Bounds bounds = targetSprite.bounds;

        Vector3 worldBottomLeft = new Vector3(bounds.min.x, bounds.min.y, bounds.min.z);
        Vector3 worldTopRight = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);

        Vector3 screenBottomLeft = Camera.main.WorldToScreenPoint(worldBottomLeft);
        Vector3 screenTopRight = Camera.main.WorldToScreenPoint(worldTopRight);

        int startX = Mathf.RoundToInt(screenBottomLeft.x);
        int startY = Mathf.RoundToInt(screenBottomLeft.y);
        int Width = Mathf.RoundToInt(screenTopRight.x - screenBottomLeft.x);
        int Height = Mathf.RoundToInt(screenTopRight.y - screenBottomLeft.y);

        Texture2D screenshotTexture = new Texture2D(Width, Height, TextureFormat.ARGB32, false);

        screenshotTexture.ReadPixels(new Rect(startX, startY, Width, Height), 0, 0);

        Color[] pixels = screenshotTexture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            // Check if Red, Green, and Blue values are all above the threshold
            if (pixels[i].r >= whiteThreshold &&
                pixels[i].g >= whiteThreshold &&
                pixels[i].b >= whiteThreshold)
            {
                pixels[i] = Color.clear; // Sets r, g, b, and alpha to 0
            }
        }
        screenshotTexture.SetPixels(pixels);

        screenshotTexture.Apply();

        Sprite screenshotSprite = Sprite.Create(screenshotTexture, new Rect(0, 0, Width, Height), new Vector2(0.5f, 0.5f));

        screenshots.Add(screenshotSprite);
        Debug.Log($"Snapped screenshot. Total pictures: {screenshots.Count}");
    }
}
