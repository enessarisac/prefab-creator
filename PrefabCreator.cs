using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class PrefabCreator : EditorWindow
{
    private string prefabPath = "";

    GameObject gameObject;
    List<GameObject> objectList;

    public GameObject[] objects;

    [MenuItem("Window/PrefabCreator")]
    private static void ShowWindow()
    {
        GetWindow<PrefabCreator>("PrefabCreator");
    }
    private void OnGUI()
    {
        GUILayout.Label("PrefabCreator V1.0", EditorStyles.boldLabel);

        EditorGUILayout.Space(10f);

        gameObject = EditorGUILayout.ObjectField("Object", gameObject, typeof(GameObject), false) as GameObject;

        prefabPath = EditorGUILayout.TextField("Prefab Path : ", prefabPath);

        EditorGUILayout.Space(10f);

        GUILayout.Label("Girdiginiz ismi iceren butun objeleri sectiginiz prefaba donusturur.");
        EditorGUILayout.Space(3f);
        if (GUILayout.Button("Isim ile Prefab yapma"))
        {
            objectList = new List<GameObject>(FindObjectsOfType<GameObject>());

            foreach (GameObject Object in objectList)
            {
                if (Object.name.Contains(gameObject.name))
                {
                    ChangeToPrefab(Object);
                }
            }


        }


        EditorGUILayout.Space(50f);

        GUILayout.Label("Secilen objeleri prefaba donusturur.");

        ScriptableObject scriptableObj = this;
        SerializedObject serialObj = new SerializedObject(scriptableObj);
        SerializedProperty serialProp = serialObj.FindProperty("objects");
        EditorGUILayout.PropertyField(serialProp, true);
        serialObj.ApplyModifiedProperties();

        EditorGUILayout.Space(10f);

        if (GUILayout.Button("Secili Objeleri Prefab yapma"))
        {
            foreach (GameObject Object in objects)
            {
                ChangeToPrefab(Object);               
            }
        }
    }
    void ChangeToPrefab(GameObject go)
    {
        gameObject = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)));
        gameObject.transform.position = go.transform.position;
        DestroyImmediate(go);
    }
}
    

