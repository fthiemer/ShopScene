using System;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class BuyableObject : MonoBehaviour {


    /// <summary>
    /// The distance between center and lower edge. Used for correct placement on the counter.
    /// </summary>
    public float YOffset { get; private set; }

    [SerializeField] private string name;
    public string Name { get { return name; } }
    
    [SerializeField] private float price;
    public float Price { get { return price; } }
    
    [SerializeField] private Counter counter;


    private void Awake() {
        var tmpMeshFilter = gameObject.GetComponent<MeshRenderer>();
        YOffset = tmpMeshFilter.bounds.extents.y;
    }

    private void OnMouseDown() {
        counter.PlaceOnCounter(gameObject);
        Debug.Log($"Bought {Name} for {Price}");
    }
}
