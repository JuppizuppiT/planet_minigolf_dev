using UnityEngine;

public class CameraFollow : MonoBehaviour {

    public Transform player;
    public Transform black_hole;

    private Camera cam;
    private float min_size = 10f;
    private float max_size = 50f;

    void Start() {
        cam = gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update () {
        Vector3 player_to_goal_vector = black_hole.transform.position - player.transform.position;
        Vector3 new_camera_pos_xy = player.transform.position + player_to_goal_vector * 0.5f;
        float height = (player_to_goal_vector.magnitude / 2) / Mathf.Tan(30 * (Mathf.PI / 180));
        Debug.Log(new_camera_pos_xy);
        transform.position = new_camera_pos_xy + new Vector3(0, 0, -5);
        
        height = (height < min_size) ? min_size : height;
        
        cam.orthographicSize = height * 0.8f;
    }
}