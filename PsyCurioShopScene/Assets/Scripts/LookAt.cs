using UnityEngine;

/// <summary>
/// Makes object face away from the camera.
/// For the speechbubble in the scene therefore a pivot object and
/// a y rotation of the bubble itself by 180° were added.
/// </summary>
public class LookAt : MonoBehaviour {
    [SerializeField] private Transform objectToLookAt;

    private void OnEnable() {
        gameObject.transform.LookAt(objectToLookAt);
    }
}
