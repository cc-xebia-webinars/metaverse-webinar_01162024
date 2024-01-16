using UnityEngine; 

public class InstantiatePlayer : MonoBehaviour 
{ 
    public GameObject playerPrefab; 

    // Start is called before the first frame update 
    void Start() 
    { 
      // creates a new player instance in the scene
      // the player is placed in the location of the spawn point using the transform property
        Instantiate(playerPrefab, transform); 
    } 
} 