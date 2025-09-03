using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlatformMover))]
public class PlatformMoverEditor : Editor
{
    SerializedProperty destinationsProp;
    SerializedProperty speedProp;
    SerializedProperty loopProp;
    SerializedProperty arriveThresholdProp;
    SerializedProperty onArriveAtIndexProp;
    SerializedProperty riderAnchorProp;

    private void OnEnable()
    {
        destinationsProp = serializedObject.FindProperty("destinations");
        speedProp = serializedObject.FindProperty("speed");
        loopProp = serializedObject.FindProperty("loop");
        arriveThresholdProp = serializedObject.FindProperty("arriveThreshold");
        onArriveAtIndexProp = serializedObject.FindProperty("onArriveAtIndex");

    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Movement fields
        EditorGUILayout.PropertyField(speedProp);
        EditorGUILayout.PropertyField(loopProp);
        EditorGUILayout.PropertyField(arriveThresholdProp);

        EditorGUILayout.Space();

        // Global event
        EditorGUILayout.PropertyField(onArriveAtIndexProp, new GUIContent("OnArriveAtIndex (int)"));
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Destinations", EditorStyles.boldLabel);

        // Add Destination button: erstellt ein Child-GameObject und fügt es als Destination ein
        if (GUILayout.Button("Add Destination"))
        {
            PlatformMover pm = (PlatformMover)target;
            GameObject go = new GameObject("Destination " + (pm.transform.childCount + 1));
            Undo.RegisterCreatedObjectUndo(go, "Create Destination");
            go.transform.parent = pm.transform;
            go.transform.position = pm.transform.position;

            // erweitere die serialisierten Daten
            Undo.RecordObject(pm, "Add Destination");
            destinationsProp.arraySize++;
            serializedObject.ApplyModifiedProperties();
            // setze das point-Reference
            SerializedProperty newElem = destinationsProp.GetArrayElementAtIndex(destinationsProp.arraySize - 1);
            newElem.FindPropertyRelative("point").objectReferenceValue = go.transform;
            newElem.FindPropertyRelative("waitTime").floatValue = 0f;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(pm);
        }

        EditorGUILayout.Space();

        // draw each element (inklusive nested fields)
        for (int i = 0; i < destinationsProp.arraySize; i++)
        {
            SerializedProperty element = destinationsProp.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Point " + i, EditorStyles.boldLabel);
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                SerializedProperty pointProp = element.FindPropertyRelative("point");
                if (pointProp != null && pointProp.objectReferenceValue != null)
                    Selection.activeObject = pointProp.objectReferenceValue;
            }
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                // falls das Transform ein Child ist, lösche das GameObject
                SerializedProperty pointProp = element.FindPropertyRelative("point");
                Transform toRemove = pointProp.objectReferenceValue as Transform;
                if (toRemove != null && toRemove.IsChildOf(((PlatformMover)target).transform))
                {
                    Undo.DestroyObjectImmediate(toRemove.gameObject);
                }

                // entferne das Element aus der Liste
                destinationsProp.DeleteArrayElementAtIndex(i);
                // falls noch ein null-Element übrig ist, nochmal löschen
                if (i < destinationsProp.arraySize)
                    destinationsProp.DeleteArrayElementAtIndex(i);

                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break; // Liste verändert -> Abbruch der For-Schleife
            }
            EditorGUILayout.EndHorizontal();

            // Zeige verschachtelte Felder (Transform, waitTime, onArrive)
            EditorGUILayout.PropertyField(element, GUIContent.none, true);

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Clear All (remove references)"))
        {
            if (EditorUtility.DisplayDialog("Clear all destinations", "Alle Destinationen-Referenzen entfernen? (GameObjects bleiben erhalten)", "Remove References", "Cancel"))
            {
                destinationsProp.ClearArray();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
