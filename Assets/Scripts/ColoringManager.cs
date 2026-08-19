using UnityEngine;

public class ColoringManager : MonoBehaviour
{
    [SerializeField] private Color color = Color.red;
    [SerializeField] private SpriteRenderer picture;
    [SerializeField] private SpriteRenderer paintLayer;
    public float brushSize = 20f;
    private Texture2D canvasTexture;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeCanvasTexture();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            Vector2 touchPosition = touch.position;
            Paint(touchPosition);
        }

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            Paint(Input.mousePosition);
        }
#endif
    }

    private void InitializeCanvasTexture()
    {
        canvasTexture = new Texture2D(picture.sprite.texture.width, picture.sprite.texture.height, TextureFormat.RGBA32, false);
        canvasTexture.filterMode = FilterMode.Bilinear;

        Sprite paintSprite = Sprite.Create(canvasTexture, new Rect(0, 0, picture.sprite.texture.width, picture.sprite.texture.height), new Vector2(0.5f, 0.5f), picture.sprite.pixelsPerUnit);
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

    public void SetBrushSize(float newBrushSize)
    {
        brushSize = newBrushSize;
    }

    private void Paint(Vector2 touchPosition)
    {
        Vector3 screenPosition = new Vector3(touchPosition.x, touchPosition.y, -Camera.main.transform.position.z);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

        if (!picture.bounds.Contains(worldPosition))
        {
            return;
        }

        float x = Mathf.InverseLerp(picture.bounds.min.x, picture.bounds.max.x, worldPosition.x);
        float y = Mathf.InverseLerp(picture.bounds.min.y, picture.bounds.max.y, worldPosition.y);

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
