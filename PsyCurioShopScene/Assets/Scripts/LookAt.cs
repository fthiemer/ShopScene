using UnityEngine;

public class LookAt : MonoBehaviour {
    [SerializeField] private Transform objectToLookAt;

    private void OnEnable() {
        gameObject.transform.LookAt(objectToLookAt);
    }
}
