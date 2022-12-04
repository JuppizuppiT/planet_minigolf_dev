using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public Transform player;
    public Transform black_hole;

    private Camera cam;
    private float h_factor = 0.8f;

    void Start() {
        cam = gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update () {
        transform.position = player.transform.position + new Vector3(0, 0, -5);
    }
}