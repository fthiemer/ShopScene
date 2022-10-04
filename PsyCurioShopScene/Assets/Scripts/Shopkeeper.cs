using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Responsible for Shopkeeper Animations, speechbubble text & activation.
/// </summary>
public class Shopkeeper : MonoBehaviour, IPointerClickHandler {
    [SerializeField] private GameObject speechbubble;
    [SerializeField] private TMPro.TMP_Text speechbubbleText;
    [SerializeField] private float toWaveTransitionDuration = 0.3f;
    [SerializeField] private float toIdleTransitionDuration = 0.8f;

    public float ToWaveTransitionDuration => toWaveTransitionDuration;
    public float ToIdleTransitionDuration => toIdleTransitionDuration;

    private Animator animator;
    private AnimationClip wave, idle;

    // Animation control
    private bool isWaving;
    public bool IsWaving => isWaving;
    private int baseLayerIndex;

    private void Awake() {
        gameObject.tag = Tags.Shopkeeper;
        animator = GetComponent<Animator>();
        baseLayerIndex = animator.GetLayerIndex("Base Layer");
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
