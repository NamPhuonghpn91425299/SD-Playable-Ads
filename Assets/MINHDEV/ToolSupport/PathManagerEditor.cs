#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using static GameConstants;

public class PathManagerTool : EditorWindow
{
    private PathManager pathManager;
    private SerializedObject serializedPathManager;
    private SerializedProperty listWaypointProperty;
    private GameObject pathWayPoint;

    [MenuItem("Tools/Path Manager Tool")]
    public static void ShowWindow()
    {
        GetWindow<PathManagerTool>("Path Manager Tool");
    }

    private void OnEnable()
    {
        pathManager = FindObjectOfType<PathManager>();
        if (pathManager != null)
        {
            serializedPathManager = new SerializedObject(pathManager);
            listWaypointProperty = serializedPathManager.FindProperty("Listwaypoint");
        }
    }

    private void OnGUI()
    {
        if (pathManager == null)
        {
            EditorGUILayout.HelpBox("PathManager not found in the scene. Please add a PathManager component to a GameObject.", MessageType.Warning);
            if (GUILayout.Button("Refresh"))
            {
                OnEnable();
            }
            return;
        }

        serializedPathManager.Update();

        pathWayPoint = (GameObject)EditorGUILayout.ObjectField("Path WayPoint", pathWayPoint, typeof(GameObject), true);
        EditorGUILayout.PropertyField(listWaypointProperty, new GUIContent("WayPoint Lists"), true);

        GUILayout.Space(10);
        GUILayout.Label("Custom Tool", EditorStyles.boldLabel);

        // Add button to create waypoints
        if (GUILayout.Button("Create Waypoints"))
        {
            CreateWaypoints();
        }

        if (GUILayout.Button("Randomize Waypoints"))
        {
            RandomizeWaypoints();
        }

        serializedPathManager.ApplyModifiedProperties();
    }

    private void CreateWaypoints()
    {
        if (pathWayPoint == null)
        {
            Debug.LogError("Please assign PathWayPoint.");
            return;
        }

        for (int i = 0; i < listWaypointProperty.arraySize; i++)
        {
            SerializedProperty wayPointlistProperty = listWaypointProperty.GetArrayElementAtIndex(i);
            SerializedProperty samplePrefabProperty = wayPointlistProperty.FindPropertyRelative("SamplePrefab");

            if (samplePrefabProperty == null || samplePrefabProperty.objectReferenceValue == null)
            {
                Debug.LogError($"Please assign SamplePrefab for {wayPointlistProperty.FindPropertyRelative("botType").enumDisplayNames}.");
                continue;
            }

            int pointsPerWay = wayPointlistProperty.FindPropertyRelative("WayPointTotal").intValue;
            CreateBotTypeWaypoints(wayPointlistProperty, (GameObject)samplePrefabProperty.objectReferenceValue, pointsPerWay);
        }
    }

    private void RandomizeWaypoints()
    {
        for (int i = 0; i < listWaypointProperty.arraySize; i++)
        {
            SerializedProperty wayPointlistProperty = listWaypointProperty.GetArrayElementAtIndex(i);
            SerializedProperty listLimitWayPointProperty = wayPointlistProperty.FindPropertyRelative("listLimitWayPoint");

            SerializedProperty wayPointList = wayPointlistProperty.FindPropertyRelative("_wayPointlist");
            if (wayPointList.arraySize > 0)
            {
                for (int j = 0; j < wayPointList.arraySize; j++)
                {
                    SerializedProperty wayPointProperty = wayPointList.GetArrayElementAtIndex(j);
                    SerializedProperty wayPointsProperty = wayPointProperty.FindPropertyRelative("WayPoints");

                    for (int k = 0; k < wayPointsProperty.arraySize; k++)
                    {
                        if (k < listLimitWayPointProperty.arraySize)
                        {
                            SerializedProperty limitWayPointElement = listLimitWayPointProperty.GetArrayElementAtIndex(k).FindPropertyRelative("LimitWayPoint");
                            List<Vector3> limitedPositions = new List<Vector3>();

                            for (int l = 0; l < limitWayPointElement.arraySize; l++)
                            {
                                var element = limitWayPointElement.GetArrayElementAtIndex(l).FindPropertyRelative("Limited");
                                for (int m = 0; m < element.arraySize; m++)
                                {
                                    Transform limitedTransform = element.GetArrayElementAtIndex(m).objectReferenceValue as Transform;
                                    if (limitedTransform != null)
                                    {
                                        limitedPositions.Add(limitedTransform.position);
                                    }
                                }
                            }

                            if (limitedPositions.Count > 0)
                            {
                                Vector3 randomPosition = GetRandomPositionWithinBounds(limitedPositions);

                                if (wayPointsProperty.GetArrayElementAtIndex(k).objectReferenceValue != null)
                                {
                                    ((Transform)wayPointsProperty.GetArrayElementAtIndex(k).objectReferenceValue).position = randomPosition;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private Vector3 GetRandomPositionWithinBounds(List<Vector3> points)
    {
        if (points.Count < 3) return Vector3.zero;

        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        foreach (var point in points)
        {
            if (point.x < minX) minX = point.x;
            if (point.x > maxX) maxX = point.x;
            if (point.y < minY) minY = point.y;
            if (point.y > maxY) maxY = point.y;
            if (point.z < minZ) minZ = point.z;
            if (point.z > maxZ) maxZ = point.z;
        }

        float randomX = UnityEngine.Random.Range(minX, maxX);
        float randomY = UnityEngine.Random.Range(minY, maxY);
        float randomZ = UnityEngine.Random.Range(minZ, maxZ);

        return new Vector3(randomX, randomY, randomZ);
    }

    private void CreateBotTypeWaypoints(SerializedProperty wayPointlistProperty, GameObject prefab, int pointsPerWay)
    {
        BotType botType = (BotType)wayPointlistProperty.FindPropertyRelative("botType").enumValueIndex;
        GameObject botParent = new GameObject(botType.ToString());
        botParent.transform.parent = pathWayPoint.transform;

        SerializedProperty wayPointList = wayPointlistProperty.FindPropertyRelative("_wayPointlist");
        for (int i = 0; i < wayPointList.arraySize; i++)
        {
            SerializedProperty wayPointProperty = wayPointList.GetArrayElementAtIndex(i);
            SerializedProperty wayPointsProperty = wayPointProperty.FindPropertyRelative("WayPoints");

            GameObject wayParent = new GameObject("Way" + (i + 1));
            wayParent.transform.parent = botParent.transform;

            for (int j = 0; j < pointsPerWay; j++)
            {
                GameObject point = Instantiate(prefab);
                point.name = "Point" + (j + 1);
                point.transform.parent = wayParent.transform;
                wayPointsProperty.InsertArrayElementAtIndex(j);
                wayPointsProperty.GetArrayElementAtIndex(j).objectReferenceValue = point.transform;
            }
        }
    }
}
#endif 
