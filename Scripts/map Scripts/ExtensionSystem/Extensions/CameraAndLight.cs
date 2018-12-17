#if GAIA_PRESENT && UNITY_EDITOR

using UnityEngine;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine.Rendering;

namespace Gaia.GX.ProceduralWorlds
{
    /// <summary>
    /// Simple camera and light FX for Gaia.
    /// </summary>
    public class CameraAndLight : MonoBehaviour
    {
        #region Generic informational methods

        /// <summary>
        /// Returns the publisher name if provided. 
        /// This will override the publisher name in the namespace ie Gaia.GX.PublisherName
        /// </summary>
        /// <returns>Publisher name</returns>
        public static string GetPublisherName()
        {
            return "Procedural Worlds";
        }

        /// <summary>
        /// Returns the package name if provided
        /// This will override the package name in the class name ie public class PackageName.
        /// </summary>
        /// <returns>Package name</returns>
        public static string GetPackageName()
        {
            return "Camera And Light";
        }

        #endregion

        #region Methods exposed by Gaia as buttons must be prefixed with GX_

        public static void GX_About()
        {
            EditorUtility.DisplayDialog("About Camera and Light", "Camera and Light was put together for your convenience with assistance and coaching from Josh Savage (@TerrainBuilder), Don Anderson (@Olander), and the Unity community as a whole.", "OK");
        }

        public static void GX_SetLinearDeferredLighting()
        {
            PlayerSettings.colorSpace = ColorSpace.Linear;

            #if UNITY_5_5_OR_NEWER && UNITY_EDITOR

            var tier1 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier1);
            tier1.renderingPath = RenderingPath.DeferredShading;
            EditorGraphicsSettings.SetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier1, tier1);

            var tier2 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier2);
            tier2.renderingPath = RenderingPath.DeferredShading;
            EditorGraphicsSettings.SetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier2, tier2);

            var tier3 = EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier3);
            tier3.renderingPath = RenderingPath.DeferredShading;
            EditorGraphicsSettings.SetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier3, tier3);

#else
            PlayerSettings.renderingPath = RenderingPath.DeferredShading;
#endif
        }

        /// <summary>
        /// Add camera effects to the main camera in the scene, will ignore if they are already there
        /// </summary>
        public static void GX_CreateCameraEffects()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                camera = FindObjectOfType<Camera>();
            }
            if (camera == null)
            {
                EditorUtility.DisplayDialog("OOPS!", "Could not find camera to add camera effects to. Please add a camera to your scene.", "OK");
                return;
            }

            //Set up the camera
            #if UNITY_5_6_OR_NEWER
            camera.allowHDR = true;
            #else
            camera.hdr = true;
            #endif
            camera.farClipPlane = GetRangeFromTerrain() * 2f;

            //Now set up camera FX
            GameObject cameraObj = camera.gameObject;

            //Field info
            FieldInfo fi = null;

            //Add flare
            if (cameraObj.GetComponent<FlareLayer>() == null)
            {
                cameraObj.AddComponent<FlareLayer>();
            }

            //Set up POSTFX
            #if UNITY_5_4_OR_NEWER

            try {
                Type postProcessingBehaviourType = Gaia.Utils.GetType("UnityEngine.PostProcessing.PostProcessingBehaviour");
                if (postProcessingBehaviourType == null)
                {
                    EditorUtility.DisplayDialog("OOPS!",
                        "Could not add post processing. Please download and import the Unity Post Processing Stack.",
                        "OK");
                    Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/content/83912");
                    return;
                }
                else
                {
                    //Type postProcessingProfileType = Gaia.Utils.GetType("UnityEngine.PostProcessing.PostProcessingProfile");

                    var postProcessingBehaviour = cameraObj.GetComponent(postProcessingBehaviourType);
                    if (postProcessingBehaviour == null)
                    {
                        postProcessingBehaviour = cameraObj.AddComponent(postProcessingBehaviourType);
                    }
                    if (postProcessingBehaviour != null)
                    {
                        //Grab the default Gaia Post Processing Profile
                        ScriptableObject ppProfile = Gaia.Utils.GetAsset("Gaia Post FX 1.asset", typeof(ScriptableObject)) as ScriptableObject;

                        fi = postProcessingBehaviourType.GetField("profile", BindingFlags.Public | BindingFlags.Instance);
                        if (fi != null)
                        {
                            fi.SetValue(postProcessingBehaviour, ppProfile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #if UNITY_EDITOR
                EditorUtility.DisplayDialog("OOPS!", "Unable to add Post Processing Stack!! Reason : " + ex.Message, "OK");
                #else
                Debug.LogError("Unable to add Post Processing Stack!! Reason : " + ex.Message);    
                #endif
            }
            
            #else

            try
            { 

                //Add anti aliasing
                Type antiAliasingType = Gaia.Utils.GetType("UnityStandardAssets.ImageEffects.Antialiasing");
                if (antiAliasingType == null)
                {
                    EditorUtility.DisplayDialog("OOPS!", "Could not add antialiasing. Please find import Standard Effects Package : Assets -> Import Package -> Effects.", "OK");
                    return;
                }
                else
                {
                    var antiAliasing = cameraObj.GetComponent(antiAliasingType);
                    if (antiAliasing == null)
                    {
                        antiAliasing = cameraObj.AddComponent(antiAliasingType);
                    }
                }

                //Add SSAO
                Type ssaoType = Gaia.Utils.GetType("UnityStandardAssets.ImageEffects.ScreenSpaceAmbientOcclusion");
                if (ssaoType == null)
                {
                    EditorUtility.DisplayDialog("OOPS!", "Could not add screen space abient occlusion. Please find import Standard Effects Package : Assets -> Import Package -> Effects.", "OK");
                    return;
                }
                else
                {
                    var ssao = cameraObj.GetComponent(ssaoType);
                    if (ssao == null)
                    {
                        ssao = cameraObj.AddComponent(ssaoType);
                    }
                }

                Type bloomType = Gaia.Utils.GetType("UnityStandardAssets.ImageEffects.BloomOptimized");
                if (bloomType == null)
                {
                    EditorUtility.DisplayDialog("OOPS!", "Could not add bloom. Please find import Standard Effects Package : Assets -> Import Package -> Effects.", "OK");
                    return;
                }
                else
                {
                    var bloom = cameraObj.GetComponent(bloomType);
                    if (bloom == null)
                    {
                        bloom = cameraObj.AddComponent(bloomType);
                    }
                    fi = bloomType.GetField("threshold", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(bloom, 0.25f);
                    }
                    fi = bloomType.GetField("intensity", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(bloom, 0.25f);
                    }
                    fi = bloomType.GetField("blurSize", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(bloom, 1f);
                    }
                    fi = bloomType.GetField("blurIterations", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(bloom, 2);
                    }
                    fi = bloomType.GetField("blurType", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        //bloom.blurType = UnityStandardAssets.ImageEffects.BloomOptimized.BlurType.Standard;
                        fi.SetValue(bloom, 0);
                    }
                }

                Type dofType = Gaia.Utils.GetType("UnityStandardAssets.ImageEffects.DepthOfField");
                if (dofType == null)
                {
                    EditorUtility.DisplayDialog("OOPS!", "Could not add depth of field. Please find import Standard Effects Package : Assets -> Import Package -> Effects.", "OK");
                    return;
                }
                else
                {
                    var dof = cameraObj.GetComponent(dofType);
                    if (dof == null)
                    {
                        dof = cameraObj.AddComponent(dofType);
                    }
                    fi = dofType.GetField("focalLength", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(dof, 2.5f);
                    }
                    fi = dofType.GetField("focalSize", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(dof, 2f);
                    }
                    fi = dofType.GetField("aperture", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(dof, 0.5f);
                    }
                    fi = dofType.GetField("nearBlur", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(dof, false);
                    }
                    fi = dofType.GetField("highResolution", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(dof, false);
                    }
                    fi = dofType.GetField("focalSize", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(dof, 2f);
                    }
                    fi = dofType.GetField("maxBlurSize", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(dof, 1f);
                    }
                    fi = dofType.GetField("blurType", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        //dof.blurType = UnityStandardAssets.ImageEffects.DepthOfField.BlurType.DiscBlur;
                        fi.SetValue(dof, 0);
                    }
                    fi = dofType.GetField("blurSampleCount", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        //dof.blurSampleCount = UnityStandardAssets.ImageEffects.DepthOfField.BlurSampleCount.Medium;
                        fi.SetValue(dof, 1);
                    }
                }

                //Rather nasty hack around the fact that unity made this a private class
                Type fogType = Gaia.Utils.GetType("UnityStandardAssets.ImageEffects.GlobalFog");
                if (fogType == null)
                {
                    EditorUtility.DisplayDialog("OOPS!", "Could not add global fog. Please find import Standard Effects Package : Assets -> Import Package -> Effects.", "OK");
                    return;
                }
                else
                {
                    var globalFog = cameraObj.GetComponent(fogType);
                    if (globalFog == null)
                    {
                        globalFog = cameraObj.AddComponent(fogType);
                    }
                    fi = fogType.GetField("heightFog", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(globalFog, false);
                    }
                    fi = fogType.GetField("startDistance", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(globalFog, 100);
                    }
                }

                Type toneMappingType = Gaia.Utils.GetType("UnityStandardAssets.ImageEffects.Tonemapping");
                if (toneMappingType == null)
                {
                    EditorUtility.DisplayDialog("OOPS!", "Could not add tonemapping. Please find import Standard Effects Package : Assets -> Import Package -> Effects.", "OK");
                    return;
                }
                else
                {
                    var toneMapping = cameraObj.GetComponent(toneMappingType);
                    if (toneMapping == null)
                    {
                        toneMapping = cameraObj.AddComponent(toneMappingType);
                    }
                    //tm.type = UnityStandardAssets.ImageEffects.Tonemapping.TonemapperType.SimpleReinhard;
                    fi = toneMappingType.GetField("type", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(toneMapping, 0);
                    }
                    fi = toneMappingType.GetField("exposureAdjustment", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(toneMapping, 2f);
                    }
                }

                Type vignetteType = Gaia.Utils.GetType("UnityStandardAssets.ImageEffects.VignetteAndChromaticAberration");
                if (vignetteType == null)
                {
                    EditorUtility.DisplayDialog("OOPS!", "Could not add vignette. Please find import Standard Effects Package : Assets -> Import Package -> Effects.", "OK");
                    return;
                }
                else
                {
                    var vignette = cameraObj.GetComponent(vignetteType);
                    if (vignette == null)
                    {
                        vignette = cameraObj.AddComponent(vignetteType);
                    }
                    fi = vignetteType.GetField("intensity", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        #if UNITY_5_1 || UNITY_5_2_0 || UNITY_5_2_1
                        fi.SetValue(vignette, 3.5f);
                        #else
                        fi.SetValue(vignette, 0.32f);
                        #endif
                    }
                    fi = vignetteType.GetField("blur", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(vignette, 0.2f);
                    }
                    fi = vignetteType.GetField("blurSpread", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(vignette, 0.15f);
                    }
                    fi = vignetteType.GetField("mode", BindingFlags.Public | BindingFlags.Instance);
                    if (fi != null)
                    {
                        fi.SetValue(vignette, 0); //Simple
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
#if UNITY_EDITOR
                EditorUtility.DisplayDialog("OOPS!", "Unable to add Cammera Effects!! If you would like camera effects as well then please import the standard Unity Effects package and then try again.", "OK");
#else
                Debug.LogError("Unable to add Camera Effects");    
#endif
            }

#endif
        }

        /// <summary>
        /// Set gradient based lighting
        /// </summary>
        /// <param name="sky">Sky colour "#AABBCC"</param>
        /// <param name="equator">Equator color "#AABBCC"</param>
        /// <param name="ground">Ground color "#AABBCC"</param>
        public static void SetTrilight(string sky, string equator, string ground)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;

            Color skyColor = new Color();
            ColorUtility.TryParseHtmlString(sky, out skyColor);
            RenderSettings.ambientSkyColor = skyColor;

            Color equatorColor = new Color();
            ColorUtility.TryParseHtmlString(equator, out equatorColor);
            RenderSettings.ambientEquatorColor = equatorColor;

            Color groundColor = new Color();
            ColorUtility.TryParseHtmlString(ground, out groundColor);
            RenderSettings.ambientGroundColor = groundColor;
        }

        public static void GX_SetMorningLight()
        {
            GameObject lightObj = GameObject.Find("Directional Light");
            if (lightObj == null)
            {
                lightObj = GameObject.Find("Directional light");
            }
            if (lightObj != null)
            {
                lightObj.transform.localRotation = Quaternion.Euler(new Vector3(18f, 176, 8f));
                Light light = lightObj.GetComponent<Light>();
                if (light != null)
                {
                    light.color = new Color(201f / 255f, 136f / 255f, 66f / 255f, 1f);
                    light.intensity = 1.5f;
                    string flarePath = GetAssetPath("Gaia85mmLens");
                    if (!string.IsNullOrEmpty(flarePath))
                    {
                        light.flare = AssetDatabase.LoadAssetAtPath<Flare>(flarePath);
                    }
                }

                //Set the skybox material
                string matPath = GetAssetPath("GaiaMorningMaterial");
                if (!string.IsNullOrEmpty(matPath))
                {
                    RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                }

                //Set the fog 
                RenderSettings.fog = true;
                RenderSettings.fogColor = new Color(221f/255f, 206f/255f, 189f/255f, 1f);
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogStartDistance = 100f;
                RenderSettings.fogDensity = 0.0008f;

                //Set the ambience
                SetTrilight("#838383", "#717C7C", "#3C280E");
            }
            else
            {
                Debug.Log("Unable to find Directional Light");
            }
        }

        /// <summary>
        /// Set the scene light to afternoon
        /// </summary>
        public static void GX_SetAfternoonLight()
        {
            GameObject lightObj = GameObject.Find("Directional Light");
            if (lightObj == null)
            {
                lightObj = GameObject.Find("Directional light");
            }
            if (lightObj != null)
            {
                lightObj.transform.localRotation = Quaternion.Euler(new Vector3(40f, 346f, -1.7f));
                Light light = lightObj.GetComponent<Light>();
                if (light != null)
                {
                    light.color = new Color(255f / 255f, 244f / 255f, 214f / 255f, 1f);
                    light.intensity = 1.1f;
                    string flarePath = GetAssetPath("Gaia85mmLens");
                    if (!string.IsNullOrEmpty(flarePath))
                    {
                        light.flare = AssetDatabase.LoadAssetAtPath<Flare>(flarePath);
                    }
                }

                //Set the skybox material
                string matPath = GetAssetPath("GaiaAfternoonMaterial");
                if (!string.IsNullOrEmpty(matPath))
                {
                    RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                }

                //Set the fog 
                RenderSettings.sun = light;
                RenderSettings.fog = true;
                //RenderSettings.fogColor = new Color(199f/255f, 199f/255f, 199f/255f, 1f);
                RenderSettings.fogColor = new Color(150f / 255f, 183f / 255f, 191f / 255f, 1f);
                RenderSettings.fogMode = FogMode.Linear;
                RenderSettings.fogStartDistance = 100f;
                RenderSettings.fogEndDistance = 2000f;
                //RenderSettings.fogDensity = 0.0015f;

                //Set the ambience
                SetTrilight("#BCBBDA", "#6C7394", "#878262");
            }
            else
            {
                Debug.Log("Unable to find Directional Light");
            }
        }


        /// <summary>
        /// Set the scene light to evening
        /// </summary>
        public static void GX_SetEveningLight()
        {
            GameObject lightObj = GameObject.Find("Directional Light");
            if (lightObj == null)
            {
                lightObj = GameObject.Find("Directional light");
            }
            if (lightObj != null)
            {
                lightObj.transform.localRotation = Quaternion.Euler(new Vector3(7f, 3f, -170f));
                Light light = lightObj.GetComponent<Light>();
                if (light != null)
                {
                    light.color = new Color(171f / 255f, 146f / 255f, 119f / 255f, 1f);
                    light.intensity = 1.36f;
                    string flarePath = GetAssetPath("Gaia85mmLens");
                    if (!string.IsNullOrEmpty(flarePath))
                    {
                        light.flare = AssetDatabase.LoadAssetAtPath<Flare>(flarePath);
                    }
                }

                //Set the skybox material
                string matPath = GetAssetPath("GaiaEveningMaterial");
                if (!string.IsNullOrEmpty(matPath))
                {
                    RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                }

                //Set the fog 
                RenderSettings.sun = light;
                RenderSettings.fog = true;
                RenderSettings.fogColor = new Color(197f / 255f, 172f / 255f, 146f / 255f, 1f);
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogStartDistance = 100f;
                RenderSettings.fogDensity = 0.0009f;

                //Set the ambience
                SetTrilight("#58677F", "#5A554B", "#141806");
            }
            else
            {
                Debug.Log("Unable to find Directional Light");
            }
        }

        /// <summary>
        /// Set the scene light to night
        /// </summary>
        public static void GX_SetNightLight()
        {
            GameObject lightObj = GameObject.Find("Directional Light");
            if (lightObj == null)
            {
                lightObj = GameObject.Find("Directional light");
            }
            if (lightObj != null)
            {
                lightObj.transform.localRotation = Quaternion.Euler(new Vector3(58f, -173f, 14f));
                Light light = lightObj.GetComponent<Light>();
                if (light != null)
                {
                    light.color = new Color(200f / 255f, 209f / 255f, 227f / 255f, 1f);
                    light.intensity = 0.5f;
                    string flarePath = GetAssetPath("GaiaSubtle4");
                    if (!string.IsNullOrEmpty(flarePath))
                    {
                        light.flare = AssetDatabase.LoadAssetAtPath<Flare>(flarePath);
                    }
                }

                //Set the skybox material
                string matPath = GetAssetPath("GaiaNightMaterial");
                if (!string.IsNullOrEmpty(matPath))
                {
                    RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                }

                //Set the fog 
                RenderSettings.sun = light;
                RenderSettings.fog = true;
                RenderSettings.fogColor = new Color(9f / 255f, 9f / 255f, 9f / 255f, 1f);
                RenderSettings.fogMode = FogMode.Exponential;
                RenderSettings.fogStartDistance = 200f;
                RenderSettings.fogDensity = 0.005f;
            }
            else
            {
                Debug.Log("Unable to find Directional Light");
            }
        }

#endregion

        #region Helper methods

        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns></returns>
        private static string GetAssetPath(string name)
        {
            #if UNITY_EDITOR
            string[] assets = AssetDatabase.FindAssets(name, null);
            if (assets.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(assets[0]);
            }
            #endif
            return null;
        }

        /// <summary>
        /// Get the asset prefab if we can find it in the project
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static GameObject GetAssetPrefab(string name)
        {
            #if UNITY_EDITOR
            string[] assets = AssetDatabase.FindAssets(name, null);
            for (int idx = 0; idx < assets.Length; idx++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assets[idx]);
                if (path.Contains(".prefab"))
                {
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }
            }
            #endif
            return null;
        }

        /// <summary>
        /// Get the range from the terrain or return a default range if no terrain
        /// </summary>
        /// <returns></returns>
        public static float GetRangeFromTerrain()
        {
            Terrain terrain = GetActiveTerrain();
            if (terrain != null)
            {
                return Mathf.Max(terrain.terrainData.size.x, terrain.terrainData.size.z) / 2f;
            }
            return 1024f;
        }

        /// <summary>
        /// Get the currently active terrain - or any terrain
        /// </summary>
        /// <returns>A terrain if there is one</returns>
        public static Terrain GetActiveTerrain()
        {
            //Grab active terrain if we can
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null && terrain.isActiveAndEnabled)
            {
                return terrain;
            }

            //Then check rest of terrains
            for (int idx = 0; idx < Terrain.activeTerrains.Length; idx++)
            {
                terrain = Terrain.activeTerrains[idx];
                if (terrain != null && terrain.isActiveAndEnabled)
                {
                    return terrain;
                }
            }
            return null;
        }

        #endregion
    }
}

#endif