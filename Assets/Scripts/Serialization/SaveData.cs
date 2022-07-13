[System.Serializable]
public class SaveData
{
    private static SaveData instance;
    public static SaveData Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SaveData();
            }
            return instance;
        }
        set
        {
            if (value != null)
            {
                instance = value;
            }
        }
    }
    public GameData gameData = new GameData();
}
