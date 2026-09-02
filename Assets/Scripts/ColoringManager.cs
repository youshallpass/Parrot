using UnityEngine;
using UnityEngine.UI;

public class ColoringManager : MonoBehaviour
{
    [SerializeField] private Sprite[] pictures;
    [SerializeField] private Image picture;
    [SerializeField] private SpriteRenderer paintLayer;
    private Texture2D[] canvasTextures;
    private Texture2D canvasTexture;
    private int pictureIndex;
    
    public float brushSize = 20f;
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

    public void NextPicture()
    {
        pictureIndex++;
        //if (pictureIndex >= pictures.Length)
        //{
        //    pictureIndex = 0;
        //}
        picture.sprite = pictures[pictureIndex];
        InitializeCanvasTexture();
    }

    private void Paint(Vector2 touchPosition)
    {
        Vector3 screenPosition = new Vector3(touchPosition.x, touchPosition.y, -Camera.main.transform.position.z);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        if (!picture.sprite.bounds.Contains(worldPosition))
        {
            return;
        }

        float x = Mathf.InverseLerp(picture.sprite.bounds.min.x, picture.sprite.bounds.max.x, worldPosition.x);
        float y = Mathf.InverseLerp(picture.sprite.bounds.min.y, picture.sprite.bounds.max.y, worldPosition.y);

        int pixelX = Mathf.RoundToInt(x * canvasTexture.width);
        int pixelY = Mathf.RoundToInt(y * canvasTexture.height);

        int size = Mathf.RoundToInt(brushSize);
        int radius = size / 2;

        for (int i = -radius; i < radius;  i++)
        {
            for (int j = -radius; j < radius; j++)
            {
                if (i * i + j * j <= radius * radius)
                {
                    if (pixelX + i >= 0 && pixelX + i < canvasTexture.width && pixelY + j >= 0 && pixelY + j < canvasTexture.height)
                    {
                        canvasTexture.SetPixel(pixelX + i, pixelY + j, color);
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
