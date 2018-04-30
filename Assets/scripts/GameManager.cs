using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;
    public BoardCreator boardCreator;
    public GameObject player;
    public GameObject enemyPrefab;
    public int numberOfEnemies;
    public GameObject scoreTxt;

    //private PlayerController playerController;

    // Use this for initialization
    void Start () {

        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

       
        boardCreator = GetComponent<BoardCreator>();
        boardCreator.GenerateMap();
        player.transform.position = boardCreator.GetPlayerStartLocation();
        MakeEnemies();
	}

    void MakeEnemies()
    {
        Vector3 end = boardCreator.GetPlayerEndLocation();

        for(int i = 0; i < numberOfEnemies; i++)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, end, Quaternion.identity) as GameObject;
            newEnemy.name = "Enemy: " + i;
            newEnemy.GetComponent<EnemyController>().player = player;
        }
    }
	
	//// Update is called once per frame
	//void Update () {
 //       if (playerController.dead)
 //           Debug.Log("Dead");
	//}
}
