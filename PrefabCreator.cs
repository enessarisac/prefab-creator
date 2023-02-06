using UnityEngine;
using UnityEditor;

public class PrefabCreator : EditorWindow
{
    private string prefabPath = "";

    [HideInInspector] GameObject gameObject;
    public GameObject[] objects;
    private string ObjectName;
    [MenuItem("Window/PrefabCreator")]
    private static void ShowWindow()
    {
        GetWindow<PrefabCreator>("PrefabCreator");
    }
    private void OnGUI()
    {
        GUILayout.Label("PrefabCreator V1.1 made by Enes Sarisac", EditorStyles.boldLabel);
        
        EditorGUILayout.Space(20f); 
        GUILayout.Label("Please add only identical objects to the list. ", EditorStyles.boldLabel);
        GUILayout.Label("Saves the first object in the list as prefab.", EditorStyles.boldLabel);
        GUILayout.Label("It saves the values of the others and converts it to prefab (Except Unity Components).", EditorStyles.boldLabel);
        
        EditorGUILayout.Space(20f);

        prefabPath = EditorGUILayout.TextField("Prefab Path : ", prefabPath);

        EditorGUILayout.Space(25f);

        GUILayout.Label("Select objects to change", EditorStyles.boldLabel);
        EditorGUILayout.Space(10f);

        ScriptableObject scriptableObj = this;
        SerializedObject serialObj = new SerializedObject(scriptableObj);
        SerializedProperty serialProp = serialObj.FindProperty("objects");
        EditorGUILayout.PropertyField(serialProp, true);
        serialObj.ApplyModifiedProperties();
        if (GUILayout.Button("Change objects to the prefab"))
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (i == 0)
                {
                    ObjectName = "/" + objects[i].name + ".prefab";
                    PrefabUtility.SaveAsPrefabAsset(objects[i], prefabPath + ObjectName);
                }
                ChangeToPrefab(objects[i]);
            }
            objects = null;
        }
    }
    void ChangeToPrefab(GameObject go)
    {
        gameObject = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath(prefabPath + ObjectName, typeof(GameObject)));
        SetFields(go, gameObject);

    }
    void SetFields(GameObject firstObject, GameObject prefabObject)
    {
        prefabObject.transform.position = firstObject.transform.position;
        prefabObject.transform.rotation = firstObject.transform.rotation;
        prefabObject.transform.localScale = firstObject.transform.localScale;
        prefabObject.transform.parent = firstObject.transform.parent;
        prefabObject.name = firstObject.name;

        object[] components = firstObject.GetComponents<Component>();
        Debug.Log("Components: " + components.Length);
        foreach (var component in components)
        {
            var firstObjectComponents = component.GetType().GetFields();
            var prefabObjectComponents = prefabObject.GetComponent(component.GetType()).GetType().GetFields();
            Debug.Log("FirstObjectComp: " + firstObjectComponents.Length + " PrefabObjectComp: " + prefabObjectComponents.Length);
            foreach (var field in firstObjectComponents)
            {
                foreach (var prefabField in prefabObjectComponents)
                {
                    if (field.Name == prefabField.Name)
                    {
                        Debug.Log(field.Name + " " + prefabField.Name);
                        if (field.GetValue(component) == null)
                        {
                            Debug.Log("Null");
                            continue;
                        }
                        object value = field.GetValue(component);
                        if ((object)value == (object)firstObject)
                        {
                            Debug.Log("FirstObject");
                            field.SetValue(component, prefabObject);
                        }
                        prefabField.SetValue(prefabObject.GetComponent(component.GetType()), field.GetValue(component));
                    }
                }
            }
        }
        DestroyImmediate(firstObject);
    }
}
