using UnityEngine;

public class BoardCameraMovement : MonoBehaviour
{
    #region Members
    public GameObject boardObject;
    private bool goingUp, goingDown, speedingUp, speedingDown;
    public bool movementAllowed;
    public float rotationSpeed, translationSpeed;
    #endregion

    #region Start
    void Start()
    {
        rotationSpeed = 60.0f;
        translationSpeed = 2.5f;
        speedingUp = movementAllowed = true; speedingDown = false;
    }
    #endregion

    #region Update
    void Update()
    {
        if (movementAllowed)
        {
            gameObject.transform.RotateAround(boardObject.transform.position - new Vector3(31, 0, 0), Vector3.up, Time.deltaTime * rotationSpeed);
            if (gameObject.transform.position.y >= 145.0f)
            {
                goingUp = true;
                goingDown = false;
            }
            if (gameObject.transform.position.y <= 140.0f)
            {
                goingDown = true;
                goingUp = false;
            }
            if (goingUp)
            {
                gameObject.transform.Translate(translationSpeed * Time.deltaTime * Vector3.down);
            }
            else if (goingDown)
            {
                gameObject.transform.Translate(translationSpeed * Time.deltaTime * Vector3.up);
            }
            if (rotationSpeed <= 30.0f)
            {
                speedingUp = true;
                speedingDown = false;
            }
            if (rotationSpeed >= 80.0f)
            {
                speedingDown = true;
                speedingUp = false;
            }
            if (speedingUp)
            {
                rotationSpeed += 10.0f * Time.deltaTime;
                translationSpeed += 0.25f * Time.deltaTime;
            }
            else if (speedingDown)
            {
                rotationSpeed -= 10.0f * Time.deltaTime;
                translationSpeed -= 0.25f * Time.deltaTime;
            }
        }
    }
    #endregion
}