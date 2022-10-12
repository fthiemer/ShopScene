using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Responsible for Shopkeeper Animations, speechbubble text & activation.
/// </summary>
public class Shopkeeper : MonoBehaviour, IPointerClickHandler, IShopkeeper {
    public float ToWaveTransitionDuration => toWaveTransitionDuration;
    public float ToIdleTransitionDuration => toIdleTransitionDuration;
    
    [SerializeField] private float toWaveTransitionDuration = 0.3f;
    [SerializeField] private float toIdleTransitionDuration = 0.8f;

    private TMP_Text speechbubbleText;

    // Animation control
    private Animator animator;
    private bool isWaving;
    public bool IsWaving => isWaving;
    private int baseLayerIndex;

    private void Awake() {
        gameObject.tag = Tags.Shopkeeper;
        animator = GetComponent<Animator>();
        baseLayerIndex = animator.GetLayerIndex("Base Layer");
    }

    private void Start() {
        var speechbubble = GameObject.FindWithTag(Tags.SpeechbubbleTMP);
        speechbubbleText = speechbubble.GetComponent<TMP_Text>();
    }

    public void OnPointerClick (PointerEventData eventData) {
        Wave();
    }

    /// <summary>
    /// Activate the Speechbubble and display given string on it.
    /// </summary>
    /// <param name="text"></param>
    public void Say(string text) {
        speechbubbleText.text = text;
    }

    /// <summary>
    /// Play waving animation, if not waving.
    /// </summary>
    private void Wave() {
        // Only start waving, when not waving at the moment 
        if (isWaving || animator.IsInTransition(baseLayerIndex)) return;
        animator.CrossFadeInFixedTime("Waving", ToWaveTransitionDuration);
        isWaving = true;
    }

    /// <summary>
    /// Transition back to idle animation. Is triggered by the event trigger on the Waving animation.
    /// </summary>
    [UsedImplicitly]
    private void TransitionToIdle() {
        animator.CrossFadeInFixedTime("Idle", toIdleTransitionDuration);
        isWaving = false;
    }
}
