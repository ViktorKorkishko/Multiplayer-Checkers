using TMPro;
using UnityEngine;

public class AuthUIManager : MonoBehaviour
{
    public static AuthUIManager Instance;

    [Header("References")] 
    [SerializeField] private GameObject checkingForAccount;
    [SerializeField] private GameObject loginUI;
    [SerializeField] private GameObject registerUI;
    [SerializeField] private GameObject verifyEmailUI;
    [SerializeField] private TMP_Text verifyEmailText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if(Instance != null)
        {
            Destroy(gameObject);
        }
    }

    private void ClearUI()
    {
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        FirebaseManager.Instance.ClearOutputs();
    }

    public void OpenLoginScreen()
    {
        ClearUI();
        loginUI.SetActive(true);
    }

    public void OpenRegisterScreen()
    {
        ClearUI();
        registerUI.SetActive(true);
    }
}