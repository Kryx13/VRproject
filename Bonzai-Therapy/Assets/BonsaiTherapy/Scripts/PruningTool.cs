using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections.Generic;

/// <summary>
/// Pruning tool (scissors) for cutting bonsai leaves.
/// Detects leaves in the trigger zone and cuts them when the player presses the trigger.
/// </summary>
[RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable))]
public class PruningTool : MonoBehaviour
{
    [Header("Haptic Feedback")]
    [Range(0f, 1f)]
    public float vibrationIntensity = 0.4f;
    [Range(0.01f, 0.2f)]
    public float vibrationDuration = 0.05f;

    [Header("Audio")]
    public AudioClip snipSound;
    [Range(0f, 1f)]
    public float snipVolume = 0.5f;

    [Header("Visual Feedback")]
    public bool animateOnCut = true;
    public float cutAnimationSpeed = 10f;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    private AudioSource audioSource;
    private List<GameObject> leavesInRange = new List<GameObject>();

    // For scissor animation
    private Transform bladePivot;
    private bool isClosing = false;
    private float bladeAngle = 0f;

    void Awake()
    {
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Setup audio
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound

        // Generate a snip sound if none is assigned
        if (snipSound == null)
        {
            snipSound = GenerateSnipSound();
        }

        // Subscribe to activation event (trigger press)
        grabInteractable.activated.AddListener(OnTriggerPressed);
        grabInteractable.deactivated.AddListener(OnTriggerReleased);
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(OnTriggerPressed);
            grabInteractable.deactivated.RemoveListener(OnTriggerReleased);
        }
    }

    void Update()
    {
        // Animate scissor blades
        if (animateOnCut && bladePivot != null)
        {
            float targetAngle = isClosing ? 15f : 0f;
            bladeAngle = Mathf.Lerp(bladeAngle, targetAngle, Time.deltaTime * cutAnimationSpeed);
            bladePivot.localRotation = Quaternion.Euler(0, 0, bladeAngle);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Leaf"))
        {
            if (!leavesInRange.Contains(other.gameObject))
            {
                leavesInRange.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Leaf"))
        {
            leavesInRange.Remove(other.gameObject);
        }
    }

    private void OnTriggerPressed(ActivateEventArgs args)
    {
        isClosing = true;
        CutLeaves();
    }

    private void OnTriggerReleased(DeactivateEventArgs args)
    {
        isClosing = false;
    }

    private void CutLeaves()
    {
        // Clean up null references
        leavesInRange.RemoveAll(leaf => leaf == null || !leaf.activeSelf);

        if (leavesInRange.Count > 0)
        {
            // Cut the first leaf in range
            GameObject leafToCut = leavesInRange[0];
            leavesInRange.RemoveAt(0);

            // Disable the leaf (cut it)
            leafToCut.SetActive(false);

            // Play sound
            PlaySnipSound();

            // Haptic feedback
            TriggerHaptics();

            Debug.Log("Leaf cut!");
        }
    }

    private void PlaySnipSound()
    {
        if (snipSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(snipSound, snipVolume);
        }
    }

    private void TriggerHaptics()
    {
        if (grabInteractable.interactorsSelecting.Count > 0)
        {
            var interactor = grabInteractable.interactorsSelecting[0];

            if (interactor is XRBaseInputInteractor inputInteractor)
            {
                inputInteractor.SendHapticImpulse(vibrationIntensity, vibrationDuration);
            }
        }
    }

    private AudioClip GenerateSnipSound()
    {
        int sampleRate = 44100;
        float duration = 0.15f;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = Mathf.Exp(-t * 40f); // Sharp decay
            float noise = (Random.value * 2f - 1f) * 0.6f;
            float click = Mathf.Sin(2f * Mathf.PI * 2500f * t) * 0.4f;
            samples[i] = (noise + click) * envelope;
        }

        AudioClip clip = AudioClip.Create("SnipSound", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    /// <summary>
    /// Set the blade pivot for animation (called by ScissorsGenerator)
    /// </summary>
    public void SetBladePivot(Transform pivot)
    {
        bladePivot = pivot;
    }

    /// <summary>
    /// Returns the number of leaves currently in cutting range
    /// </summary>
    public int GetLeavesInRangeCount()
    {
        leavesInRange.RemoveAll(leaf => leaf == null || !leaf.activeSelf);
        return leavesInRange.Count;
    }
}
