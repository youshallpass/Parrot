using UnityEngine;

public class PanZoom : MonoBehaviour
{
    [SerializeField] GameObject boundsObject;
    
    [SerializeField] float zoomOutMin = 1;
    [SerializeField] float zoomOutMax = 8;
    
    private Vector3 touchStart;

    private Bounds objectBounds;
    private Vector3 targetPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objectBounds = boundsObject.GetComponent<SpriteRenderer>().bounds;
    }

    // Update is called once per frame
    void Update()
    {
        TouchInput();
    }

    private void TouchInput()
    {
        targetPosition = Camera.main.transform.position;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStart = Camera.main.ScreenToWorldPoint(touch.position);
            }
            if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                float difference = currentMagnitude - prevMagnitude;

                Zoom((float)(difference * 0.01));
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(touch.position);
                targetPosition += direction;
            }
        }

        Camera.main.transform.position = GetCameraBounds(targetPosition);
    }

    private void Zoom(float increment)
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, zoomOutMin, zoomOutMax);
    }

    private Vector3 GetCameraBounds(Vector3 targetPos)
    {
        float height = Camera.main.orthographicSize;
        float width = height * Camera.main.aspect;

        float minX = objectBounds.min.x + width;
        float maxX = objectBounds.max.x - width;
        float minY = objectBounds.min.y + height;
        float maxY = objectBounds.max.y - height;

        if (minX > maxX)
        {
            minX = maxX = (objectBounds.min.x + objectBounds.max.x) / 2f;
        }
        if (minY > maxY)
        {
            minY = maxY = (objectBounds.min.y + objectBounds.max.y) / 2f;
        }

        return new Vector3(
            Mathf.Clamp(targetPos.x, minX, maxX),
            Mathf.Clamp(targetPos.y, minY, maxY),
            Camera.main.transform.position.z
            );
    }
}
