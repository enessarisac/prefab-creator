using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;

public class PrefabCreator : EditorWindow
{
    private string prefabPath = "";

    [HideInInspector] GameObject gameObject;
    public bool IsCheckingUnityComponents = false;
    public GameObject[] objects;
    private string ObjectName;
    [MenuItem("Window/PrefabCreator")]
    private static void ShowWindow()
    {
        GetWindow<PrefabCreator>("PrefabCreator");
    }
    private void OnGUI()
    {
        GUILayout.Label("PrefabCreator V1.2 made by Enes Sarisac", EditorStyles.boldLabel);

        EditorGUILayout.Space(20f);
        GUILayout.Label("Please add only identical objects to the list. ", EditorStyles.boldLabel);
        GUILayout.Label("Saves the first object in the list as prefab.", EditorStyles.boldLabel);
        GUILayout.Label("It saves the values ​​of the others and converts it to prefab (Except Unity Components).", EditorStyles.boldLabel);

        EditorGUILayout.Space(20f);

        prefabPath = EditorGUILayout.TextField("Prefab Path : ", prefabPath);

        EditorGUILayout.Space(25f);

        GUILayout.Label("Select objects to change", EditorStyles.boldLabel);
        EditorGUILayout.Space(10f);
        IsCheckingUnityComponents = EditorGUILayout.Toggle("Check Unity Components", IsCheckingUnityComponents);
        ScriptableObject scriptableObj = this;
        SerializedObject serialObj = new SerializedObject(scriptableObj);
        SerializedProperty serialProp = serialObj.FindProperty("objects");
        EditorGUILayout.PropertyField(serialProp, true);
        serialObj.ApplyModifiedProperties();
        if (GUILayout.Button("Change objects to the prefab"))
        {
            if (objects.Length == 0)
            {
                Debug.LogWarning("Please select objects");
                return;
            }
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
    void SetFields(GameObject oldObject, GameObject newObject)
    {
        if (oldObject.GetComponent<RectTransform>() == null)
        {
            newObject.transform.position = oldObject.transform.position;
            newObject.transform.rotation = oldObject.transform.rotation;
            newObject.transform.localScale = oldObject.transform.localScale;
        }
        else
        {
            var oldObjectRectTransform = oldObject.GetComponent<RectTransform>();
            var newObjectRectTransform = newObject.GetComponent<RectTransform>();
            newObjectRectTransform.anchoredPosition = oldObjectRectTransform.anchoredPosition;
            newObjectRectTransform.anchorMax = oldObjectRectTransform.anchorMax;
            newObjectRectTransform.anchorMin = oldObjectRectTransform.anchorMin;
            newObjectRectTransform.offsetMax = oldObjectRectTransform.offsetMax;
            newObjectRectTransform.offsetMin = oldObjectRectTransform.offsetMin;
            newObjectRectTransform.pivot = oldObjectRectTransform.pivot;
            newObjectRectTransform.sizeDelta = oldObjectRectTransform.sizeDelta;
            newObjectRectTransform.SetPositionAndRotation(oldObjectRectTransform.position, oldObjectRectTransform.rotation);
            newObjectRectTransform.localScale = oldObjectRectTransform.localScale;

        }
        newObject.transform.SetParent(oldObject.transform.parent);
        newObject.tag = oldObject.tag;
        newObject.layer = oldObject.layer;
        newObject.isStatic = oldObject.isStatic;
        newObject.name = oldObject.name;

        object[] oldObjectComponents = oldObject.GetComponents<Component>();
        Debug.Log("Components: " + oldObjectComponents.Length);
        if (IsCheckingUnityComponents)
        {
            CheckUnityComponents(newObject, oldObject);
        }
        foreach (var component in oldObjectComponents)
        {
            var oldObjectFields = component.GetType().GetFields();
            if (newObject.GetComponent(component.GetType()) == null)
            {
                newObject.AddComponent(component.GetType());
            }
            var newObjectFields = newObject.GetComponent(component.GetType()).GetType().GetFields();
            Debug.Log("FirstObjectComp: " + oldObjectFields.Length + " PrefabObjectComp: " + newObjectFields.Length);

            foreach (var field in oldObjectFields)
            {

                foreach (var prefabField in newObjectFields)
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
                        if ((object)value == (object)oldObject)
                        {
                            Debug.Log("FirstObject");
                            field.SetValue(component, newObject);
                        }

                        prefabField.SetValue(newObject.GetComponent(component.GetType()), field.GetValue(component));
                    }
                }
            }
        }
        DestroyImmediate(oldObject);
    }
    public void CheckUnityComponents(GameObject newObject, GameObject oldObject)
    {
        var replacedObjectComponents = oldObject.GetComponents<Component>();
        Debug.Log("Components: " + replacedObjectComponents.Length);
        foreach (var component in replacedObjectComponents)
        {
            Debug.Log(component.GetType());
            //check component is rigidbody
            if (component.GetType() == typeof(Rigidbody))
            {
                if (newObject.GetComponent<Rigidbody>() == null)
                {
                    newObject.AddComponent<Rigidbody>();
                }
                Debug.Log("Rigidbody");
                Rigidbody rigidbody = (Rigidbody)component;
                Rigidbody prefabRigidbody = newObject.GetComponent<Rigidbody>();
                prefabRigidbody.mass = rigidbody.mass;
                prefabRigidbody.drag = rigidbody.drag;
                prefabRigidbody.angularDrag = rigidbody.angularDrag;
                prefabRigidbody.useGravity = rigidbody.useGravity;
                prefabRigidbody.isKinematic = rigidbody.isKinematic;
                prefabRigidbody.interpolation = rigidbody.interpolation;
                prefabRigidbody.collisionDetectionMode = rigidbody.collisionDetectionMode;
                prefabRigidbody.constraints = rigidbody.constraints;
                prefabRigidbody.centerOfMass = rigidbody.centerOfMass;
                prefabRigidbody.inertiaTensorRotation = rigidbody.inertiaTensorRotation;
                prefabRigidbody.inertiaTensor = rigidbody.inertiaTensor;
                prefabRigidbody.sleepThreshold = rigidbody.sleepThreshold;
                prefabRigidbody.maxAngularVelocity = rigidbody.maxAngularVelocity;
                prefabRigidbody.solverIterations = rigidbody.solverIterations;
                prefabRigidbody.solverVelocityIterations = rigidbody.solverVelocityIterations;
                prefabRigidbody.position = rigidbody.position;
                prefabRigidbody.rotation = rigidbody.rotation;
                prefabRigidbody.velocity = rigidbody.velocity;
                prefabRigidbody.angularVelocity = rigidbody.angularVelocity;
            }
            if (component.GetType() == typeof(BoxCollider))
            {
                if (newObject.GetComponent<BoxCollider>() == null)
                {
                    newObject.AddComponent<BoxCollider>();
                }
                Debug.Log("BoxCollider");
                BoxCollider boxCollider = (BoxCollider)component;
                BoxCollider prefabBoxCollider = newObject.GetComponent<BoxCollider>();
                prefabBoxCollider.center = boxCollider.center;
                prefabBoxCollider.size = boxCollider.size;
                prefabBoxCollider.isTrigger = boxCollider.isTrigger;
                prefabBoxCollider.material = boxCollider.material;
                if (boxCollider.material != null)
                    prefabBoxCollider.sharedMaterial = boxCollider.sharedMaterial;

                prefabBoxCollider.enabled = boxCollider.enabled;
            }
            if (component.GetType() == typeof(MeshCollider))
            {
                if (newObject.GetComponent<MeshCollider>() == null)
                {
                    newObject.AddComponent<MeshCollider>();
                }
                Debug.Log("MeshCollider");
                MeshCollider meshCollider = (MeshCollider)component;
                MeshCollider prefabMeshCollider = newObject.GetComponent<MeshCollider>();
                prefabMeshCollider.sharedMesh = meshCollider.sharedMesh;
                prefabMeshCollider.convex = meshCollider.convex;
                prefabMeshCollider.isTrigger = meshCollider.isTrigger;
                prefabMeshCollider.material = meshCollider.material;
                prefabMeshCollider.sharedMaterial = meshCollider.sharedMaterial;
                prefabMeshCollider.enabled = meshCollider.enabled;
            }
            if (component.GetType() == typeof(SphereCollider))
            {
                if (newObject.GetComponent<SphereCollider>() == null)
                {
                    newObject.AddComponent<SphereCollider>();
                }
                Debug.Log("SphereCollider");
                SphereCollider sphereCollider = (SphereCollider)component;
                SphereCollider prefabSphereCollider = newObject.GetComponent<SphereCollider>();
                prefabSphereCollider.center = sphereCollider.center;
                prefabSphereCollider.radius = sphereCollider.radius;
                prefabSphereCollider.isTrigger = sphereCollider.isTrigger;
                prefabSphereCollider.material = sphereCollider.material;
                prefabSphereCollider.sharedMaterial = sphereCollider.sharedMaterial;
                prefabSphereCollider.enabled = sphereCollider.enabled;
            }
            if (component.GetType() == typeof(CapsuleCollider))
            {
                if (newObject.GetComponent<CapsuleCollider>() == null)
                {
                    newObject.AddComponent<CapsuleCollider>();
                }
                Debug.Log("CapsuleCollider");
                CapsuleCollider capsuleCollider = (CapsuleCollider)component;
                CapsuleCollider prefabCapsuleCollider = newObject.GetComponent<CapsuleCollider>();
                prefabCapsuleCollider.center = capsuleCollider.center;
                prefabCapsuleCollider.radius = capsuleCollider.radius;
                prefabCapsuleCollider.height = capsuleCollider.height;
                prefabCapsuleCollider.direction = capsuleCollider.direction;
                prefabCapsuleCollider.isTrigger = capsuleCollider.isTrigger;
                prefabCapsuleCollider.material = capsuleCollider.material;
                prefabCapsuleCollider.sharedMaterial = capsuleCollider.sharedMaterial;
                prefabCapsuleCollider.enabled = capsuleCollider.enabled;
            }
            if (component.GetType() == typeof(MeshRenderer))
            {
                if (newObject.GetComponent<MeshRenderer>() == null)
                {
                    newObject.AddComponent<MeshRenderer>();
                }
                Debug.Log("MeshRenderer");
                MeshRenderer meshRenderer = (MeshRenderer)component;
                MeshRenderer prefabMeshRenderer = newObject.GetComponent<MeshRenderer>();
                prefabMeshRenderer.sharedMaterial = meshRenderer.sharedMaterial;
                prefabMeshRenderer.enabled = meshRenderer.enabled;
                prefabMeshRenderer.shadowCastingMode = meshRenderer.shadowCastingMode;
                prefabMeshRenderer.receiveShadows = meshRenderer.receiveShadows;
                prefabMeshRenderer.lightProbeUsage = meshRenderer.lightProbeUsage;
                prefabMeshRenderer.reflectionProbeUsage = meshRenderer.reflectionProbeUsage;
                prefabMeshRenderer.motionVectorGenerationMode = meshRenderer.motionVectorGenerationMode;
                prefabMeshRenderer.lightProbeProxyVolumeOverride = meshRenderer.lightProbeProxyVolumeOverride;
                prefabMeshRenderer.realtimeLightmapIndex = meshRenderer.realtimeLightmapIndex;
                prefabMeshRenderer.lightmapIndex = meshRenderer.lightmapIndex;
                prefabMeshRenderer.lightmapScaleOffset = meshRenderer.lightmapScaleOffset;
                prefabMeshRenderer.realtimeLightmapScaleOffset = meshRenderer.realtimeLightmapScaleOffset;
                prefabMeshRenderer.probeAnchor = meshRenderer.probeAnchor;
                prefabMeshRenderer.sortingLayerID = meshRenderer.sortingLayerID;
                prefabMeshRenderer.sortingLayerName = meshRenderer.sortingLayerName;
                prefabMeshRenderer.sortingOrder = meshRenderer.sortingOrder;
                prefabMeshRenderer.allowOcclusionWhenDynamic = meshRenderer.allowOcclusionWhenDynamic;

            }
            if (component.GetType() == typeof(MeshFilter))
            {
                if (newObject.GetComponent<MeshFilter>() == null)
                {
                    newObject.AddComponent<MeshFilter>();
                }
                Debug.Log("MeshFilter");
                MeshFilter meshFilter = (MeshFilter)component;
                MeshFilter prefabMeshFilter = newObject.GetComponent<MeshFilter>();
                prefabMeshFilter.sharedMesh = meshFilter.sharedMesh;
            }
            if (component.GetType() == typeof(SkinnedMeshRenderer))
            {
                if (newObject.GetComponent<SkinnedMeshRenderer>() == null)
                {
                    newObject.AddComponent<SkinnedMeshRenderer>();
                }
                Debug.Log("SkinnedMeshRenderer");
                SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)component;
                SkinnedMeshRenderer prefabSkinnedMeshRenderer = newObject.GetComponent<SkinnedMeshRenderer>();
                prefabSkinnedMeshRenderer.material = skinnedMeshRenderer.material;
                prefabSkinnedMeshRenderer.sharedMaterial = skinnedMeshRenderer.sharedMaterial;
                prefabSkinnedMeshRenderer.enabled = skinnedMeshRenderer.enabled;
            }
            if (component.GetType() == typeof(Animator))
            {
                if (newObject.GetComponent<Animator>() == null)
                {
                    newObject.AddComponent<Animator>();
                }
                Debug.Log("Animator");
                Animator animator = (Animator)component;
                Animator prefabAnimator = newObject.GetComponent<Animator>();
                prefabAnimator.runtimeAnimatorController = animator.runtimeAnimatorController;
                prefabAnimator.avatar = animator.avatar;
                prefabAnimator.enabled = animator.enabled;
            }
            if (component.GetType() == typeof(AudioSource))
            {
                if (newObject.GetComponent<AudioSource>() == null)
                {
                    newObject.AddComponent<AudioSource>();
                }
                Debug.Log("AudioSource");
                AudioSource audioSource = (AudioSource)component;
                AudioSource prefabAudioSource = newObject.GetComponent<AudioSource>();
                prefabAudioSource.clip = audioSource.clip;
                prefabAudioSource.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;
                prefabAudioSource.playOnAwake = audioSource.playOnAwake;
                prefabAudioSource.loop = audioSource.loop;
                prefabAudioSource.mute = audioSource.mute;
                prefabAudioSource.bypassEffects = audioSource.bypassEffects;
                prefabAudioSource.bypassListenerEffects = audioSource.bypassListenerEffects;
                prefabAudioSource.bypassReverbZones = audioSource.bypassReverbZones;
                prefabAudioSource.dopplerLevel = audioSource.dopplerLevel;
                prefabAudioSource.spread = audioSource.spread;
                prefabAudioSource.priority = audioSource.priority;
                prefabAudioSource.volume = audioSource.volume;
                prefabAudioSource.pitch = audioSource.pitch;
                prefabAudioSource.panStereo = audioSource.panStereo;
                prefabAudioSource.reverbZoneMix = audioSource.reverbZoneMix;
                prefabAudioSource.rolloffMode = audioSource.rolloffMode;
                prefabAudioSource.minDistance = audioSource.minDistance;
                prefabAudioSource.maxDistance = audioSource.maxDistance;
                prefabAudioSource.spatialBlend = audioSource.spatialBlend;
                prefabAudioSource.spatialize = audioSource.spatialize;
                prefabAudioSource.spatializePostEffects = audioSource.spatializePostEffects;
                prefabAudioSource.enabled = audioSource.enabled;
            }
            if (component.GetType() == typeof(Image))
            {
                if (newObject.GetComponent<Image>() == null)
                {
                    newObject.AddComponent<Image>();
                }
                Debug.Log("Image");
                Image image = (Image)component;
                Image prefabImage = newObject.GetComponent<Image>();
                prefabImage.sprite = image.sprite;
                prefabImage.color = image.color;
                prefabImage.material = image.material;
                prefabImage.raycastTarget = image.raycastTarget;
                prefabImage.enabled = image.enabled;
            }
            if (component.GetType() == typeof(Text))
            {
                if (newObject.GetComponent<Text>() == null)
                {
                    newObject.AddComponent<Text>();
                }
                Debug.Log("Text");
                Text text = (Text)component;
                Text prefabText = newObject.GetComponent<Text>();
                prefabText.text = text.text;
                prefabText.font = text.font;
                prefabText.fontSize = text.fontSize;
                prefabText.fontStyle = text.fontStyle;
                prefabText.alignment = text.alignment;
                prefabText.alignByGeometry = text.alignByGeometry;
                prefabText.horizontalOverflow = text.horizontalOverflow;
                prefabText.verticalOverflow = text.verticalOverflow;
                prefabText.resizeTextForBestFit = text.resizeTextForBestFit;
                prefabText.resizeTextMinSize = text.resizeTextMinSize;
                prefabText.resizeTextMaxSize = text.resizeTextMaxSize;
                prefabText.color = text.color;
                prefabText.material = text.material;
                prefabText.raycastTarget = text.raycastTarget;
                prefabText.enabled = text.enabled;
            }
            if (component.GetType() == typeof(RawImage))
            {
                if (newObject.GetComponent<RawImage>() == null)
                {
                    newObject.AddComponent<RawImage>();
                }
                Debug.Log("RawImage");
                RawImage rawImage = (RawImage)component;
                RawImage prefabRawImage = newObject.GetComponent<RawImage>();
                prefabRawImage.texture = rawImage.texture;
                prefabRawImage.color = rawImage.color;
                prefabRawImage.material = rawImage.material;
                prefabRawImage.raycastTarget = rawImage.raycastTarget;
                prefabRawImage.enabled = rawImage.enabled;
            }
            if (component.GetType() == typeof(Button))
            {
                if (newObject.GetComponent<Button>() == null)
                {
                    newObject.AddComponent<Button>();
                }
                Debug.Log("Button");
                Button button = (Button)component;
                Button prefabButton = newObject.GetComponent<Button>();
                prefabButton.transition = button.transition;
                prefabButton.colors = button.colors;
                prefabButton.spriteState = button.spriteState;
                prefabButton.animationTriggers = button.animationTriggers;
                prefabButton.targetGraphic = button.targetGraphic;
                prefabButton.enabled = button.enabled;
            }
            if (component.GetType() == typeof(Toggle))
            {
                if (newObject.GetComponent<Toggle>() == null)
                {
                    newObject.AddComponent<Toggle>();
                }
                Debug.Log("Toggle");
                Toggle toggle = (Toggle)component;
                Toggle prefabToggle = newObject.GetComponent<Toggle>();
                prefabToggle.transition = toggle.transition;
                prefabToggle.colors = toggle.colors;
                prefabToggle.spriteState = toggle.spriteState;
                prefabToggle.animationTriggers = toggle.animationTriggers;
                prefabToggle.targetGraphic = toggle.targetGraphic;
                prefabToggle.isOn = toggle.isOn;
                prefabToggle.enabled = toggle.enabled;
            }
            if (component.GetType() == typeof(Slider))
            {
                if (newObject.GetComponent<Slider>() == null)
                {
                    newObject.AddComponent<Slider>();
                }
                Debug.Log("Slider");
                Slider slider = (Slider)component;
                Slider prefabSlider = newObject.GetComponent<Slider>();
                prefabSlider.transition = slider.transition;
                prefabSlider.colors = slider.colors;
                prefabSlider.spriteState = slider.spriteState;
                prefabSlider.animationTriggers = slider.animationTriggers;
                prefabSlider.targetGraphic = slider.targetGraphic;
                prefabSlider.direction = slider.direction;
                prefabSlider.minValue = slider.minValue;
                prefabSlider.maxValue = slider.maxValue;
                prefabSlider.wholeNumbers = slider.wholeNumbers;
                prefabSlider.value = slider.value;
                prefabSlider.enabled = slider.enabled;
            }
            if (component.GetType() == typeof(Scrollbar))
            {
                if (newObject.GetComponent<Scrollbar>() == null)
                {
                    newObject.AddComponent<Scrollbar>();
                }
                Debug.Log("Scrollbar");
                Scrollbar scrollbar = (Scrollbar)component;
                Scrollbar prefabScrollbar = newObject.GetComponent<Scrollbar>();
                prefabScrollbar.transition = scrollbar.transition;
                prefabScrollbar.colors = scrollbar.colors;
                prefabScrollbar.spriteState = scrollbar.spriteState;
                prefabScrollbar.animationTriggers = scrollbar.animationTriggers;
                prefabScrollbar.targetGraphic = scrollbar.targetGraphic;
                prefabScrollbar.direction = scrollbar.direction;
                prefabScrollbar.value = scrollbar.value;
                prefabScrollbar.size = scrollbar.size;
                prefabScrollbar.numberOfSteps = scrollbar.numberOfSteps;
                prefabScrollbar.enabled = scrollbar.enabled;
            }
            if (component.GetType() == typeof(Dropdown))
            {
                if (newObject.GetComponent<Dropdown>() == null)
                {
                    newObject.AddComponent<Dropdown>();
                }
                Debug.Log("Dropdown");
                Dropdown dropdown = (Dropdown)component;
                Dropdown prefabDropdown = newObject.GetComponent<Dropdown>();
                prefabDropdown.transition = dropdown.transition;
                prefabDropdown.colors = dropdown.colors;
                prefabDropdown.spriteState = dropdown.spriteState;
                prefabDropdown.animationTriggers = dropdown.animationTriggers;
                prefabDropdown.targetGraphic = dropdown.targetGraphic;
                prefabDropdown.captionText = dropdown.captionText;
                prefabDropdown.captionImage = dropdown.captionImage;
                prefabDropdown.itemText = dropdown.itemText;
                prefabDropdown.itemImage = dropdown.itemImage;
                prefabDropdown.options = dropdown.options;
                prefabDropdown.value = dropdown.value;
                prefabDropdown.enabled = dropdown.enabled;
            }
            if (component.GetType() == typeof(InputField))
            {
                if (newObject.GetComponent<InputField>() == null)
                {
                    newObject.AddComponent<InputField>();
                }
                Debug.Log("InputField");
                InputField inputField = (InputField)component;
                InputField prefabInputField = newObject.GetComponent<InputField>();
                prefabInputField.transition = inputField.transition;
                prefabInputField.colors = inputField.colors;
                prefabInputField.spriteState = inputField.spriteState;
                prefabInputField.animationTriggers = inputField.animationTriggers;
                prefabInputField.targetGraphic = inputField.targetGraphic;
                prefabInputField.text = inputField.text;
                prefabInputField.placeholder = inputField.placeholder;
                prefabInputField.contentType = inputField.contentType;
                prefabInputField.lineType = inputField.lineType;
                prefabInputField.inputType = inputField.inputType;
                prefabInputField.keyboardType = inputField.keyboardType;
                prefabInputField.characterValidation = inputField.characterValidation;
                prefabInputField.characterLimit = inputField.characterLimit;
                prefabInputField.onEndEdit = inputField.onEndEdit;
                prefabInputField.enabled = inputField.enabled;
            }

        }
    }
}



