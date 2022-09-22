using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {

    public GameController gc;
    private float speed;
    public float personalSpeed;

	// Use this for initialization
	void Start ()
    {
        gc = GameObject.Find("GameController").GetComponent<GameController>();
	}

    private void Update()
    {
        if (gc.player.alive == false)
        {
            speed = 0;
            personalSpeed = 0;
            if (this.GetComponent<Animator>() != null)
            {
                this.GetComponent<Animator>().enabled = false;
            }
        }

        if (this.transform.localPosition.x <= -20)
        {
            gc.RemoveObstacle(this.gameObject);
            Destroy(this.gameObject);
        }
        speed = gc.Parallax();
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        this.transform.localPosition = new Vector2(this.transform.localPosition.x - speed + personalSpeed, this.transform.localPosition.y);	
	}
}
