using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
public class WaypointCreator : EditorWindow
{
    private MovingPlatform targetPlatform;
    private string waypointName = "Waypoint";
    
    [MenuItem("Tools/Platform Waypoints Creator")]
    public static void ShowWindow()
    {
        GetWindow<WaypointCreator>("Waypoint Creator");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Moving Platform Waypoint Creator", EditorStyles.boldLabel);
        
        targetPlatform = (MovingPlatform)EditorGUILayout.ObjectField(
            "Target Platform:", 
            targetPlatform, 
            typeof(MovingPlatform), 
            true
        );
        
        waypointName = EditorGUILayout.TextField("Waypoint Name:", waypointName);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Create Waypoint at Scene View Position"))
        {
            if (targetPlatform != null)
            {
                CreateWaypoint(SceneView.lastActiveSceneView.camera.transform.position);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select a target Moving Platform", "OK");
            }
        }
        
        if (GUILayout.Button("Create Waypoint at Platform Position"))
        {
            if (targetPlatform != null)
            {
                CreateWaypoint(targetPlatform.transform.position);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please select a target Moving Platform", "OK");
            }
        }
    }
    
    private void CreateWaypoint(Vector3 position)
    {
        // Create waypoint GameObject
        GameObject waypoint = new GameObject(waypointName);
        
        // Position it
        waypoint.transform.position = position;
        
        // Make it a child of the platform's parent
        if (targetPlatform.transform.parent != null)
        {
            waypoint.transform.parent = targetPlatform.transform.parent;
        }
        
        // Record the waypoint in the platform's waypoint list
        SerializedObject serializedObject = new SerializedObject(targetPlatform);
        SerializedProperty waypointsProperty = serializedObject.FindProperty("waypoints");
        
        int index = waypointsProperty.arraySize;
        waypointsProperty.arraySize++;
        waypointsProperty.GetArrayElementAtIndex(index).objectReferenceValue = waypoint.transform;
        
        serializedObject.ApplyModifiedProperties();
        
        // Select the newly created waypoint
        Selection.activeGameObject = waypoint;
        
        // Focus on it in the scene view
        SceneView.lastActiveSceneView.FrameSelected();
    }
}
#endif 