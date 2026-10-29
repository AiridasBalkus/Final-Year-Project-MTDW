using UnityEngine;


public class MoveLeftRight : MonoBehaviour
{
    public float distance = 3f;
    public float speed = 1f;
    private Vector3 startPos;
    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * speed) * distance;
        transform.position = startPos + transform.right * offset;
    }
}