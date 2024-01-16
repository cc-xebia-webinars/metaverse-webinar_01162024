using UnityEngine; 
using UnityEngine.SceneManagement; 
using TMPro; 

public class WorldItemBehaviour : MonoBehaviour 
{ 
    //In this variable we will connect the GameObject Name for later access to its properties. 
    public TextMeshProUGUI worldNameGameObject; 

    //In this variable we will store the name of the scene where we want to travel. 
    public string sceneName; 

    //In this variable we will store the public name of the world 
    public string worldName; 

    //When instantiating the GameObject, the script will assign the value of the variable worldName to the GameObject Name 
    void Start() 
    { 
        worldNameGameObject.text = worldName; 
    } 

    //Pressing the Join button will call this function and change the scene. 
    public void JoinWorld() 
    { 
        SceneManager.LoadScene(sceneName); 
    } 
} 