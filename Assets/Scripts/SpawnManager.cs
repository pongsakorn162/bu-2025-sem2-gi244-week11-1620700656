using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Wave
{
    public int totalSpawnEnemies;       // จำนวนศัตรูใน wave
    public int numberOfRandomSpawnPoint; // จำนวนจุดเกิดที่จะสุ่มเลือก
    public float delayStart;            // เวลาก่อนเริ่ม wave
    public float spawnInterval;         // เวลาห่างระหว่างการเกิดแต่ละตัว
    public int numberOfPowerUp;         // จำนวน PowerUp ตอนเริ่ม wave
}

public class SpawnManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject[] powerupPrefabs; // ใส่ Prefab ของ PowerUp ทั้งหมดที่มี
    public Transform[] spawnPoints;     // ลาก SpawnPoint01-06 มาใส่ในนี้
    public Wave[] waves;                // ตั้งค่า Wave ทั้ง 4 ใน Inspector

    void Start()
    {
        // เปลี่ยนจาก InvokeRepeating เป็น Coroutine
        StartCoroutine(SpawnWaveRoutine());
    }

    IEnumerator SpawnWaveRoutine()
    {
        foreach (Wave currentWave in waves)
        {
            // --- ขั้นตอนที่ 1: เกิด PowerUp ทันทีเมื่อเริ่ม Wave ---
            for (int i = 0; i < currentWave.numberOfPowerUp; i++)
            {
                Instantiate(powerupPrefabs[Random.Range(0, powerupPrefabs.Length)],
                            GetRandomPowerUpPosition(), Quaternion.identity);
            }

            // --- ขั้นตอนที่ 2: สุ่มเลือกชุดจุดเกิดสำหรับ Wave นี้ ---
            List<Transform> selectedPoints = GetRandomPoints(currentWave.numberOfRandomSpawnPoint);

            // --- ขั้นตอนที่ 3: รอตามเวลา delayStart ---
            yield return new WaitForSeconds(currentWave.delayStart);

            // --- ขั้นตอนที่ 4: ปล่อยศัตรูจนครบจำนวน ---
            for (int i = 0; i < currentWave.totalSpawnEnemies; i++)
            {
                // สุ่มเลือก 1 จุดจาก "ชุดจุดเกิดที่เลือกไว้แล้ว"
                Transform randomPoint = selectedPoints[Random.Range(0, selectedPoints.Count)];
                Instantiate(enemyPrefab, randomPoint.position, Quaternion.identity);

                // รอระหว่างแต่ละตัว
                yield return new WaitForSeconds(currentWave.spawnInterval);
            }

            // --- ขั้นตอนที่ 5: รอให้ศัตรูตายหมดก่อนเริ่ม Wave ถัดไป ---
            while (GameObject.FindGameObjectsWithTag("Enemy").Length > 0)
            {
                yield return new WaitForSeconds(1.0f);
            }

            Debug.Log("Wave Finished!");
        }
    }

    // ฟังก์ชันสุ่มเลือกจุดเกิดแบบไม่ซ้ำ
    List<Transform> GetRandomPoints(int count)
    {
        List<Transform> tempList = new List<Transform>(spawnPoints);
        List<Transform> selected = new List<Transform>();
        for (int i = 0; i < count && tempList.Count > 0; i++)
        {
            int index = Random.Range(0, tempList.Count);
            selected.Add(tempList[index]);
            tempList.RemoveAt(index);
        }
        return selected;
    }

    // ฟังก์ชันสุ่มตำแหน่งวาง PowerUp (ปรับค่า range ตามขนาดเกาะ)
    Vector3 GetRandomPowerUpPosition()
    {
        return new Vector3(Random.Range(-9, 9), 0, Random.Range(-9, 9));
    }
}
