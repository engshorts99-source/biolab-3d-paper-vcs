#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using BioLab.Core;

namespace BioLab.Editor
{
    public static class BioLabSetup
    {
        [MenuItem("BioLab/Build Main Laboratory Scene")]
        public static void BuildMainScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Create Directional Light (Sun / Main Cleanroom Diffuse Key Light)
            GameObject lightObj = new GameObject("Key_Directional_Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1.0f, 0.97f, 0.92f);
            light.intensity = 0.92f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0);

            // Create Master GameManager
            GameObject gmObj = new GameObject("Master_GameManager");
            var gm = gmObj.AddComponent<GameManager>();

            // Explicitly load and assign the Toon shader so Unity build never strips it!
            Shader toonShader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/GenshinToonNPR.shader");
            if (toonShader != null)
            {
                gm.toonShader = toonShader;
                Debug.Log("[BioLab] Successfully bound GenshinToonNPR shader to GameManager.");
            }

            Shader faceShader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/GenshinFace.shader");
            if (faceShader != null)
            {
                gm.faceShader = faceShader;
                Debug.Log("[BioLab] Successfully bound GenshinFace shader to GameManager.");
            }

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/BioCleanroomMain.unity");
            Debug.Log("[BioLab] Successfully built and saved BioCleanroomMain.unity scene!");
        }
    }
}
#endif
