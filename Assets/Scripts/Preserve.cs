using UnityEngine;

public class Preserve : MonoBehaviour
{
    #region Members
    public static Preserve instance;
    public bool againstAI, isLocalOnline;
    public bool isCameraRotatable = true;
    #endregion

    #region Awake
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }
    #endregion

    #region Update
    private void Update()
    {
        if (Application.targetFrameRate != 60)
        {
            Application.targetFrameRate = 60;
        }
    }
    #endregion
}