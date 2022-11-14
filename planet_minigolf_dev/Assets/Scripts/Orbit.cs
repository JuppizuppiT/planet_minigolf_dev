using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject planet;
    float speed = 15.0f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        OrbitAround();
    }

    void OrbitAround()
    {
        transform.RotateAround(planet.transform.position, Vector3.forward, speed * Time.deltaTime);
    }
}
