using UnityEngine;

[CreateAssetMenu(menuName = "Manager/Game Settings")]
public class GameSettings : ScriptableObject
{
    [SerializeField] private string gameVersion = "1.0.0";
    [SerializeField] private string nickName;
    [SerializeField] private int maxRoomCount = 10;
    public string GameVersion { get { return gameVersion; } }
    public string NickName {
        get {
            if (AuthenticationManager.instance != null)
            {
                return AuthenticationManager.instance.auth.CurrentUser.UserId;
            }

            return "";
        }
    }
    public int MaxRoomCount { get { return maxRoomCount; } }
}
