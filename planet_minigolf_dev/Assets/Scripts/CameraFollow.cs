using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public Transform player;
    public Transform black_hole;

    private Camera cam;

    void Start() {
        cam = gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update () {
        Vector3 black_hole_to_player_vector = player.transform.position - black_hole.transform.position;
        Vector3 new_camera_pos_xy = black_hole.transform.position + black_hole_to_player_vector * 0.5f;
        transform.position = player.transform.position + new Vector3(0, 0, -5);
        Debug.Log(cam.fieldOfView);
    }
}