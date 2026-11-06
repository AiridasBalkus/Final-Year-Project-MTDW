using UnityEngine;

public class MoveUpDown : MonoBehaviour
{
    public float distance = 2f;
    public float speed = 1f;
    private Vector3 startPos;
    void Start()
    {
        startPos = transform.position;
    }
    void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * distance;
        transform.position = startPos + Vector3.up * offset;
    }
}