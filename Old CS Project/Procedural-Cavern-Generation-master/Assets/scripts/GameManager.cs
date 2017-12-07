using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;
    public BoardCreator boardCreator;
    public GameObject player;


    //private PlayerController playerController;

	// Use this for initialization
	void Start () {

        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

       
        boardCreator = GetComponent<BoardCreator>();
        boardCreator.GenerateMap();
        //playerController = GetComponent<PlayerController>();
	}
	
	//// Update is called once per frame
	//void Update () {
 //       if (playerController.dead)
 //           Debug.Log("Dead");
	//}
}
