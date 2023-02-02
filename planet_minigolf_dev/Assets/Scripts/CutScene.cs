using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutScene : MonoBehaviour
{
    public GameObject player;
    public GameObject mate;

    float speed = 3.0f; //how fast it shakes
    float amount = 0.1f; //how much it shakes
    

    // Update is called once per frame
    void Update()
    {
        player.transform.position = new Vector3(- Mathf.Sin(Time.time * speed) * 0.7f * amount  + 4, 0, 0);
        mate.transform.position = new Vector3(Mathf.Sin(Time.time * speed) * amount - 4, 0, 0);
    }
}
