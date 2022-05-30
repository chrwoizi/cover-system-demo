using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float Speed = 0.1f;
    public float Distance = 10;
    public float Angle = 0;

    void Update()
    {
        Angle += Speed * Time.deltaTime;
        transform.position = new Vector3(Mathf.Sin(Angle) * Distance, 0, Mathf.Cos(Angle) * Distance);
        transform.rotation = Quaternion.LookRotation(Vector3.Normalize(-transform.position), Vector3.up);
    }
}
