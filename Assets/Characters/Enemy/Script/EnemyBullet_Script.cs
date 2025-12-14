using UnityEngine;

public class EnemyBullet_Script : MonoBehaviour
{
    public float bulletDamage = 2f;
    private Player_Stats player_script;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collider = collision.gameObject;
        if (collider.CompareTag("Player") || collider.layer.Equals(7))
        {
            Debug.Log("Player got hit");
            player_script = collider.GetComponent<Player_Stats>();

            player_script.TakeDamage(bulletDamage);

            Explode();
        }
        if (collider.layer.Equals(6))
        {
            Explode();
            Debug.Log("Bullet hit ground");
            //Explode
        }
        if (collider.layer.Equals(8))
        {
            Explode();
            Debug.Log("Bullet hit wall");
            //Explode
        }
        else
        {
            return;
        }
    }

    private void Explode()
    {
        Invoke("Delay", 0.01f);
    }

    private void Delay()
    {
        Destroy(gameObject);
    }

}
