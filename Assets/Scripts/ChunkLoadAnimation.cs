using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ChunkLoadAnimation : MonoBehaviour
{
    private float speed = 3f;
    private Vector3 targetPos;
    private float waitTimer;
    private float timer;
    private void Start()
    {
        waitTimer = Random.Range(0.0f, 3.0f);
        targetPos = transform.position;
        transform.position = new Vector3(transform.position.x, -VoxelData.chunkHeight, transform.position.z);
    }
    private void Update()
    {
        if (timer < waitTimer)
        {
            timer += Time.deltaTime;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
            if ((targetPos.y - transform.position.y) < 0.05f)
            {
                transform.position = targetPos;
                Destroy(this);
            }
        }
    }
}
