using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#region Enums
public enum SceneIndex
{
    INITIAL = 0,
    AUTH_SCREEN = 1,
    MAIN_MENU = 2,
    GAME_SCREEN = 3
}
#endregion

public class SceneHandler : MonoBehaviour
{
    #region Members
    public static SceneHandler instance;

    public GameObject loadingScreenPrefab;
    private GameObject loadingScreen;
    private ProgressBar progressBar;

    public List<AsyncOperation> scenesLoading = new List<AsyncOperation>();
    private float totalSceneProgress;
    private float totalSpawnProgress;
    #endregion

    #region Awake
    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this);
        SceneManager.activeSceneChanged += OnSceneActivation;
        StartCoroutine(LoadAndActivateScene((int)SceneIndex.AUTH_SCREEN));
    }
    #endregion

    #region LoadScene
    public void LoadScene(int indexToLoad, int[] indexesToUnload)
    {
        if (loadingScreen == null)
            loadingScreen = Instantiate(loadingScreenPrefab, null);
        progressBar = loadingScreen.GetComponentInChildren<ProgressBar>();
        StartCoroutine(LoadAndActivateScene(indexToLoad, indexesToUnload));
        if (indexToLoad == (int)SceneIndex.GAME_SCREEN)
        {
            StartCoroutine(GetSceneLoadProgress());
            StartCoroutine(GetTotalProgress());
        }
        else if (indexToLoad == (int)SceneIndex.MAIN_MENU)
            StartCoroutine(GetSceneLoadProgress(indexToLoad));
    }
    #endregion

    #region Scene Activation
    public IEnumerator LoadAndActivateScene(int loadIndex, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        scenesLoading.Add(SceneManager.LoadSceneAsync(loadIndex, mode));
        yield return new WaitUntil(() => scenesLoading[scenesLoading.Count - 1].isDone);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(loadIndex));
        if (loadIndex == (int)SceneIndex.AUTH_SCREEN)
            AuthenticationManager.instance.UserConnection += LoadMainMenuScene;
    }
    public IEnumerator LoadAndActivateScene(int loadIndex, int[] unloadIndexes, LoadSceneMode mode = LoadSceneMode.Additive)
    {
        foreach (int buildIndex in unloadIndexes)
        {
            if (SceneManager.GetSceneByBuildIndex(buildIndex).isLoaded)
                scenesLoading.Add(SceneManager.UnloadSceneAsync(buildIndex));
        }
        scenesLoading.Add(SceneManager.LoadSceneAsync(loadIndex, mode));
        yield return new WaitUntil(() => scenesLoading[scenesLoading.Count - 1].isDone);
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(loadIndex));
        if (loadIndex == (int)SceneIndex.AUTH_SCREEN)
            AuthenticationManager.instance.UserConnection += LoadMainMenuScene;
    }

    public Scene GetActiveScene()
    {
        return SceneManager.GetActiveScene();
    }

    public Scene MapIndexToScene(SceneIndex index)
    {
        return SceneManager.GetSceneByBuildIndex((int)index);
    }

    public SceneIndex MapSceneToIndex(Scene scene)
    {
        int index = scene.buildIndex;
        return (SceneIndex)index;
    }

    private void OnSceneActivation(Scene current, Scene next)
    {
        if (MapSceneToIndex(next) == SceneIndex.MAIN_MENU)
        {
            PunServer.instance.ConnectToMaster();
        }
    }
    #endregion

    #region Loading Progress
    public IEnumerator GetSceneLoadProgress()
    {
        for (int i = 0; i < scenesLoading.Count; i++)
        {
            while (!scenesLoading[i].isDone)
            {
                totalSceneProgress = 0;
                foreach (AsyncOperation operation in scenesLoading)
                {
                    totalSceneProgress += operation.progress;
                }
                totalSceneProgress *= 100f / scenesLoading.Count;
                yield return null;
            }
        }
    }

    public IEnumerator GetSceneLoadProgress(int desiredSceneIndex)
    {
        for (int i = 0; i < scenesLoading.Count; i++)
        {
            while (!scenesLoading[i].isDone)
            {
                totalSceneProgress = 0;
                foreach (AsyncOperation operation in scenesLoading)
                {
                    totalSceneProgress += operation.progress;
                }
                totalSceneProgress *= 100f / scenesLoading.Count;
                yield return null;
            }
        }
        while (SceneManager.GetActiveScene().buildIndex != desiredSceneIndex)
        {
            yield return null;
        }
        Destroy(loadingScreen);
        loadingScreen = null;
        yield return new WaitForSeconds(0.5f);
        progressBar.current = 0;
    }

    public IEnumerator GetTotalProgress()
    {
        while (Board.instance == null || PieceManager.instance == null || !Board.instance.isSetupDone || !PieceManager.instance.isSetupDone)
        {
            if (Board.instance == null || PieceManager.instance == null)
            {
                totalSpawnProgress = 0;
            }
            else
            {
                totalSpawnProgress = Mathf.RoundToInt((Board.instance.setupProgress + PieceManager.instance.setupProgress) * 50f);
            }
            progressBar.current = Mathf.RoundToInt((totalSceneProgress + totalSpawnProgress) / 2f);
            yield return null;
        }
        Destroy(loadingScreen);
        loadingScreen = null;
        progressBar.current = 0;
    }
    #endregion

    #region LoadMainMenuScene
    private void LoadMainMenuScene()
    {
        int[] indexesToUnload = new int[1] { (int)SceneIndex.AUTH_SCREEN };
        AuthenticationManager.instance.UserConnection -= LoadMainMenuScene;
        MainThreadManager.worker.AddJob(() => LoadScene((int)SceneIndex.MAIN_MENU, indexesToUnload));
    }
    #endregion

    #region OnDestroy
    private void OnDestroy()
    {
        if (AuthenticationManager.instance != null)
        {
            AuthenticationManager.instance.UserConnection -= LoadMainMenuScene;
        }

        SceneManager.activeSceneChanged -= OnSceneActivation;
    }
    #endregion
}
