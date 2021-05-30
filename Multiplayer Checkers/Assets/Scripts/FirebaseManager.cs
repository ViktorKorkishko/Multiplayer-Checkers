using System;
using System.Collections;
using Firebase;
using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    [Header("Firebase")] 
    public FirebaseAuth auth;
    public FirebaseUser user;

    [Space(5f)] 
    [Header("Login")] 
    [SerializeField] private TMP_InputField loginEmail;
    [SerializeField] private TMP_InputField loginPassword;
    [SerializeField] private TMP_Text loginOutputText;

    [Header("Register Reference")] 
    [SerializeField] private TMP_InputField registerUsername;
    [SerializeField] private TMP_InputField registerEmail;
    [SerializeField] private TMP_InputField registerPassword;
    [SerializeField] private TMP_InputField registerConfirmPassword;
    [SerializeField] private TMP_Text registerOutputText;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(Instance.gameObject);
            Instance = this;
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(checkDependancyTask =>
        {
            var dependencyStatus = checkDependancyTask.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies");
            }
        });
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Sighed out");
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log($"Signed In: {user.DisplayName}");
            }
        }
    }

    public void ClearOutputs()
    {
        loginOutputText.text = "";
        registerOutputText.text = "";
    }

    public void LoginButton()
    {
        StartCoroutine(LoginLogic(loginEmail.text, loginPassword.text));
    }

    public void RegisterButton()
    {
        StartCoroutine(RegisterLogic(registerUsername.text, registerEmail.text, registerPassword.text, registerConfirmPassword.text));
    }

    private IEnumerator LoginLogic(string email, string password)
    {
        Credential credential = EmailAuthProvider.GetCredential(email, password);

        var loginTask = auth.SignInWithCredentialAsync(credential);
        
        yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            FirebaseException firebaseException = (FirebaseException) loginTask.Exception.GetBaseException();
            AuthError error = (AuthError) firebaseException.ErrorCode;
            string output = "Unknown Error, Please Try Again!";
            
            switch (error)
            {
                case AuthError.MissingEmail:
                    output = "Please Enter Your Email!";
                    break;
                case AuthError.MissingPassword:
                    output = "Please Enter Your Password!";
                    break;
                case AuthError.InvalidEmail:
                    output = "Invalid Email!";
                    break;
                case AuthError.WrongPassword:
                    output = "Incorrect password!";
                    break;
                case AuthError.UserNotFound:
                    output = "Account Does Not Exist!";
                    break;
            }

            loginOutputText.text = output;
        }
        else
        {
            if (user.IsEmailVerified)
            {
                yield return new WaitForSeconds(1f);
                Debug.Log("LOGGED IN!\nSWITCH THE SCENE!");
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                //CHANGE SCENE TO MAIN MENU
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    private IEnumerator RegisterLogic(string username, string email, string password, string confirmPassord)
    {
        if (username == "")
        {
            registerOutputText.text = "Please Enter A Username";
        }
        else if(password != confirmPassord)
        {
            registerOutputText.text = "Passwords Do Not Match!";
        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

            yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException) registerTask.Exception.GetBaseException();
                AuthError error = (AuthError) firebaseException.ErrorCode;
                string output = "Unknown Error, Please Try Again!";
            
                switch (error)
                {
                    case AuthError.InvalidEmail:
                        output = "Invalid Email!";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        output = "Email Already In Use!";
                        break;
                    case AuthError.WeakPassword:
                        output = "Weak Password!";
                        break;
                    case AuthError.MissingEmail:
                        output = "Please Enter Your Email!";
                        break;
                    case AuthError.MissingPassword:
                        output = "Please Enter Your Password!";
                        break;
                }

                registerOutputText.text = output;
            }
            else
            {
                UserProfile profile = new UserProfile
                {
                    DisplayName = username,
                    //TODO : Give profile Default Photo
                };
                
                var defaultUserTask = user.UpdateUserProfileAsync(profile);
                yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

                if (registerTask.Exception != null)
                {
                    FirebaseException firebaseException = (FirebaseException) registerTask.Exception.GetBaseException();
                    AuthError error = (AuthError) firebaseException.ErrorCode;
                    string output = "Unknown Error, Please Try Again!";
            
                    switch (error)
                    {
                        case AuthError.Cancelled:
                            output = "Update User Cancelled!";
                            break;
                        case AuthError.SessionExpired:
                            output = "Session Expired!";
                            break;
                    }

                    registerOutputText.text = output;
                }
                else
                {
                    Debug.Log($"Firebase User Created Seccsessfully: {user.DisplayName} ({user.UserId})");
                    
                    //TODO : Send Email Verification
                }
            }
        }
    }
}