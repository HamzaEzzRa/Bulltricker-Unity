using UnityEngine;

public class EventCodes : MonoBehaviour
{
    #region Members
    public static EventCodes instance;
    public static byte PIECE_MOVE { get; private set; } = 0;
    public static byte PIECE_UP { get; private set; } = 1;
    public static byte PIECE_DOWN { get; private set; } = 2;
    public static byte PIECE_UP_EASED { get; private set; } = 3;
    public static byte PIECE_DOWN_EASED { get; private set; } = 4;
    #endregion

    #region Awake
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(transform.gameObject);
    }
    #endregion
}
