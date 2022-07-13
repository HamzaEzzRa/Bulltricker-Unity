using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Google;
using Facebook.Unity;
using UnityEngine;
using TMPro;

public class AuthenticationManager : MonoBehaviour
{
    #region Members
    public static AuthenticationManager instance;
    public event Action UserConnection;

    private bool isLogShown;
    public GameObject logView;
    public TMP_Text viewText;
    public TMP_Text buttonText;
    private int logLineCount;
    public string infoText;

    public GameObject connectionWaitPrefab;
    private GameObject connectionWait;

    public string webClientId = "421364313004-j08ag2ptdese0e41seikin8ojunpb8fn.apps.googleusercontent.com";
    public FirebaseAuth auth;
    private GoogleSignInConfiguration googleConfiguration;
    #endregion

    #region Awake
    private void Awake()
    {
        instance = this;
        viewText = logView.gameObject.GetComponentInChildren<TMP_Text>();
        googleConfiguration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
        CheckFirebaseDependencies();
        if (!FB.IsInitialized)
            FB.Init(InitCallback, OnHideUnity);
        else
        {
            FB.ActivateApp();
            try
            {
                AddToLog("Calling Facebook Express Login");
                FacebookExpressLogin();
            }
            catch (Exception facebookException)
            {
                AddToLog("Failed To Sign In Silently With Facebook. " + facebookException);
                if (connectionWait != null)
                {
                    Destroy(connectionWait);
                    connectionWait = null;
                }
            }
        }
    }
    #endregion

    #region CheckFirebaseDependencies
    private void CheckFirebaseDependencies()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                if (task.Result == DependencyStatus.Available)
                {
                    auth = FirebaseAuth.DefaultInstance;
                    MainThreadManager.worker.AddJob(() =>
                    {
                        try
                        {
                            SignInSilentlyWithGoogle();
                        }
                        catch (Exception googleException)
                        {
                            AddToLog("Failed To Call Google Silent Sign In. " + googleException);
                        }
                    });
                }
                else
                {
                    AddToLog("Could not resolve all Firebase dependencies: " + task.Result.ToString());
                }   
            }
            else
            {
                AddToLog("Dependency check was not completed. Error: " + task.Exception.Message);
            }
        });
    }
    #endregion

    #region Google Authentication
    public void SignInWithGoogle() { OnGoogleSignIn(); }
    public void SignOutFromGoogle() { OnGoogleSignOut(); }

    private void OnGoogleSignIn()
    {
        GoogleSignIn.Configuration = googleConfiguration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToLog("Calling SignIn");
        try
        {
            GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
        }
        catch (Exception exception)
        {
            AddToLog("Failed To Sign In With Google. " + exception);
            if (connectionWait != null)
            {
                Destroy(connectionWait);
                connectionWait = null;
            }
        }
    }

    private void OnGoogleSignOut()
    {
        AddToLog("Calling SignOut");
        try
        {
            GoogleSignIn.DefaultInstance.SignOut();
        }
        catch (Exception exception)
        {
            AddToLog("Failed To Sign Out With Google. " + exception);
        }
    }

    public void OnDisconnect()
    {
        AddToLog("Calling Disconnect");
        GoogleSignIn.DefaultInstance.Disconnect();
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    AddToLog("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    AddToLog("Got Unexpected Exception: " + task.Exception);
                }
            }
            if (connectionWait != null)
            {
                Destroy(connectionWait);
                connectionWait = null;
            }
        }
        else if (task.IsCanceled)
        {
            AddToLog("Canceled");
            if (connectionWait != null)
            {
                Destroy(connectionWait);
                connectionWait = null;
            }
        }
        else
        {
            AddToLog("Welcome: " + task.Result.DisplayName);
            AddToLog("Email = " + task.Result.Email);
            AddToLog("Google ID Token = " + task.Result.IdToken.Substring(0, 12) + "...") ;
            try
            {
                SignInWithGoogleOnFirebase(task.Result.IdToken);
            }
            catch (Exception exception)
            {
                AddToLog("Failed To Login Silently With Google Account. " + exception);
                if (connectionWait != null)
                {
                    Debug.Log("Here 2");
                    Destroy(connectionWait);
                    connectionWait = null;
                }
            }
        }
    }

    private void SignInWithGoogleOnFirebase(string idToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        AddToLog(credential.ToString());
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            AggregateException ex = task.Exception;
            if (ex != null)
            {
                if (ex.InnerExceptions[0] is FirebaseException inner && (inner.ErrorCode != 0))
                    AddToLog("Error code = " + inner.ErrorCode + " Message = " + inner.Message);
                if (connectionWait != null)
                {
                    Destroy(connectionWait);
                    connectionWait = null;
                }
            }
            else
            {
                AddToLog("Sign In Successful. UID: " + auth.CurrentUser.UserId);
                UserConnection?.Invoke();
                // LoadMainMenuScene();
            }
            AddToLog(ex.ToString());
        });
    }

    public void SignInSilentlyWithGoogle()
    {
        if (connectionWait == null)
            connectionWait = Instantiate(connectionWaitPrefab, null);
        GoogleSignIn.Configuration = googleConfiguration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        AddToLog("Calling SignIn Silently");
        try
        {
            GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
        }
        catch (Exception exception)
        {
            AddToLog("Failed To Login Silently With Google Account. " + exception);
            if (connectionWait != null)
            {
                Destroy(connectionWait);
                connectionWait = null;
            }
        }
    }

    public void OnGamesSignIn()
    {
        GoogleSignIn.Configuration = googleConfiguration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;

        AddToLog("Calling Games SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }
    #endregion

    #region Facebook Authentication
    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
            try
            {
                AddToLog("Calling Facebook Express Login");
                FacebookExpressLogin();
            }
            catch (Exception facebookException)
            {
                AddToLog("Failed To Sign In Silently With Facebook. " + facebookException);
                if (connectionWait != null)
                {
                    Destroy(connectionWait);
                    connectionWait = null;
                }
            }
        }
        else
        {
            AddToLog("Failed To Initialize The Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void SignInWithFacebook() { OnFacebookSignIn(); }

    public void SignOutFromFacebook() { OnFacebookSignOut(); }

    private void OnFacebookSignIn()
    {
        List<string> permissions = new List<string>() { "public_profile", "email" };
        AddToLog("Calling SignIn");
        try
        {
            FB.LogInWithReadPermissions(permissions, OnFacebookSignInCallback);
        }
        catch (Exception exception)
        {
            AddToLog("Failed To Sign In With Facebook. " + exception);
        }
    }

    private void OnFacebookSignOut()
    {
        AddToLog("Calling SignOut");
        try
        {
            FB.LogOut();
        }
        catch (Exception exception)
        {
            AddToLog("Failed To Sign Out With Facebook. " + exception);
        }
    }

    private void OnFacebookSignInCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            AccessToken accessToken = AccessToken.CurrentAccessToken;
            AddToLog("User Id: " + accessToken.UserId);
            AddToLog("Permissions:");
            foreach (string permission in accessToken.Permissions)
            {
                AddToLog("\t" + permission);
            }
            SignInWithFacebookOnFirebase(accessToken);
        }
        else
        {
            AddToLog("User Cancelled Login");
        }
    }

    private void FacebookExpressLogin()
    {
        if (connectionWait == null)
            connectionWait = Instantiate(connectionWaitPrefab, null);
        FB.Android.RetrieveLoginStatus(ExpressLoginCallback);
    }

    private void ExpressLoginCallback(ILoginStatusResult result)
    {
        if (!string.IsNullOrEmpty(result.Error))
        {
            AddToLog("Error: " + result.Error);
            if (connectionWait != null)
            {
                Destroy(connectionWait);
                connectionWait = null;
            }
        }
        else if (result.Failed)
        {
            AddToLog("Failure: Access Token Could Not Be Retrieved");
            if (connectionWait != null)
            {
                Destroy(connectionWait);
                connectionWait = null;
            }
        }
        else
        {
            AddToLog("Success: " + result.AccessToken.UserId);
            SignInWithFacebookOnFirebase(result.AccessToken);
        }
    }

    private void SignInWithFacebookOnFirebase(AccessToken accessToken)
    {
        Credential credential = FacebookAuthProvider.GetCredential(accessToken.TokenString);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
            if (task.IsCanceled)
            {
                AddToLog("Got Error With Firebase Facebook SignIn. SignInWithCredentialAsync Was Canceled.");
                if (connectionWait != null)
                {
                    Destroy(connectionWait);
                    connectionWait = null;
                }
                return;
            }
            if (task.IsFaulted)
            {
                AddToLog("Got Error With Firebase Facebook SignIn. SignInWithCredentialAsync Encountered An Error: " + task.Exception);
                if (connectionWait != null)
                {
                    Destroy(connectionWait);
                    connectionWait = null;
                }
                return;
            }
            FirebaseUser newUser = task.Result;
            AddToLog("Welcome: " + newUser.DisplayName);
            AddToLog("User Id: " + newUser.UserId);
            UserConnection?.Invoke();
            // LoadMainMenuScene();
        });
    }
    #endregion

    #region Guest Authentication
    public void ContinueAsGuest()
    {
        auth.SignInAnonymouslyAsync().ContinueWith(task => {
            if (task.IsCanceled)
            {
                AddToLog("Got An Error With Anonymous Sign In. SignInAnonymouslyAsync Was Canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                AddToLog("Got An Error With Anonymous Sign In. SignInAnonymouslyAsync Encountered An Error: " + task.Exception);
                return;
            }
            FirebaseUser newUser = task.Result;
            AddToLog("Welcome Guest");
            UserConnection?.Invoke();
            // LoadMainMenuScene();
        });
    }
    #endregion

    #region Log Handling
    public void AddToLog(string str)
    {
        logLineCount++;
        infoText += "\n" + logLineCount + ". " + (str.Length > 500 ? (str.Substring(0, 500) + "...") : str);
    }

    public void HandleLog()
    {
        if (!isLogShown)
        {
            logView.gameObject.SetActive(true);
            viewText.SetText(infoText);
            buttonText.SetText("Hide Log");
            isLogShown = true;
        }
        else
        {
            logView.gameObject.SetActive(false);
            buttonText.SetText("Show Log");
            isLogShown = false;
        }
    }

    public void ClearLog()
    {
        logLineCount = 0;
        infoText = "";
        viewText.SetText(infoText);
    }
    #endregion
}
