using UnityEngine; 

public class IdentificationUIManager : MonoBehaviour 
{ 
    public GameObject MenuButtonsPanel; 
    public GameObject SignUpPanel; 
    public GameObject SignInPanel; 
    public GameObject BackButton; 

    void Start() 
    { 
        //Initial setup 
        ShowInitialScreen(); 
    } 

    //We deactivate all Panels except the initial buttons. 
    public void ShowInitialScreen() 
    { 
        MenuButtonsPanel.SetActive(true); 
        SignUpPanel.SetActive(false); 
        SignInPanel.SetActive(false);
        BackButton.SetActive(false);
    } 

    //We deactivate all Panels except for the registration Panel. 
    public void ShowSignUpPanel() 
    { 
        MenuButtonsPanel.SetActive(false); 
        SignUpPanel.SetActive(true); 
        SignInPanel.SetActive(false); 
        BackButton.SetActive(true);
    } 

    //We deactivate all Panels except for the login panel. 
    public void ShowSignInPanel() 
    { 
        MenuButtonsPanel.SetActive(false); 
        SignUpPanel.SetActive(false); 
        SignInPanel.SetActive(true); 
        BackButton.SetActive(true);
    } 
} 