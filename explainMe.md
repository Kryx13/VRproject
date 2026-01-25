🌿 Bonsai Therapy VR: Project Documentation

1. Project Overview
Bonsai Master VR is a minimalist, meditative experience where the player shapes a digital bonsai tree. Unlike traditional games, there are no scores, timers, or "win" conditions. The goal is to find "flow" through the repetitive, precise act of pruning.
Why this is unique to VR:
 * Depth Perception: Players must physically move their heads to see between branches and identify which specific leaves to trim.
 * Haptic Tactility: Every cut triggers a localized vibration in the controller, simulating the tactile "snap" of a branch that a mouse click cannot replicate.
 * 1:1 Precision: The player uses their own hand steadiness to align the tool with 2cm-wide leaf nodes in 3D space.
2. Step-by-Step Development Guide
Phase 1: Environment & XR Setup
 * Project Start: Create a Unity project using the URP (Universal Render Pipeline) for best VR performance.
 * XR Integration: Install the XR Interaction Toolkit. Setup the XR Origin (Action-based) to enable headset and controller tracking.
 * The Workshop: Create a simple room with a Floor and a Table (Cube scaled to 1.2, 0.8, 0.8).
Phase 2: Building the Modular Bonsai
To ensure stability and artistic freedom, the tree is built using a Parent-Child hierarchy of primitive shapes.
 * The Trunk: A Cylinder at the center (Scale: 0.05, 0.2, 0.05).
 * The Branches: 4 Cylinders attached to the trunk. Each rotated on the Y-axis (45°, 135°, -135°, -45°) to create a 3D spiral.
 * The Leaf Nodes: * Create 10-15 small Spheres per branch (Scale: 0.02).
   * Set their Tag to Leaf.
   * Check the Is Trigger box on their Sphere Colliders.
Phase 3: The Pruning Tool (The "Scissors")
 * The Model: Create a Cylinder (or a scissors mesh) and add the XR Grab Interactable component.
 * The Trigger Zone: Add a small Box Collider (Is Trigger) at the tip of the tool. This is the "cutting edge."
 * The Logic: Attach the PruningTool.cs script (see Section 3). It listens for the Trigger press on the VR controller to disable the leaf currently inside the trigger zone.
Phase 4: Atmosphere & Polish
 * Lighting: Use a warm Directional Light and a soft Skybox to create a calm atmosphere.
 * Audio: Add a "snip" sound effect that plays whenever a leaf is disabled.
 * Haptics: Configure the script to send a short haptic pulse (0.4f intensity) to the controller upon cutting.
3. Core Interaction Script
This script manages the detection of individual leaves and the VR haptic feedback.
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;

public class PruningTool : MonoBehaviour
{
    public float vibrationIntensity = 0.4f;
    public float vibrationDuration = 0.05f;
    
    private XRGrabInteractable grabInteractable;
    private List<GameObject> leavesInRange = new List<GameObject>();

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        grabInteractable.activated.AddListener(OnTriggerPressed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Leaf")) leavesInRange.Add(other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Leaf")) leavesInRange.Remove(other.gameObject);
    }

    private void OnTriggerPressed(ActivateEventArgs args)
    {
        if (leavesInRange.Count > 0)
        {
            GameObject leafToCut = leavesInRange[0]; 
            leavesInRange.Remove(leafToCut);
            leafToCut.SetActive(false);
            TriggerHaptics();
        }
    }

    private void TriggerHaptics()
    {
        IXRInteractor interactor = grabInteractable.interactorsSelecting[0];
        if (interactor is XRBaseControllerInteractor controller)
        {
            controller.xrController.SendHapticImpulse(vibrationIntensity, vibrationDuration);
        }
    }
}

4. Stability & Performance Tips
 * Static Batching: Mark the Table and Trunk as "Static" in the Inspector to save rendering power.
 * Layer Collision Matrix: In Project Settings, make sure the "Leaf" layer only collides with the "Tool" layer to avoid unnecessary physics calculations.
 * Scale Calibration: Ensure the table height matches the player's real-world floor for a comfortable reach.
Final Project Status
 * [x] VR-Specific Interaction (Haptics + Spatial Depth)
 * [x] Optimized for Stability (Primitive colliders + SetActive logic)
 * [x] Meditative/Peaceful design focus
Would you like me to create a "Reset" script so you can restore all the leaves instantly while testing your project?