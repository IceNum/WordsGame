using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CameraScale : MonoBehaviour
{
    private float minSize;
    public float maxSize;
    public float currentSize;
    public Vector3 startPos;
    public Vector3 prevPos;
    public Vector3 currentPos;
    public Vector3 nextPos;
    public float xMin, xMax;
    public float yMin, yMax;
    public bool zoomed;
    private float dCenter;
    private Camera _camera;

    void Start()
    {
        _camera = Camera.main;
        float screenHeight = Screen.height;
        float screenWidth = Screen.width;
        float boardSize = 15.0f;
        dCenter = ((screenHeight - screenWidth) / 2) * (7.5f/screenWidth);

        _camera.orthographicSize = currentSize = maxSize = boardSize * screenHeight / screenWidth * 0.5f;
        minSize = maxSize / 2;

        float viewHalfHeight = Mathf.Abs(_camera.ViewportToWorldPoint(Vector3.zero).y - _camera.ViewportToWorldPoint(Vector3.one).y) / 4;
        float viewHalfWidth = viewHalfHeight / (screenHeight / screenWidth) / 2;

        float deltaY = (boardSize / 2) - (viewHalfHeight * 2);
        transform.position = nextPos = startPos = new Vector3(0, deltaY, -10);

        xMin = -viewHalfWidth * 2;
        xMax = viewHalfWidth * 2;

        yMin = -viewHalfHeight;
        yMax = (boardSize / 2) - viewHalfHeight;
    }

    void Update()
    {

        _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, currentSize, 20*Time.deltaTime);

#if UNITY_ANDROID || UNITY_IPHONE
        MobileInput();
#endif

#if UNITY_EDITOR || UNITY_WEBGL
        MouseInput();
#endif

        //Dragging camera
        if (Input.GetMouseButtonDown(0))
        {
            currentPos = prevPos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            if (GameController.data.uiTouched || GameController.data.letterDragging)
                return;
            currentPos = Input.mousePosition;
            Vector3 deltaPosition = currentPos - prevPos;
            prevPos = currentPos;
            float screenfactor = 7.5f / Screen.width;
            nextPos -= deltaPosition * screenfactor;
            nextPos.z = -10;
        }

        if (Input.GetMouseButtonUp(0))
            currentPos = prevPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        UpdateCamPosition();
    }


    public void ResizeCam(float size, Vector3 center)
    {
        if (size == currentSize)
            return;

        zoomed = size == maxSize ? false : true;

        currentSize = size;
        center.z = -10;
        center.y -= dCenter;
        nextPos = center;
    }

    public void ZoomIn(Vector3 center)
    {
        ResizeCam(minSize, center);
    }

    public void UpdateCamPosition()
    {
        if (zoomed)
        {
            nextPos.x = Mathf.Clamp(nextPos.x, xMin, xMax);
            nextPos.y = Mathf.Clamp(nextPos.y, yMin, yMax);
        }
        else {
            nextPos = startPos;
        }

        transform.position = Vector3.Lerp(transform.position, nextPos, 17.2f * Time.deltaTime);
    }

    void MobileInput()
    {
        if (GameController.data.uiTouched || GameController.data.letterDragging)
            return;

        if (Swiper.instance.pinchIn)
            ResizeCam(maxSize, startPos);
        if (Swiper.instance.pinchOut)
            ResizeCam(minSize, _camera.ScreenToWorldPoint(Input.mousePosition));
        if (Swiper.instance.doubleTap)
            ResizeCam(currentSize == minSize ? maxSize : minSize, _camera.ScreenToWorldPoint(Swiper.instance.lastTouchPos));
    }

    void MouseInput()
    {
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
            ResizeCam(maxSize, Vector3.zero);
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
            ResizeCam(minSize, _camera.ScreenToWorldPoint(Input.mousePosition));
    }
}