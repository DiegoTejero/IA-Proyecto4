using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour {

    private GameController gc;
    public int score, highscore;

    private Text scoreText, highScoreText;

    private void Awake()
    {
        if (FindObjectsOfType(GetType()).Length > 1)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

        // Use this for initialization
    void Start ()
    {
        score = 0;
        gc = GameObject.Find("GameController").GetComponent<GameController>();

        scoreText = GameObject.Find("ScoreCount").GetComponent<Text>();
        highScoreText = GameObject.Find("HighScore").GetComponent<Text>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (scoreText == null || highScoreText == null || gc == null)
        {
            Start();
        }
        //almacena el score en base a la velocidad
        if(gc.player.alive == true)
            score = (int)Mathf.Round( gc.speedScore * 1000);

        //actualiza el highscore
        if (score >= highscore)
        {
            highscore = score;
        }
        //convierte a texto el score
        if (score.ToString().Length > 4)
        {

            scoreText.text = score.ToString();
            highScoreText.text = highscore.ToString();
        }
        //pinta los ceros a la izquierda del score
        else
        {
            int nzero = (5 - score.ToString().Length);
            int nhzero = (5 - highscore.ToString().Length);

            string zeros = new string('0', nzero);
            string nhzeros = new string('0', nhzero);

            scoreText.text = zeros + score.ToString();
            highScoreText.text = nhzeros + highscore.ToString();
        }
	}
}
