using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    #region Members
    public Transform cameraTransform, targetTransform;
    public float movementSpeed, movementTime, rotationAmount, initialDistance, pinchCoefficient;
    public float touchCoefficient;
    public Vector3 originalPosition, originalRotation, originalZoom;
    public Vector3 nextPosition, nextZoom, zoomAmount;
    public Vector3 dragStartPosition, dragCurrentPosition;
    private Touch initialTouch = new Touch();
    public bool translatingTouch, rotatingTouch;
    private bool canMove = true;
    #endregion

    #region Start
    void Start()
    {
        originalPosition = nextPosition = transform.position;
        originalRotation = transform.rotation.eulerAngles;
        originalZoom = nextZoom = cameraTransform.localPosition;
    }
    #endregion

    #region Update
    void Update()
    {
        HandleMovementInput();
    }
    #endregion

    #region HandleMovementInput
    private void HandleMovementInput()
    {
        #region Windows
        if (ApplicationUtil.Platform == RuntimePlatform.WindowsPlayer)
        {
            if (Input.mouseScrollDelta.y != 0)
            {
                nextZoom += Input.mouseScrollDelta.y * zoomAmount;
            }
            if (Input.GetMouseButtonDown(0))
            {
                dragStartPosition = GetPlanePosition(Input.mousePosition);
            }
            if (Input.GetMouseButton(0))
            {
                dragCurrentPosition = GetPlanePosition(Input.mousePosition);
                Vector3 translation = dragStartPosition - dragCurrentPosition;
                nextPosition = transform.position + translation;
            }
            if (Input.GetMouseButton(1))
            {
                float yawAmount = Mathf.Clamp(-Input.GetAxis("Mouse Y") * rotationAmount, -3.0f, 3.0f);
                float pitchAmount = Mathf.Clamp(Input.GetAxis("Mouse X") * rotationAmount, -5.0f, 5.0f);
                transform.Rotate(transform.right, yawAmount, Space.World);
                //transform.RotateAround(targetTransform.position - new Vector3(31.0f, 0f, 0f), Vector3.up, pitchAmount);
                transform.RotateAround(transform.position, Vector3.up, pitchAmount);
            }
            if (Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.O))
            {
                ResetCameraTransform();
            }
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                nextPosition += transform.forward * movementSpeed;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                nextPosition += transform.forward * -movementSpeed;
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                nextPosition += transform.right * -movementSpeed;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                nextPosition += transform.right * movementSpeed;
            }
            if (Input.GetKey(KeyCode.R) && nextZoom.y > 1f)
            {
                nextZoom += zoomAmount;
            }
            if (Input.GetKey(KeyCode.F) && nextZoom.y < 110f)
            {
                nextZoom -= zoomAmount;
            }
        }
        #endregion
        #region Android/OSX
        else if (ApplicationUtil.Platform == RuntimePlatform.Android || ApplicationUtil.Platform == RuntimePlatform.OSXPlayer)
        {
            if (Input.touchCount == 1 && canMove)
            {
                Touch currentTouch = Input.GetTouch(0);
                if (currentTouch.phase == TouchPhase.Began)
                {
                    if (translatingTouch)
                    {
                        dragStartPosition = GetPlanePosition(currentTouch.position);
                    }
                    else if (rotatingTouch)
                    {
                        initialTouch = currentTouch;
                    }
                }
                else if (currentTouch.phase == TouchPhase.Moved)
                {
                    if (translatingTouch)
                    {
                        dragCurrentPosition = GetPlanePosition(currentTouch.position);
                        Vector3 translation = dragStartPosition - dragCurrentPosition;
                        nextPosition = transform.position + translation;
                    }
                    else if (rotatingTouch)
                    {
                        float deltaX = initialTouch.position.x - currentTouch.position.x;
                        float deltaY = initialTouch.position.y - currentTouch.position.y;
                        float yawAmount = deltaY * Time.deltaTime * rotationAmount * touchCoefficient;
                        float pitchAmount = -deltaX * Time.deltaTime * rotationAmount * touchCoefficient;
                        transform.Rotate(transform.right, yawAmount, Space.World);
                        //transform.RotateAround(targetTransform.position - new Vector3(31.0f, 0f, 0f), Vector3.up, pitchAmount);
                        transform.RotateAround(transform.position, Vector3.up, pitchAmount);
                    }
                }
            }
            else if (Input.touchCount == 2)
            {
                canMove = false;
                Vector3 firstTouchPosition = GetPlanePosition(Input.GetTouch(0).position);
                Vector3 secondTouchPosition = GetPlanePosition(Input.GetTouch(1).position);
                Vector3 lastFirstTouchPosition = GetPlanePosition(Input.GetTouch(0).position - Input.GetTouch(0).deltaPosition);
                Vector3 lastSecondTouchPosition = GetPlanePosition(Input.GetTouch(1).position - Input.GetTouch(1).deltaPosition);
                float lastDistance = Vector3.Distance(lastFirstTouchPosition, lastSecondTouchPosition);
                nextZoom += (Vector3.Distance(firstTouchPosition, secondTouchPosition) - lastDistance) * pinchCoefficient * zoomAmount;
            }
            if (Input.touchCount == 0)
            {
                canMove = true;
            }
        }
        #endregion
        #region Smoothing
        nextPosition.x = Mathf.Clamp(nextPosition.x, -50.0f, 50.0f);
        nextPosition.z = Mathf.Clamp(nextPosition.z, -30.0f, 30.0f);
        nextZoom.y = Mathf.Clamp(nextZoom.y, 1f, 110f);
        transform.position = Vector3.Lerp(transform.position, nextPosition, movementTime * Time.deltaTime);
        if ((transform.position - nextPosition).sqrMagnitude <= 0.01f)
        {
            transform.position = nextPosition;
        }
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, nextZoom, movementTime * Time.deltaTime);
        if ((cameraTransform.localPosition - nextZoom).sqrMagnitude <= 0.01f)
        {
            cameraTransform.localPosition = nextZoom;
        }
        #endregion
    }
    #endregion

    #region ClampRotationAroundXAxis
    private Quaternion ClampRotationAroundXAxis(Quaternion q, float minValue, float maxValue)
    {
        q.x /= q.w; q.y /= q.w; q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);
        angleX = Mathf.Clamp(angleX, minValue, maxValue);
        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }
    #endregion

    #region GetPlanePosition
    private Vector3 GetPlanePosition(Vector3 screenPosition)
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray;
        Camera cam = cameraTransform.GetComponent<Camera>();
        if (cam != null)
        {
            ray = cam.ScreenPointToRay(screenPosition);
            if (plane.Raycast(ray, out float entry))
            {
                return ray.GetPoint(entry);
            }
            return Vector3.zero;
        }
        return Vector3.zero;
    }
    #endregion

    #region ResetCameraTransform
    public void ResetCameraTransform()
    {
        nextPosition = originalPosition;
        nextZoom = originalZoom;
        transform.LeanRotate(originalRotation, 10f * Time.deltaTime);
    }
    #endregion
}
