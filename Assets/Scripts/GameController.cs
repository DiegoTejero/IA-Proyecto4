using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public Player player;
    public GameObject[] prefabObstacleList;
    public List<GameObject> ObstacleList;

    public GameObject cloudPrefab;
    public float speed;
    public float speedScore;
    private bool creatingObstacle;

    private int generations;

    public Transform floor;
    public Transform floor2;

    public GameObject gameOver;
    public Button retry;

    private float MAX_SPEED = 0.3f;


    void Start () {
        player = GameObject.Find("Dino").GetComponent<Player>();
        speedScore = speed;
        creatingObstacle = false;
        generations = 1;
    }

    void Update()
    {
       

        if (creatingObstacle == false)
        {

            creatingObstacle = true;
            StartCoroutine(CooldownSpawn());
            StartCoroutine(CooldownClouds());
        }

        FloorMove();

        if (player.alive == false)
        {
            ClearConsole();
            Debug.Log("Generation : " + generations);
            Restart();
        }
    }

    public void CreateObstacle ()
    {

        int rand = Random.Range(0, prefabObstacleList.Length);
        

        GameObject obstacle = (GameObject)Instantiate(prefabObstacleList[rand].gameObject, GameObject.Find("Spawn1").transform.localPosition, Quaternion.identity);


        if (!ObstacleList.Contains(obstacle))
        {
        ObstacleList.Add(obstacle);
        }        

        if (obstacle.name.Contains("Polluelo"))
        {
            int rand2 = Random.Range(0, 3);

            if (rand2 == 0)
            {
                obstacle.transform.localPosition = GameObject.Find("Spawn2").transform.localPosition;
            }
            else if (rand2 == 1)
            {
                obstacle.transform.localPosition = GameObject.Find("Spawn3").transform.localPosition;
            }

        }

    }

    public void CreateCloud()
    {
        float randY = Random.Range(2,5.5f);
        int randFlip = Random.Range(0, 2);
        float randSpeed = Random.Range(0, 0.1f);

        GameObject cloud = (GameObject)Instantiate(cloudPrefab.gameObject, new Vector3(30,randY,0), Quaternion.identity);

        cloud.GetComponent<Obstacle>().personalSpeed -= randSpeed;

        if (randFlip == 0)
        {
            cloud.GetComponent<SpriteRenderer>().flipX = true;
        }

    }

    public void RemoveObstacle(GameObject obstacle)
    {
        if (ObstacleList.Contains(obstacle))
        {
            ObstacleList.Remove(obstacle);
        }
    }

    public float Parallax ()
    {
        if (player.alive == true)
        {
            speed = Mathf.Clamp((speed + 0.0001f), 0, MAX_SPEED);
            speedScore = speedScore + 0.0001f;
            return speed;
        }
        else
        {
            speed = 0;
            /*retry.gameObject.SetActive(true);
            gameOver.SetActive(true);*/

            return speed;
        }
    }

    public void FloorMove ()
    {
        floor = GameObject.Find("Floor").transform;
        floor2 = GameObject.Find("Floor1").transform;

        if (floor.localPosition.x <= -15)
        {
            floor.position = new Vector3(floor2.position.x + 24, floor.position.y, floor.position.z);
        }
        if (floor2.localPosition.x <= -15)
        {
            floor2.position = new Vector3(floor.position.x + 24, floor2.position.y, floor2.position.z);
        }
    }
    
    // Update is called once per frame

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Restart()
    {
        for (int i = 0; i < ObstacleList.Count; i++)
        {
          Destroy(ObstacleList[i]);
        }

        /*GameObject.Find("Floor").transform.SetPositionAndRotation(initialFloor.position, initialFloor.rotation);
        GameObject.Find("Floor1").transform.SetPositionAndRotation(initialFloor2.position, initialFloor2.rotation);*/

        ObstacleList.Clear();
        speedScore = 0;

        speed = 0;

        

        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY;
        player.GetComponent<Rigidbody2D>().velocity = new Vector2(0,0);
        player.transform.position = player.initialPos;
        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;

        player.alive = true;

        generations++;

    }
    
    public IEnumerator CooldownSpawn()
    {
        float wait;

        if (Parallax() >= 0.25f)
        {
            float rand = Random.Range(-2.0f, 1.0f);
            wait = 2.3f + rand;
        }
        else
            wait = 3f;
        
        yield return new WaitForSeconds(wait);

        CreateObstacle();
        creatingObstacle = false;
    }

    public IEnumerator CooldownClouds()
    {
        float wait;

        float rand = Random.Range(-5.0f, +5.0f);
        wait = 10 + rand;
       
        yield return new WaitForSeconds(wait);

        CreateCloud();
        
    }

    static void ClearConsole()
    {
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");

        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

        clearMethod.Invoke(null, null);
    }

}
