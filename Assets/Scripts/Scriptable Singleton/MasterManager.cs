using UnityEngine;

[CreateAssetMenu(menuName = "Singletons/Master Manager")]
public class MasterManager : SingletonScriptableObject<MasterManager>
{
    [SerializeField] private GameSettings gameSettings = null;
    public static GameSettings GameSettings { get { return Instance.gameSettings; } }
}
