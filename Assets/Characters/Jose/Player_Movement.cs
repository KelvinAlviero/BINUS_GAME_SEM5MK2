using UnityEngine;


public class Player_Movement : MonoBehaviour
{

    public float speed;

    public Rigidbody2D body; //How it works, {accesibility, component, inputted name}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()   {
        float xInput = Input.GetAxis("Horizontal"); //Gets X axis input
        float yInput = Input.GetAxis("Vertical"); //Gets Y axis input
        
        // if (Mathf.Abs(xInput) > 0) {
        //     body.linearVelocity = new Vector2(xInput*speed, body.linearvelocity.y);
        // }

        // if (Mathf.Abs(xInput) > 0) {
        //     body.linearVelocity = new Vector2(body.linearvelocity.x, yInput*speed);//NOTE, IF VELOCITY DOESN'T WORK, USE "linearVelocity", new API lolololol
        // }
        Vector2 direction = new Vector2(xInput, yInput).normalized; //Normalized is to set X and Y speed the same
        body.linearVelocity = direction * speed; 


    }
}
