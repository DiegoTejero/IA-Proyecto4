using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public Vector2 initialPos;

    public bool alive;
    private bool grounded;
    private bool isJumping = false;
    public Rigidbody2D physics2D;
    public float jumpForce;
    private float minJumpDistance = 6.25f;
    public GameObject nearObstacle = null;
    public float distanceToObstacle = 20000;
    public GameObject nearSecondObstacle = null;
    public float distanceToSecondObstacle = 20000;
    public float distanceBetweenObstacles = 20000;
    private float minDistanceForDoubleObstacleJump = 8;
    public float ObstacleHeight;
    private float MinDuckHeight = 0.87f;
    private bool goDown = false;
   
    public GameController gc;

    private Animator dinoAnim;

    public byte state = 0; //OUTPUTS: 0 = saltar, 1 = agacharse, 2 = bajar
    public RedNeuronalDino brain;
    private float[] inputs;


    private void Awake()
    {
        initialPos = new Vector2(0, 0.497f);
    }

    // Use this for initialization
    void Start() {
        dinoAnim = this.GetComponent<Animator>();

        jumpForce = 340;
        physics2D = this.GetComponent<Rigidbody2D>();
        grounded = false;
        alive = true;

        gc = GameObject.Find("GameController").GetComponent<GameController>();

        inputs = new float[5];
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.tag == "Floor") 
        {
            grounded = true;
        }

        else if (collision.gameObject.tag == "Obstacle")
        {
            
            alive = false;
            //dinoAnim.enabled = false;   
           
            
        }
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Floor")
        {

            grounded = true;
            dinoAnim.SetBool("Grounded", grounded);

        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Floor")
        {

            grounded = false;
            dinoAnim.SetBool("Grounded", grounded);
        }
    }

    // Update is called once per frame
    void Update ()
    {
        //PlayerControls();

        SetVariables();

        DecidirEstado();
        switch (state)
        {
            case 0:
                //saltar
                if (grounded && alive)
                {
                    grounded = false;
                    Jump();
                }                   
                break;
            case 1:
                //Agacharse
                if (grounded && alive)
                    Duck();
                else if (!goDown && !grounded)
                {

                    goDown = true;
                    physics2D.AddForce(Vector2.down * jumpForce * 2);
                }
                break;
            case 2:
                //Bajar cual sanic
                if (grounded)
                    UnDuck();
                else if (!goDown)
                {

                    goDown = true;
                    physics2D.AddForce(Vector2.down * jumpForce*2);
                }
                    
                break;
        }
        Debug.Log(state);
        CalcularIncrementoNodos();
        //LAFUNCIONDEDEBUGS();
    }

    private void Jump()
    {
        grounded = false;
        goDown = false;
        StartCoroutine(SingleJump());
        dinoAnim.SetBool("Grounded", grounded);
    }

    private void Duck()
    {
        if (dinoAnim.GetBool("Ducked") == false)
        {
            this.GetComponent<BoxCollider2D>().size = new Vector2(this.GetComponent<BoxCollider2D>().size.x, 0.5f);
            dinoAnim.SetBool("Ducked", true);
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y - 0.19f, this.transform.localPosition.z);
        }
    }

    private void UnDuck()
    {
        if (dinoAnim.GetBool("Ducked") == true)
        {
            this.GetComponent<BoxCollider2D>().size = new Vector2(this.GetComponent<BoxCollider2D>().size.x, 0.88f);
            dinoAnim.SetBool("Ducked", false);
            this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y + 0.19f, this.transform.localPosition.z);
        }
    }

    private void SetVariables()
    {
        

        distanceToObstacle = 100;
        distanceToSecondObstacle = 100;
        //de los raycast elegir el mas pequeño positivo y actualizar distancia con el
        for (int i = 0; i < gc.ObstacleList.Count; i++)
        {
            if (gc.ObstacleList[i].transform.position.x - transform.position.x <= distanceToObstacle 
                && gc.ObstacleList[i].transform.position.x - transform.position.x > 0)
            {
                nearObstacle = gc.ObstacleList[i];
                distanceToObstacle = nearObstacle.transform.position.x - transform.position.x;

                ObstacleHeight = nearObstacle.transform.position.y - nearObstacle.GetComponent<BoxCollider2D>().size.y / 2;
            }

            if (nearObstacle.transform.position != gc.ObstacleList[i].transform.position 
                && gc.ObstacleList[i].transform.position.x - transform.position.x <= distanceToSecondObstacle
                && gc.ObstacleList[i].transform.position.x - transform.position.x > 0)
            {
                nearSecondObstacle = gc.ObstacleList[i];
                distanceToSecondObstacle = nearSecondObstacle.transform.position.x - transform.position.x;

                distanceBetweenObstacles = nearSecondObstacle.transform.position.x - nearObstacle.transform.position.x;
            }
        }
    }

    private void CalcularIncrementoNodos()
    {
        if (distanceToObstacle < minJumpDistance)
        {
            // REFUERZO SU SALTO
            float[] output = { 0.9f, 0.1f, 0.1f };

            brain.RetrainNetwork(inputs, output);
        }
        if (ObstacleHeight >= MinDuckHeight)
        {
            // REFUERZO EL AGACHADO
            float[] output = { 0.1f, 0.9f, 0.1f };
            
            brain.RetrainNetwork(inputs, output);
        }
        if ((distanceBetweenObstacles > minDistanceForDoubleObstacleJump && !grounded) 
            || (distanceToObstacle > minJumpDistance && !grounded )
            || (transform.position.y > 1 && ObstacleHeight > 1 && !grounded) )
        {
            //REFUERZO BAJAR RAPIDO
            float[] output = { 0.1f, 0.1f, 0.6f };

            brain.RetrainNetwork(inputs, output);
        }

    }

    private void DecidirEstado()
    {
        // INPUTS: Distancia a obstaculo, altura del dino, altura del obstaculo, espacio entre obstaculos, velocidad
        inputs[0] = distanceToObstacle;//Esto es el raycast al obstaculo mas cercano
        inputs[1] = this.transform.position.y + this.GetComponent<BoxCollider2D>().size.y/2;// transform + 1/2 del box collider del dino
        inputs[2] = ObstacleHeight;// transform del obstaculo
        inputs[3] = distanceBetweenObstacles;// X de obstaculo del segundo más cercano - X de obstaculo más cercano 
        inputs[4] = gc.speed;// speed
        byte newState;

        newState = brain.CheckAction(inputs);

        if (state != newState)
        {
            state = newState;

        }
    }

    public IEnumerator SingleJump()
    {

        if (!isJumping)
        {
            isJumping = true;
            physics2D.AddForce(Vector2.up * jumpForce);
        }

        yield return new WaitForSeconds(0.2f);
        isJumping = false;

    }

    private void PlayerControls()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && grounded)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) && alive)
        {


            if (!grounded)
                physics2D.AddForce(Vector2.down * jumpForce);
            else
            {
                //grounded = true;
                if (dinoAnim.GetBool("Ducked") == false)
                {
                    this.GetComponent<BoxCollider2D>().size = new Vector2(this.GetComponent<BoxCollider2D>().size.x, 0.5f);
                    dinoAnim.SetBool("Ducked", true);
                    this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y - 0.19f, this.transform.localPosition.z);

                }
            }


        }

        else if (Input.GetKeyUp(KeyCode.DownArrow) && alive)
        {
            if (dinoAnim.GetBool("Ducked") == true)
            {
                this.GetComponent<BoxCollider2D>().size = new Vector2(this.GetComponent<BoxCollider2D>().size.x, 0.88f);

                dinoAnim.SetBool("Ducked", false);
                this.transform.localPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y + 0.19f, this.transform.localPosition.z);
            }
        }
    }

    public void LAFUNCIONDEDEBUGS()
    {
        Debug.Log("Mi distancia al obstaculo mas cercano es de : " + inputs[0]);
        Debug.Log("Mi altura maxima (DINO) es de : " + inputs[1]);
        Debug.Log("La altura del obstaculo mas cercano es de : " + inputs[2]);
        Debug.Log("El espacio entre los 2 obstaculos mas cercanos es de : " + inputs[3]);
        Debug.Log("La velocidad para el drifto es de  : " + inputs[4]);
    }

}
