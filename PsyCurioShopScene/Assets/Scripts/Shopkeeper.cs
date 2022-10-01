using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Responsible for Shopkeeper Animations, speechbubble text & activation.
/// </summary>
public class Shopkeeper : MonoBehaviour, IPointerClickHandler {
    [SerializeField] private GameObject speechbubble;
    [SerializeField] private TMPro.TMP_Text speechbubbleText;

    private Animator _animator;
    private AnimationClip _wave, _idle;

    // Animation control
    private bool _isWaving;

    private void Awake() {
        gameObject.tag = Tags.Shopkeeper;
        _animator = GetComponent<Animator>();
    }

    public void OnPointerClick (PointerEventData eventData) {
        Wave();
    }
    
    /// <summary>
    /// Activate the Speechbubble and display given string on it.
    /// </summary>
    /// <param name="text"></param>
    public void Say(string text) {
        speechbubble.SetActive(true);
        speechbubbleText.text = text;
    }

    /// <summary>
    /// Play waving animation, if not waving.
    /// </summary>
    private void Wave() {
        // Only start waving, when not waving at the moment
        if (_isWaving) return;
        _animator.CrossFadeInFixedTime("Waving", 0.3f);
        _isWaving = true;
    }

    /// <summary>
    /// Transition back to idle animation. Is triggered by the event trigger on the Waving animation.
    /// </summary>
    [UsedImplicitly]
    private void TransitionToIdle() {
        _isWaving = false;
        _animator.CrossFadeInFixedTime("Neutral Idle", 0.8f);
    }
}
