using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 3f;
    public bool isStunned = false;
    private Rigidbody rb;
    private GameObject player;

    void Awake()
    {
        player = GameObject.Find("Player");
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.y < -10)
        {
            Destroy(gameObject);
        }

        if (!isStunned)
        {
            var dir = player.transform.position - transform.position;
            dir = dir.normalized;
            rb.AddForce(dir * speed);

        }
        else 
        {
            rb.linearVelocity = Vector3.zero;
        }
    }
}
