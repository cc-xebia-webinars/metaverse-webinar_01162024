using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using System.Threading.Tasks;

[ExecuteInEditMode]
public class AdminAuthManager : MonoBehaviour
{
    public string username;
    public string password;
    FirebaseAuth auth;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
      InitializeFirebase();
    }

    void Login()
    {
      auth.SignInWithEmailAndPasswordAsync(username, password).ContinueWith(task => {

        if (task.IsCanceled)
        {
          Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
          return;
        }

        if (task.IsFaulted)
        {
          Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
          return;
        }

        FirebaseUser newUser = task.Result.User;
        Debug.LogFormat("User signed in successfully: {0} ({1})",
            newUser.DisplayName, newUser.UserId);
      });

    }
    void InitializeFirebase()
    {
      if (auth is null || auth?.CurrentUser is null)
      {
        auth = FirebaseAuth.DefaultInstance;
        Login();
      }
    }
}
