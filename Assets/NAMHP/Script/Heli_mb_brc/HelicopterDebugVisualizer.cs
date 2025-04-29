using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class HelicopterDebugVisualizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HelicopterMi28Controller helicopterController;

    [Header("Visualization Settings")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showWaypointPath = true;
    [SerializeField] private bool showRotorSpeedIndicator = true;
    [SerializeField] private bool showHeightIndicator = true;
    [SerializeField] private bool showStateInfo = true;

    [Header("Visual Style")]
    [SerializeField] private Color pathColor = new Color(0.2f, 0.8f, 0.2f, 0.7f);
    [SerializeField] private Color currentWaypointColor = Color.green;
    [SerializeField] private Color futureWaypointColor = new Color(0.5f, 0.5f, 1f, 0.6f);
    [SerializeField] private Color pastWaypointColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);
    [SerializeField] private float waypointSphereRadius = 2f;
    [SerializeField] private float lineWidth = 3f;

    [Header("On-Screen HUD")]
    [SerializeField] private bool showOnScreenHUD = true;
    [SerializeField] private Vector2 hudPosition = new Vector2(20, 20);
    [SerializeField] private int fontSize = 16;
    [SerializeField] private Font customFont;

    // Reference to the helicopter's waypoints
    [SerializeField] private List<Transform> waypoints;
    private int currentWaypointIndex = 0;

    private void Awake()
    {
        waypoints = helicopterController.waypoints;
    }

    private void Start()
    {
        if (helicopterController == null)
            helicopterController = GetComponent<HelicopterMi28Controller>();

        if (helicopterController == null)
        {
            Debug.LogError("HelicopterDebugVisualizer: No HelicopterController found!");
            enabled = false;
            return;
        }

        // Get the waypoints from the helicopter controller
        waypoints = helicopterController.waypoints;
    }

    private void Update()
    {
        // Update current waypoint index from helicopter controller
        currentWaypointIndex = helicopterController.currentWaypointIndex;
    }

    private void OnGUI()
    {
        if (!showOnScreenHUD || !showDebugInfo)
            return;

        GUI.skin.font = customFont != null ? customFont : GUI.skin.font;
        GUI.skin.label.fontSize = fontSize;

        GUILayout.BeginArea(new Rect(hudPosition.x, hudPosition.y, 300, 400));

        GUILayout.BeginVertical("box");

        GUILayout.Label("<b>HELICOPTER DEBUG</b>");

        if (showStateInfo)
        {
            GUILayout.Space(10);
            GUILayout.Label($"<b>State:</b> {helicopterController.currentState}");
        }

        if (showRotorSpeedIndicator)
        {
            GUILayout.Space(5);
            float rotorSpeed = helicopterController.currentRotorSpeed;
            float maxRotorSpeed = helicopterController.maxRotorSpeed;
            float rotorPercentage = (rotorSpeed / maxRotorSpeed) * 100f;

            GUILayout.Label($"<b>Rotor Speed:</b> {rotorSpeed:F1} RPM ({rotorPercentage:F1}%)");
            DrawProgressBar(rotorPercentage);
        }

        if (showHeightIndicator)
        {
            GUILayout.Space(5);
            float currentHeight = transform.position.y;
            float maxHeight = helicopterController.maxHeight;
            float heightPercentage = (currentHeight / maxHeight) * 100f;

            GUILayout.Label($"<b>Current Height:</b> {currentHeight:F1}m / {maxHeight:F1}m");
            DrawProgressBar(heightPercentage);
        }

        if (showWaypointPath && waypoints != null && waypoints.Count > 0)
        {
            GUILayout.Space(5);
            GUILayout.Label($"<b>Waypoint:</b> {currentWaypointIndex} / {waypoints.Count }");

            if (currentWaypointIndex < waypoints.Count)
            {
                Vector3 targetPos = waypoints[currentWaypointIndex].position;
                float distance = Vector3.Distance(transform.position, targetPos);
                GUILayout.Label($"<b>Distance to target:</b> {distance:F1}m");
            }
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void DrawProgressBar(float percentage)
    {
        Rect rect = GUILayoutUtility.GetRect(290, 20);

        // Background
        GUI.Box(rect, "");

        // Fill area based on percentage
        Rect fillRect = new Rect(rect.x + 2, rect.y + 2, (rect.width - 4) * (percentage / 100f), rect.height - 4);
        Color originalColor = GUI.color;
        GUI.color = Color.Lerp(Color.red, Color.green, percentage / 100f);
        GUI.Box(fillRect, "");
        GUI.color = originalColor;

        // Text overlay
        string text = $"{percentage:F1}%";
        GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
        centeredStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Label(rect, text, centeredStyle);
    }

    private void OnDrawGizmos()
    {
        if (!showDebugInfo || waypoints == null)
            return;

        // Draw waypoint spheres
        if (showWaypointPath)
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null)
                    continue;

                // Select color based on whether this waypoint is past, current, or future
                Gizmos.color = (i < currentWaypointIndex) ? pastWaypointColor :
                               (i == currentWaypointIndex) ? currentWaypointColor :
                               futureWaypointColor;

                Gizmos.DrawSphere(waypoints[i].position, waypointSphereRadius);

#if UNITY_EDITOR
                // Draw waypoint number (only in Editor)
                Handles.Label(waypoints[i].position + Vector3.up * waypointSphereRadius,
                            $"Waypoint {i + 1}",
                            new GUIStyle() {
                                normal = new GUIStyleState() { textColor = Color.white },
                                fontSize = 14
                            });
#endif
            }

            // Draw path lines
            Gizmos.color = pathColor;
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }

#if UNITY_EDITOR
        // Draw helicopter state (only in Editor)
        if (showStateInfo && helicopterController != null)
        {
            Handles.Label(transform.position + Vector3.up * 5f,
                        $"State: {helicopterController.currentState}",
                        new GUIStyle() {
                            normal = new GUIStyleState() { textColor = Color.white },
                            fontSize = 14,
                            alignment = TextAnchor.MiddleCenter
                        });
        }
#endif

        // Draw direction vector
        Gizmos.color = Color.blue;
        Vector3 direction = transform.forward * 5f;
        Gizmos.DrawRay(transform.position, direction);

        // Draw up vector
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up * 3f);
    }
}