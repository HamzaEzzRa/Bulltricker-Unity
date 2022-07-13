using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    #region Members
    public static DatabaseManager instance;
    public DatabaseReference reference;
    #endregion

    #region Awake
    private void Awake()
    {
        if (instance == null)
            instance = this;
        DontDestroyOnLoad(this);
        AuthenticationManager.instance.UserConnection += OnFirebaseAuthentication;
    }
    #endregion

    #region OnFirebaseAuthentication
    public void OnFirebaseAuthentication()
    {
        CheckFirebaseDependencies();
        reference = GetDatabaseReference("https://bulltricker-f54ab.firebaseio.com/");
        Firebase.Auth.FirebaseUser currentUser = AuthenticationManager.instance.auth.CurrentUser;
        reference.Child("Users").Child(currentUser.UserId).GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("Database Error, " + task.Exception);
                // Error Handling ...
            }
            else if (task.IsCompleted)
            {
                if (!task.Result.Exists)
                {
                    Dictionary<string, string> userData = new Dictionary<string, string>()
                    {
                        { "Email", currentUser.Email },
                        { "Username", currentUser.DisplayName },
                        // { "Ranking", PlayerManager.instance.InitialRank },
                        // { "Gold", PlayerManager.instance.InitialGold }
                    };
                    reference.Child("Users").Child(currentUser.UserId).SetValueAsync(userData).ContinueWith(second_task =>
                    {
                        if (second_task.IsFaulted)
                        {
                            Debug.Log("Database Error, " + second_task.Exception);
                            // Error Handling ...
                        }
                        else
                            Debug.Log("User data was successfully created");
                    });
                }
                else
                    Debug.Log(task.Result);
                AuthenticationManager.instance.UserConnection -= OnFirebaseAuthentication;
            }
        });
    }
    #endregion

    #region CheckFirebaseDependencies
    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result != DependencyStatus.Available)
                    Debug.Log("Could not resolve all Firebase dependencies: " + task.Result.ToString());
            }
            else
                Debug.Log("Dependency check was not completed. Error: " + task.Exception.Message);
        });
    }
    #endregion

    #region GetDatabaseReference
    private DatabaseReference GetDatabaseReference(string databaseUrl)
    {
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = new System.Uri(databaseUrl);
        return FirebaseDatabase.DefaultInstance.RootReference;
    }
    #endregion

    #region OnDestroy
    private void OnDestroy()
    {
        AuthenticationManager.instance.UserConnection -= OnFirebaseAuthentication;
    }
    #endregion
}
