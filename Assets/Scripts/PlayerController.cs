using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;

    public GameObject powerUpIndicator;

    private Rigidbody rb;

    private InputAction moveAction;
    private InputAction smashAction;
    private InputAction breakAction;

    private GameObject focalPoint;

    private bool hasPowerup = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        moveAction = InputSystem.actions.FindAction("Move");
        smashAction = InputSystem.actions.FindAction("Smash");
        breakAction = InputSystem.actions.FindAction("Break");

        focalPoint = GameObject.Find("FocalPoint");
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        // rb.AddForce(moveInput.y * speed * moveInput);
        rb.AddForce(moveInput.y * speed * focalPoint.transform.forward);

        if (breakAction.IsPressed())
        {
            rb.linearVelocity = Vector3.zero;
        }

        if (hasPowerup)
        {
            powerUpIndicator.transform.position = transform.position + new Vector3(0, 0.5f, 0);
            powerUpIndicator.transform.rotation = Quaternion.identity; // keep the indicator upright
        }

        if (smashAction.triggered && hasPowerup)
        {
            StartCoroutine(SmashActivate());
        }
    }

    // 1. รวมเงื่อนไขใน OnTriggerEnter อันเดิม
    private void OnTriggerEnter(Collider other)
    {
        // เงื่อนไขเดิม (PowerUp ปกติ)
        if (other.CompareTag("PowerUp"))
        {
            hasPowerup = true;
            Destroy(other.gameObject);
            Coroutine coroutine = StartCoroutine(PowerUpCooldown());
        }

        // --- เพิ่มเงื่อนไขใหม่ (PowerUpStun) เข้าไปตรงนี้ ---
        if (other.CompareTag("PowerUpStun"))
        {
            Destroy(other.gameObject);
            StartCoroutine(StunRoutine());
        }
    }

    IEnumerator PowerUpCooldown()
    {
        powerUpIndicator.SetActive(true);
        yield return new WaitForSeconds(10); // รอ 10 วินาทีตามปกติ
        hasPowerup = false;
        powerUpIndicator.SetActive(false);
        Debug.Log("Powerup has ended");
    }

    // 2. วาง IEnumerator ใหม่ ต่อท้ายฟังก์ชันอื่นๆ
    IEnumerator StunRoutine()
    {
        // ค้นหา Enemy ทั้งหมดในฉาก
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        // สั่งให้ศัตรูทุกตัวหยุด
        foreach (Enemy e in enemies)
        {
            e.isStunned = true;
        }

        // รอ 5 วินาที
        yield return new WaitForSeconds(5);

        // สั่งให้ศัตรูกลับมาเดินต่อ
        foreach (Enemy e in enemies)
        {
            if (e != null) e.isStunned = false;
        }
    }
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy") && hasPowerup)
        {
            var enemyRb = other.gameObject.GetComponent<Rigidbody>();
            Vector3 awayFromPlayer = other.transform.position - transform.position;
            enemyRb.AddForce(awayFromPlayer * 5f, ForceMode.Impulse);
            Debug.Log($"Player collided with {other.gameObject.name} and has powerup");
        }
    }

    IEnumerator SmashActivate()
    {
        // SEQ [1]
        float chargingTime = 0f;
        // player need to hold the smash button for 2 seconds for charing
        // then release the button to smash
        while (smashAction.IsPressed())
        {
            chargingTime += Time.deltaTime;
            Debug.Log($"smash charging time: {chargingTime}");
            if (chargingTime >= 2f)
            {
                Debug.Log("Smash activated");
                break;
            }
            yield return null;
        }

        // SEQ [2]
        if (chargingTime < 2f)
        {
            Debug.Log("Smash cancelled");
            yield break;
        }

        // SEQ [3]
        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            var enemyRb = enemy.GetComponent<Rigidbody>();
            enemyRb.AddExplosionForce(10f, transform.position, 20f, 0f, ForceMode.Impulse);
        }
    }
}
