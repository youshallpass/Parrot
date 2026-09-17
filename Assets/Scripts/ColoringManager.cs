using UnityEngine;
using UnityEngine.UI;

public class ColoringManager : MonoBehaviour
{
    [Header("Lineart")]
    [SerializeField] public Sprite[] pictures;
    [SerializeField] private Image picture;

    [Header("Paintlayer & Masks")]
    [SerializeField] private SpriteRenderer paintLayer;
    [SerializeField] private Texture2D[] mainMask;
    [SerializeField] private Texture2D[] removableMask;

    [Header("Brush Settings")]
    public float brushSize = 20f;

    private Texture2D canvasTexture;
    private Texture2D[] canvasTextures;
    private int pictureIndex = 0;

    private Color color = Color.red;

    private Vector2 previousTouchPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasTextures = new Texture2D[pictures.Length];

        picture.sprite = pictures[0];

        InitializeCanvasTexture();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                previousTouchPosition = touch.position;
                Paint(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                PaintLine(previousTouchPosition, touch.position);
                previousTouchPosition = touch.position;
            }
        }

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            Paint(Input.mousePosition);
        }
#endif
    }

    public void InitializeCanvasTexture()
    {
        if (canvasTextures[pictureIndex] != null)
        {
            canvasTexture = canvasTextures[pictureIndex];
            paintLayer.sprite = Sprite.Create(canvasTexture, new Rect(0, 0, canvasTexture.width, canvasTexture.height), new Vector2(0.5f, 0.5f), picture.sprite.pixelsPerUnit);
            return;
        }

        Texture2D pictureData = picture.sprite.texture;
        canvasTexture = new Texture2D(pictureData.width, pictureData.height, TextureFormat.RGBA32, false);
        canvasTexture.filterMode = FilterMode.Bilinear;
        canvasTextures[pictureIndex] = canvasTexture;

        Sprite paintSprite = Sprite.Create(canvasTexture, new Rect(0, 0, pictureData.width, pictureData.height), new Vector2(0.5f, 0.5f), picture.sprite.pixelsPerUnit);
        paintLayer.sprite = paintSprite;

        ClearCanvas();
        Canvas.ForceUpdateCanvases();
        UpdatePaintLayerToPicture();
    }

    private void UpdatePaintLayerToPicture()
    {
        if (picture == null || paintLayer == null || paintLayer.sprite == null)
        {
            return;
        }

        Camera camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("ColoringManager: No Main Camera found.");
            return;
        }

        RectTransform pictureRect = picture.rectTransform;

        // Get the actual corners of the UI Image after Canvas scaling.
        Vector3[] corners = new Vector3[4]; pictureRect.GetWorldCorners(corners);

        // Screen Space Overlay does not use a camera.
        Vector2 bottomLeftScreen = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector2 topRightScreen = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

        // Determine how far the paint layer is from the camera.
        float distanceFromCamera = Mathf.Abs(camera.transform.position.z - paintLayer.transform.position.z);

        // Convert the UI corners from screen coordinates to world coordinates.
        Vector3 bottomLeftWorld = camera.ScreenToWorldPoint(new Vector3(bottomLeftScreen.x, bottomLeftScreen.y, distanceFromCamera));
        Vector3 topRightWorld = camera.ScreenToWorldPoint(new Vector3(topRightScreen.x, topRightScreen.y, distanceFromCamera));

        // The actual world-space size occupied by the UI Image.
        Vector2 targetWorldSize = new Vector2(Mathf.Abs(topRightWorld.x - bottomLeftWorld.x), Mathf.Abs(topRightWorld.y - bottomLeftWorld.y));

        // The SpriteRenderer's original size before its Transform scale.
        Vector2 spriteSize = paintLayer.sprite.bounds.size;

        // Calculate the scale required to make the SpriteRenderer exactly the same size as the UI Image.
        float scaleX = targetWorldSize.x / spriteSize.x;
        float scaleY = targetWorldSize.y / spriteSize.y;
        paintLayer.transform.localScale = new Vector3(scaleX, scaleY, 1f);

        // Put the SpriteRenderer at the center of the UI Image.
        Vector3 worldCenter = (bottomLeftWorld + topRightWorld) * 0.5f;
        paintLayer.transform.position = new Vector3(worldCenter.x, worldCenter.y, paintLayer.transform.position.z);
    }

    private void ClearCanvas()
    {
        Color[] clearColorArray = new Color[canvasTexture.width * canvasTexture.height];
        for (var i = 0; i < clearColorArray.Length; i++)
        {
            clearColorArray[i] = Color.clear;
        }
        canvasTexture.SetPixels(clearColorArray);
        canvasTexture.Apply();
    }

    public void NextPicture(int index)
    {
        //ClearMaskPaint();
        pictureIndex = index;
        picture.sprite = pictures[pictureIndex];
        picture.rectTransform.sizeDelta = picture.sprite.rect.size;
        InitializeCanvasTexture();
    }

    public void ClearMaskPaint()
    {
        for (int x = 0; x < canvasTexture.width; x++)
        {
            for (int y = 0; y < canvasTexture.height; y++)
            {
                Color maskPixel = removableMask[pictureIndex].GetPixel(x, y);

                if (maskPixel.a > 0)
                {
                    canvasTexture.SetPixel(x, y, Color.clear);
                }
            }
        }

        canvasTexture.Apply();
    }

    private void Paint(Vector2 touchPosition)
    {
        Vector3 screenPosition = new Vector3(touchPosition.x, touchPosition.y, -Camera.main.transform.position.z);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        Bounds bounds = paintLayer.bounds;
        if (!bounds.Contains(worldPosition))
        {
            return;
        }

        float x = Mathf.InverseLerp(bounds.min.x, bounds.max.x, worldPosition.x);
        float y = Mathf.InverseLerp(bounds.min.y, bounds.max.y, worldPosition.y);

        int pixelX = Mathf.RoundToInt(x * canvasTexture.width);
        int pixelY = Mathf.RoundToInt(y * canvasTexture.height);

        int size = Mathf.RoundToInt(brushSize);
        int radius = size / 2;

        for (int i = -radius; i < radius; i++)
        {
            for (int j = -radius; j < radius; j++)
            {
                if (i * i + j * j <= radius * radius)
                {
                    if (pixelX + i >= 0 && pixelX + i < canvasTexture.width && pixelY + j >= 0 && pixelY + j < canvasTexture.height)
                    {
                        Color mainMaskPixel = mainMask[pictureIndex].GetPixel(pixelX + i, pixelY + j);
                        Color removableMaskPixel = removableMask[pictureIndex].GetPixel(pixelX + i, pixelY + j);


                        if (mainMaskPixel.a > 0 || removableMaskPixel.a > 0)
                        {
                            canvasTexture.SetPixel(pixelX + i, pixelY + j, color);
                        }
                    }
                }
            }
        }
        canvasTexture.Apply();
    }

    private void PaintLine(Vector2 start, Vector2 end)
    {
        float distance = Vector2.Distance(start, end);
        int steps = Mathf.CeilToInt(distance / (brushSize / 4));

        for (int i = 0; i < steps; i++)
        {
            float t = (float)i / steps;
            Vector2 position = Vector2.Lerp(start, end, t);

            Paint(position);
        }
    }

    public void SetBrushSize(float newBrushSize)
    {
        brushSize = newBrushSize;
    }

    public void SetColorRed()
    {
        color = Color.red;
    }

    public void SetColorGreen()
    {
        color = Color.green;
    }

    public void SetColorBlue()
    {
        color = Color.blue;
    }

    public void SetColorClear()
    {
        color = Color.clear;
    }
}
