using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;
using UnityEngine.Networking;

namespace Gaia
{
    /// <summary>
    /// Handy helper for all things Gaia
    /// </summary>
    public class GaiaManagerEditor : EditorWindow
    {
        private bool m_positionChecked = false;
        private GUIStyle m_boxStyle;
        private GUIStyle m_wrapStyle;
        private GUIStyle m_titleStyle;
        private GUIStyle m_headingStyle;
        private GUIStyle m_bodyStyle;
        private GUIStyle m_linkStyle;
        private Vector2 m_scrollPosition = Vector2.zero;
        private GaiaSettings m_settings;
        private GaiaConstants.ManagerEditorMode m_managerMode = GaiaConstants.ManagerEditorMode.Standard;
        private string[] m_menuStrings = new string[] { "Standard", "Advanced", "GX", "More..." };
        private GaiaConstants.ManagerEditorNewsMode m_managerMoreMode = GaiaConstants.ManagerEditorNewsMode.MoreOnGaia;
        private string[] m_menuStrings2 = new string[] { "Tutorials & Support", "Partners & Extensions" };
        private IEnumerator m_updateCoroutine;

        //Extension manager
        bool m_needsScan = true;
        GaiaExtensionManager m_extensionMgr = new GaiaExtensionManager();
        private bool m_foldoutSession = false;
        private bool m_foldoutTerrain = false;
        private bool m_foldoutSpawners = false;
        private bool m_foldoutCharacters = false;
        private bool m_foldoutUtils = false;

        #region Gaia Menu Commands
        /// <summary>
        /// Show Gaia Manager editor window
        /// </summary>
        [MenuItem("Window/Gaia/Show Gaia Manager... %g", false, 40)]
        public static void ShowGaiaManager()
        {
            var manager = EditorWindow.GetWindow<Gaia.GaiaManagerEditor>(false, "Gaia Manager");
            manager.Show();
        }

        /// <summary>
        /// Show the forum
        /// </summary>
        [MenuItem("Window/Gaia/Show Forum...", false, 60)]
        public static void ShowForum()
        {
            Application.OpenURL(
                "http://www.procedural-worlds.com/forum/gaia/");
        }

        /// <summary>
        /// Show tutorial
        /// </summary>
        [MenuItem("Window/Gaia/Show Tutorials...", false, 61)]
        public static void ShowTutorial()
        {
            Application.OpenURL("http://www.procedural-worlds.com/gaia/?section=tutorials");
        }

        /// <summary>
        /// Show documentation
        /// </summary>
        [MenuItem("Window/Gaia/Show Extensions...", false, 62)]
        public static void ShowExtensions()
        {
            Application.OpenURL("http://www.procedural-worlds.com/gaia/?section=gaia-extensions");
        }

        /// <summary>
        /// Show review option
        /// </summary>
        [MenuItem("Window/Gaia/Please Review Gaia...", false, 63)]
        public static void ShowAssetStore()
        {
            Application.OpenURL("https://www.assetstore.unity3d.com/#!/content/42618?aid=1101lSqC");
        }

        /// <summary>
        /// Show documentation
        /// </summary>
        [MenuItem("Window/Gaia/Show CTS Terrain Shader...", false, 64)]
        public static void ShowCTS()
        {
            Application.OpenURL("http://www.procedural-worlds.com/cts/");
        }

        /// <summary>
        /// Show review option
        /// </summary>
        [MenuItem("Window/Gaia/Lodge Support Request...", false, 65)]
        public static void ShowSupport()
        {
            Application.OpenURL("https://proceduralworlds.freshdesk.com/support/home");
        }

        #endregion

        /// <summary>
        /// Creates a new Gaia settings asset
        /// </summary>
        /// <returns>New gaia settings asset</returns>
        public static GaiaSettings CreateSettingsAsset()
        {
            GaiaSettings settings = ScriptableObject.CreateInstance<Gaia.GaiaSettings>();
            AssetDatabase.CreateAsset(settings, "Assets/Gaia/Data/GaiaSettings.asset");
            AssetDatabase.SaveAssets();
            return settings;
        }

        /// <summary>
        /// Create and returns a defaults asset
        /// </summary>
        /// <returns>New defaults asset</returns>
        public static GaiaDefaults CreateDefaultsAsset()
        {
            GaiaDefaults defaults = ScriptableObject.CreateInstance<Gaia.GaiaDefaults>();
            AssetDatabase.CreateAsset(defaults, string.Format("Assets/Gaia/Data/GD-{0:yyyyMMdd-HHmmss}.asset", DateTime.Now));
            AssetDatabase.SaveAssets();
            return defaults;
        }

        /// <summary>
        /// Create and returns a resources asset
        /// </summary>
        /// <returns>New resources asset</returns>
        public static GaiaResource CreateResourcesAsset()
        {
            GaiaResource resources = ScriptableObject.CreateInstance<Gaia.GaiaResource>();
            AssetDatabase.CreateAsset(resources, string.Format("Assets/Gaia/Data/GR-{0:yyyyMMdd-HHmmss}.asset", DateTime.Now));
            AssetDatabase.SaveAssets();
            return resources;
        }

        /// <summary>
        /// Set up the Gaia Present defines
        /// </summary>
        public static void SetGaiaDefinesStatic()
        {
            string currBuildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            //Check for and inject GAIA_PRESENT
            if (!currBuildSettings.Contains("GAIA_PRESENT"))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, currBuildSettings + ";GAIA_PRESENT");
            }
        }

        /// <summary>
        /// See if we can preload the manager with existing settings
        /// </summary>
        void OnEnable()
        {
            //Signal we need a scan
            m_needsScan = true;
            
            //Set the Gaia directories up
            Utils.CreateGaiaAssetDirectories();

            //Get or create existing settings object
            if (m_settings == null)
            {
                m_settings = (GaiaSettings)Utils.GetAssetScriptableObject("GaiaSettings");
                if (m_settings == null)
                {
                    m_settings = CreateSettingsAsset();
                }
            }

            //Make sure we have defaults
            if (m_settings.m_currentDefaults == null)
            {
                m_settings.m_currentDefaults = (GaiaDefaults)Utils.GetAssetScriptableObject("GaiaDefaults"); 
                EditorUtility.SetDirty(m_settings);
            }

            //Grab first resource we can find
            if (m_settings.m_currentResources == null)
            {
                m_settings.m_currentResources = (GaiaResource)Utils.GetAssetScriptableObject("GaiaResources");
                EditorUtility.SetDirty(m_settings);
            }

            //Grab first game object resource we can find
            if (m_settings.m_currentGameObjectResources == null)
            {
                m_settings.m_currentGameObjectResources = m_settings.m_currentResources;
                EditorUtility.SetDirty(m_settings);
            }

            if (!Application.isPlaying)
            {
                StartEditorUpdates();
                m_updateCoroutine = GetNewsUpdate();
            }
        }

        void OnDisable()
        {
            StopEditorUpdates();
        }

        void OnGUI()
        {
            if (!m_positionChecked)
            {
                m_positionChecked = true;
                Rect scenePosition = new Rect(0f, 0f, 800f, 600f);
                if (SceneView.lastActiveSceneView != null)
                {
                    scenePosition = SceneView.lastActiveSceneView.position;
                }
                if (!maximized)
                {
                    //Check our position
                    Rect p = position;
                    if (p.x < scenePosition.xMin || p.x > scenePosition.xMax)
                    {
                        p.x = scenePosition.xMin + (((scenePosition.xMax - scenePosition.xMin)/2f) - (p.width/2f));
                    }
                    if (p.y < scenePosition.yMin || p.y > scenePosition.yMax)
                    {
                        p.y = scenePosition.yMin + 100f;
                    }
                    position = p;
                }
            }

            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box);
                m_boxStyle.normal.textColor = GUI.skin.label.normal.textColor;
                m_boxStyle.fontStyle = FontStyle.Bold;
                m_boxStyle.alignment = TextAnchor.UpperLeft;
            }

            //Setup the wrap style
            if (m_wrapStyle == null)
            {
                m_wrapStyle = new GUIStyle(GUI.skin.label);
                m_wrapStyle.fontStyle = FontStyle.Normal;
                m_wrapStyle.wordWrap = true;
            }

            if (m_bodyStyle == null)
            {
                //m_bodyStyle = new GUIStyle(EditorStyles.label);
                m_bodyStyle = new GUIStyle(GUI.skin.label);
                m_bodyStyle.fontStyle = FontStyle.Normal;
                m_bodyStyle.wordWrap = true;
                //m_bodyStyle.fontSize = 14;
            }

            if (m_titleStyle == null)
            {
                m_titleStyle = new GUIStyle(m_bodyStyle);
                m_titleStyle.fontStyle = FontStyle.Bold;
                m_titleStyle.fontSize = 20;
            }

            if (m_headingStyle == null)
            {
                m_headingStyle = new GUIStyle(m_bodyStyle);
                m_headingStyle.fontStyle = FontStyle.Bold;
                //m_headingStyle.fontSize = 16;
            }

            if (m_linkStyle == null)
            {
                m_linkStyle = new GUIStyle(m_bodyStyle);
                m_linkStyle.wordWrap = false;
                m_linkStyle.normal.textColor = new Color(0x00 / 255f, 0x78 / 255f, 0xDA / 255f, 1f);
                m_linkStyle.stretchWidth = false;
            }

            //Check for state of compiler
            if (EditorApplication.isCompiling)
            {
                m_needsScan = true;
            }

            //Text intro
            GUILayout.BeginVertical(string.Format("Gaia ({0}.{1})", Gaia.GaiaConstants.GaiaMajorVersion, Gaia.GaiaConstants.GaiaMinorVersion), m_boxStyle);
            GUILayout.Label("");
            GUILayout.EndVertical();

            EditorGUILayout.BeginVertical(m_boxStyle);

            GaiaConstants.EnvironmentControllerType targetControllerType = (GaiaConstants.EnvironmentControllerType)EditorGUILayout.EnumPopup(GetLabel("Controller"), m_settings.m_currentController);
            GaiaConstants.EnvironmentTarget targetEnv = (GaiaConstants.EnvironmentTarget)EditorGUILayout.EnumPopup(GetLabel("Environment"), m_settings.m_currentEnvironment);
            GaiaConstants.EnvironmentRenderer targetRenderer = (GaiaConstants.EnvironmentRenderer)EditorGUILayout.EnumPopup(GetLabel("Renderer"), m_settings.m_currentRenderer);
            GaiaConstants.EnvironmentSize targetSize = (GaiaConstants.EnvironmentSize)EditorGUILayout.EnumPopup(GetLabel("Terrain Size"), m_settings.m_currentSize);

            bool needsUpdate = false;
            if (targetEnv != m_settings.m_currentEnvironment)
            {
                switch (targetEnv)
                {
                    case GaiaConstants.EnvironmentTarget.UltraLight:
                        m_settings.m_currentDefaults = m_settings.m_ultraLightDefaults;
                        m_settings.m_currentResources = m_settings.m_ultraLightResources;
                        m_settings.m_currentGameObjectResources = m_settings.m_ultraLightGameObjectResources;
                        m_settings.m_currentWaterPrefabName = m_settings.m_waterMobilePrefabName;
                        targetSize = GaiaConstants.EnvironmentSize.Is512MetersSq;
                        break;
                    case GaiaConstants.EnvironmentTarget.MobileAndVR:
                        m_settings.m_currentDefaults = m_settings.m_mobileDefaults;
                        m_settings.m_currentResources = m_settings.m_mobileResources;
                        m_settings.m_currentGameObjectResources = m_settings.m_mobileGameObjectResources;
                        m_settings.m_currentWaterPrefabName = m_settings.m_waterMobilePrefabName;
                        targetSize = GaiaConstants.EnvironmentSize.Is1024MetersSq;
                        break;
                    case GaiaConstants.EnvironmentTarget.Desktop:
                        m_settings.m_currentDefaults = m_settings.m_desktopDefaults;
                        m_settings.m_currentResources = m_settings.m_desktopResources;
                        m_settings.m_currentGameObjectResources = m_settings.m_desktopGameObjectResources;
                        m_settings.m_currentWaterPrefabName = m_settings.m_waterPrefabName;
                        targetSize = GaiaConstants.EnvironmentSize.Is2048MetersSq;
                        break;
                    case GaiaConstants.EnvironmentTarget.PowerfulDesktop:
                        m_settings.m_currentDefaults = m_settings.m_powerDesktopDefaults;
                        m_settings.m_currentResources = m_settings.m_powerDesktopResources;
                        m_settings.m_currentGameObjectResources = m_settings.m_powerDesktopGameObjectResources;
                        m_settings.m_currentWaterPrefabName = m_settings.m_waterPrefabName;
                        targetSize = GaiaConstants.EnvironmentSize.Is2048MetersSq;
                        break;
                }
                EditorUtility.SetDirty(m_settings);
                needsUpdate = true;
            }

            if (targetControllerType != m_settings.m_currentController)
            {
                m_settings.m_currentController = targetControllerType;
                switch (targetControllerType)
                {
                    case GaiaConstants.EnvironmentControllerType.FirstPerson:
                        m_settings.m_currentPlayerPrefabName = m_settings.m_fpsPlayerPrefabName;
                        break;
                    case GaiaConstants.EnvironmentControllerType.ThirdPerson:
                        m_settings.m_currentPlayerPrefabName = m_settings.m_3pPlayerPrefabName;
                        break;
                    case GaiaConstants.EnvironmentControllerType.Rollerball:
                        m_settings.m_currentPlayerPrefabName = m_settings.m_rbPlayerPrefabName;
                        break;
                    case GaiaConstants.EnvironmentControllerType.FlyingCamera:
                        m_settings.m_currentPlayerPrefabName = "Flycam";
                        break;
                }
                EditorUtility.SetDirty(m_settings);
            }

            if (targetEnv != m_settings.m_currentEnvironment)
            {
                m_settings.m_currentEnvironment = targetEnv;
                EditorUtility.SetDirty(m_settings);
            }

            if (targetRenderer != m_settings.m_currentRenderer)
            {
                #if !UNITY_2018_1_OR_NEWER
                targetRenderer = GaiaConstants.EnvironmentRenderer.BuiltIn;
                #endif
                m_settings.m_currentRenderer = targetRenderer;
                EditorUtility.SetDirty(m_settings);
            }

            if (needsUpdate || targetSize != m_settings.m_currentSize)
            {
                m_settings.m_currentSize = targetSize;
                switch (targetSize)
                {
                    case GaiaConstants.EnvironmentSize.Is256MetersSq:
                        m_settings.m_currentDefaults.m_terrainSize = 256;
                        break;
                    case GaiaConstants.EnvironmentSize.Is512MetersSq:
                        m_settings.m_currentDefaults.m_terrainSize = 512;
                        break;
                    case GaiaConstants.EnvironmentSize.Is1024MetersSq:
                        m_settings.m_currentDefaults.m_terrainSize = 1024;
                        break;
                    case GaiaConstants.EnvironmentSize.Is2048MetersSq:
                        m_settings.m_currentDefaults.m_terrainSize = 2048;
                        break;
                    case GaiaConstants.EnvironmentSize.Is4096MetersSq:
                        m_settings.m_currentDefaults.m_terrainSize = 4096;
                        break;
                    case GaiaConstants.EnvironmentSize.Is8192MetersSq:
                        m_settings.m_currentDefaults.m_terrainSize = 8192;
                        break;
                    case GaiaConstants.EnvironmentSize.Is16384MetersSq:
                        m_settings.m_currentDefaults.m_terrainSize = 16384;
                        break;
                }

                switch (targetEnv)
                {
                    case GaiaConstants.EnvironmentTarget.UltraLight:
                        m_settings.m_currentDefaults.m_heightmapResolution = 33;
                        m_settings.m_currentDefaults.m_baseMapDist = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / 8, 256, 512);
                        m_settings.m_currentDefaults.m_detailResolution = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / 8, 128, 512);
                        m_settings.m_currentDefaults.m_controlTextureResolution = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / 8, 64, 512);
                        m_settings.m_currentDefaults.m_baseMapSize = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / 8, 64, 512);
                        break;
                    case GaiaConstants.EnvironmentTarget.MobileAndVR:
                        m_settings.m_currentDefaults.m_heightmapResolution = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / 4, 64, 512)+1;
                        m_settings.m_currentDefaults.m_baseMapDist = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / 4, 256, 512);
                        m_settings.m_currentDefaults.m_detailResolution = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / 4, 64, 512);
                        m_settings.m_currentDefaults.m_controlTextureResolution = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / 4, 64, 512);
                        m_settings.m_currentDefaults.m_baseMapSize = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / 4, 64, 512);
                        break;
                    case GaiaConstants.EnvironmentTarget.Desktop:
                        m_settings.m_currentDefaults.m_heightmapResolution = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / 2, 256, 2048) + 1;
                        m_settings.m_currentDefaults.m_baseMapDist = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / 2, 256, 2048);
                        m_settings.m_currentDefaults.m_detailResolution = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / 2, 256, 4096);
                        m_settings.m_currentDefaults.m_controlTextureResolution = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / 2, 256, 2048);
                        m_settings.m_currentDefaults.m_baseMapSize = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / 2, 256, 2048);
                        break;
                    case GaiaConstants.EnvironmentTarget.PowerfulDesktop:
                        m_settings.m_currentDefaults.m_heightmapResolution = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize, 256, 2048) + 1;
                        m_settings.m_currentDefaults.m_baseMapDist = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize, 256, 2048);
                        m_settings.m_currentDefaults.m_detailResolution = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize, 256, 4096);
                        m_settings.m_currentDefaults.m_controlTextureResolution = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize, 256, 2048);
                        m_settings.m_currentDefaults.m_baseMapSize = Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize, 256, 2048);
                        break;
                    case GaiaConstants.EnvironmentTarget.Custom:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                //m_settings.m_currentDefaults.m_baseMapSize = Mathf.Clamp(Mathf.Clamp()  , m_settings.m_currentDefaults.m_size);


                EditorUtility.SetDirty(m_settings);
                EditorUtility.SetDirty(m_settings.m_currentDefaults);
            }

            GUILayout.Space(2);

            EditorGUILayout.BeginHorizontal();
            m_settings.m_currentDefaults = (GaiaDefaults)EditorGUILayout.ObjectField(GetLabel("Terrain Defaults"), m_settings.m_currentDefaults, typeof(GaiaDefaults), false);
            if (GUILayout.Button(GetLabel("New"), GUILayout.Width(45), GUILayout.Height(16f)))
            {
                m_settings.m_currentDefaults = CreateDefaultsAsset();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(2);

            EditorGUILayout.BeginHorizontal();
            m_settings.m_currentResources = (GaiaResource)EditorGUILayout.ObjectField(GetLabel("Terrain Resources"), m_settings.m_currentResources, typeof(GaiaResource), false);
            if (GUILayout.Button(GetLabel("New"), GUILayout.Width(45), GUILayout.Height(16f)))
            {
                m_settings.m_currentResources = CreateResourcesAsset();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            m_settings.m_currentGameObjectResources = (GaiaResource)EditorGUILayout.ObjectField(GetLabel("GameObject Resources"), m_settings.m_currentGameObjectResources, typeof(GaiaResource), false);
            if (GUILayout.Button(GetLabel("New"), GUILayout.Width(45), GUILayout.Height(16f)))
            {
                m_settings.m_currentGameObjectResources = CreateResourcesAsset();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            m_managerMode = (GaiaConstants.ManagerEditorMode)GUILayout.Toolbar((int)m_managerMode, m_menuStrings);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2);

            if (m_managerMode == GaiaConstants.ManagerEditorMode.ShowMore)
            {
                GUILayout.Space(2);
                EditorGUILayout.BeginHorizontal();
                m_managerMoreMode = (GaiaConstants.ManagerEditorNewsMode)GUILayout.Toolbar((int)m_managerMoreMode, m_menuStrings2);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
            }

            //Scroll
            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition, false, false);

            switch (m_managerMode)
            {
                case GaiaConstants.ManagerEditorMode.Standard:
                    DrawStandardEditor();
                    break;
                case GaiaConstants.ManagerEditorMode.Advanced:
                    DrawAdvancedEditor();
                    break;
                case GaiaConstants.ManagerEditorMode.Extensions:
                    DrawExtensionsEditor();
                    break;
                case GaiaConstants.ManagerEditorMode.ShowMore:
                    DrawMoreEditor();
                    break;
            }

            //End scroll
            GUILayout.EndScrollView();

            //Bottom section
            if (!m_settings.m_hideHeroMessage)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("", GUILayout.ExpandHeight(true));
                GUILayout.BeginHorizontal(m_boxStyle);
//                GUILayout.BeginVertical();
//                GUILayout.Space(3f);
//                DrawImage(m_settings.m_latestNewsImage, 50f, 50f);
//                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                GUILayout.BeginHorizontal();
                if (DrawLinkHeaderText(m_settings.m_latestNewsTitle))
                {
                    Application.OpenURL(m_settings.m_latestNewsUrl);
                }

                if (DrawLinkHeaderText("Hide", GUILayout.Width(33f)))
                {
                    m_settings.m_hideHeroMessage = true;
                    EditorUtility.SetDirty(m_settings);
                }

                GUILayout.EndHorizontal();
                DrawBodyText(m_settings.m_latestNewsBody);
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }

        /// <summary>
        /// Draw the brief editor
        /// </summary>
        void DrawStandardEditor()
        {
            EditorGUI.indentLevel++;

            if (DrawLinkBody("Follow the workflow to create your scene. Click here for tutorials."))
            {
                Application.OpenURL("http://www.procedural-worlds.com/gaia/?section=tutorials");
            }
            GUILayout.Space(5f);

            if (DisplayButton(GetLabel("1. Create Terrain & Show Stamper")))
            {
                ShowSessionManager();
                CreateTerrain();
                ShowStamper();
            }

            EditorGUI.indentLevel++;
            if (DisplayButton(GetLabel("1A. Enhance Terrain")))
            {
                ShowTerrainUtilties();
            }
            EditorGUI.indentLevel--;

            if (DisplayButton(GetLabel("2. Create Spawners")))
            {
                //Only do this if we have 1 terrain
                if (!DisplayErrorIfInvalidTerrainCount(1))
                {
                    Spawner spawner;
                    //Create the spawners
                    spawner = CreateTextureSpawner().GetComponent<Spawner>();
                    if (spawner.m_activeRuleCnt == 0)
                    {
                        DestroyImmediate(spawner.gameObject);
                    }
                    spawner = CreateCoverageGameObjectSpawner().GetComponent<Spawner>();
                    if (spawner.m_activeRuleCnt == 0)
                    {
                        DestroyImmediate(spawner.gameObject);
                    }
                    spawner = CreateClusteredTreeSpawnerFromTerrainTrees().GetComponent<Spawner>();
                    if (spawner.m_activeRuleCnt == 0)
                    {
                        DestroyImmediate(spawner.gameObject);
                    }
                    spawner = CreateClusteredTreeSpawnerFromGameObjects().GetComponent<Spawner>();
                    if (spawner.m_activeRuleCnt == 0)
                    {
                        DestroyImmediate(spawner.gameObject);
                    }
                    spawner = CreateCoverageTreeSpawner().GetComponent<Spawner>();
                    if (spawner.m_activeRuleCnt == 0)
                    {
                        DestroyImmediate(spawner.gameObject);
                    }
                    spawner = CreateCoverageTreeSpawnerFromGameObjects().GetComponent<Spawner>();
                    if (spawner.m_activeRuleCnt == 0)
                    {
                        DestroyImmediate(spawner.gameObject);
                    }
                    spawner = CreateDetailSpawner().GetComponent<Spawner>();
                    if (spawner.m_activeRuleCnt == 0)
                    {
                        DestroyImmediate(spawner.gameObject);
                    }
                }
            }

            string buttonLabel;
            if (m_settings.m_currentEnvironment == GaiaConstants.EnvironmentTarget.UltraLight ||
                m_settings.m_currentEnvironment == GaiaConstants.EnvironmentTarget.MobileAndVR)
            {
                buttonLabel= "3. Create Player, Water and Screenshotter";
            }
            else
            {
                buttonLabel = "3. Create Player, Wind, Water and Screenshotter";
            }
            if (DisplayButton(GetLabel(buttonLabel)))
            {
                //Only do this if we have 1 terrain
                if (DisplayErrorIfInvalidTerrainCount(1))
                {
                    return;
                }

                CreatePlayer();
                if ( m_settings.m_currentEnvironment != GaiaConstants.EnvironmentTarget.UltraLight &&
                    m_settings.m_currentEnvironment != GaiaConstants.EnvironmentTarget.MobileAndVR)
                {
                    CreateWindZone();
                }
                CreateWater();
                Selection.activeGameObject = CreateScreenShotter();

                Debug.Log("Don't forget to go into the GX menu and set up your camera and lighting!");
            }

            EditorGUILayout.Space();
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draw the detailed editor
        /// </summary>
        void DrawAdvancedEditor()
        {
            EditorGUI.indentLevel++;

            //            if (DrawLinkHeaderText(GetLabel("Advanced Workflow")))
            //            {
            //                Application.OpenURL("http://www.procedural-worlds.com/gaia/tutorials/import-real-world-terrain/");
            //            }

            if (DrawLinkBody("Pick and choose your tasks. Click here for tutorials."))
            {
                Application.OpenURL("http://www.procedural-worlds.com/gaia/?section=tutorials");
            }

            GUILayout.Space(5f);

            if (m_foldoutSession = EditorGUILayout.Foldout(m_foldoutSession, GetLabel("1. Create Session Manager..."), true))
            {
                EditorGUI.indentLevel++;
                if (DisplayButton(GetLabel("Show Session Manager")))
                {
                    ShowSessionManager();
                }
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            if (m_foldoutTerrain = EditorGUILayout.Foldout(m_foldoutTerrain, GetLabel("2. Create your Terrain..."), true))
            {
                EditorGUI.indentLevel++;
                if (DisplayButton(GetLabel("Create Terrain")))
                {
                    CreateTerrain();
                }
                if (DisplayButton(GetLabel("Show Stamper")))
                {
                    ShowStamper();
                }
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            if (m_foldoutSpawners = EditorGUILayout.Foldout(m_foldoutSpawners, GetLabel("3. Create and configure your Spawners..."), true))
            {
                EditorGUI.indentLevel++;
                //if (DisplayButton(GetLabel("Create Stamp Spawner")))
                //{
                //    Selection.activeObject = CreateStampSpawner();
                //}
                if (DisplayButton(GetLabel("Create Coverage Texture Spawner")))
                {
                    Selection.activeObject = CreateTextureSpawner();
                }
                if (DisplayButton(GetLabel("Create Clustered Grass Spawner")))
                {
                    Selection.activeObject = CreateClusteredDetailSpawner();
                }
                if (DisplayButton(GetLabel("Create Coverage Grass Spawner")))
                {
                    Selection.activeObject = CreateDetailSpawner();
                }
                if (DisplayButton(GetLabel("Create Clustered Terrain Tree Spawner")))
                {
                    Selection.activeObject = CreateClusteredTreeSpawnerFromTerrainTrees();
                }
                if (DisplayButton(GetLabel("Create Clustered Prefab Tree Spawner")))
                {
                    Selection.activeObject = CreateClusteredTreeSpawnerFromGameObjects();
                }
                if (DisplayButton(GetLabel("Create Coverage Terrain Tree Spawner")))
                {
                    Selection.activeObject = CreateCoverageTreeSpawner();
                }
                if (DisplayButton(GetLabel("Create Coverage Prefab Tree Spawner")))
                {
                    Selection.activeObject = CreateCoverageTreeSpawnerFromGameObjects();
                }
                if (DisplayButton(GetLabel("Create Clustered Prefab Spawner")))
                {
                    Selection.activeObject = CreateClusteredGameObjectSpawner();
                }
                if (DisplayButton(GetLabel("Create Coverage Prefab Spawner")))
                {
                    Selection.activeObject = CreateCoverageGameObjectSpawner();
                }
                //if (DisplayButton(GetLabel("Create Group Spawner")))
                //{
                //    Selection.activeObject = FindOrCreateGroupSpawner();
                //}
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }
            if (m_foldoutCharacters = EditorGUILayout.Foldout(m_foldoutCharacters, GetLabel("4. Add common Game Objects..."), true))
            {
                EditorGUI.indentLevel++;
                if (DisplayButton(GetLabel("Add Character")))
                {
                    Selection.activeGameObject = CreatePlayer();
                }
                if (DisplayButton(GetLabel("Add Wind Zone")))
                {
                    Selection.activeGameObject = CreateWindZone();
                }
                if (DisplayButton(GetLabel("Add Water")))
                {
                    Selection.activeGameObject = CreateWater();
                }
                if (DisplayButton(GetLabel("Add Screen Shotter")))
                {
                    Selection.activeGameObject = CreateScreenShotter();
                }
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }

            if (m_foldoutUtils = EditorGUILayout.Foldout(m_foldoutUtils, GetLabel("5. Handy Utilities..."), true))
            {
                EditorGUI.indentLevel++;
                if (DisplayButton(GetLabel("Show Scanner")))
                {
                    Selection.activeGameObject = CreateScanner();
                }
                if (DisplayButton(GetLabel("Show Visualiser")))
                {
                    Selection.activeGameObject = ShowVisualiser();
                }
                if (DisplayButton(GetLabel("Show Terrain Utilities")))
                {
                    ShowTerrainUtilties();
                }
                if (DisplayButton(GetLabel("Show Splatmap Exporter")))
                {
                    ShowTexureMaskExporter();
                }
                if (DisplayButton(GetLabel("Show Grass Exporter")))
                {
                    ShowGrassMaskExporter();
                }
                if (DisplayButton(GetLabel("Show Mesh Exporter")))
                {
                    ShowTerrainObjExporter();
                }
                if (DisplayButton(GetLabel("Show Shore Exporter")))
                {
                    ExportShoremaskAsPNG();
                }
                if (DisplayButton(GetLabel("Show Extension Exporter")))
                {
                    ShowExtensionExporterEditor();
                }
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;
            }


            EditorGUILayout.LabelField("Celebrate!", m_wrapStyle);
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Draw the extension editor
        /// </summary>
        void DrawExtensionsEditor()
        {
            EditorGUI.indentLevel++;

            if (DrawLinkBody(
                "Gaia eXtensions accelerate and simplify development by integrating quality assets. This tab shows the extensions for the products you've installed. Click here to see more extensions.")
            )
            {
                Application.OpenURL("http://www.procedural-worlds.com/gaia/?section=gaia-extensions");
            }
            GUILayout.Space(5f);

            //And scan if something has changed
            if (m_needsScan)
            {
                m_extensionMgr.ScanForExtensions();
                if (m_extensionMgr.GetInstalledExtensionCount() != 0)
                {
                    m_needsScan = false;
                }
            }

            int methodIdx = 0;
            string cmdName;
            string currFoldoutName = "";
            string prevFoldoutName = ""; 
            MethodInfo command;
            string[] cmdBreakOut = new string[0];
            List<GaiaCompatiblePackage> packages;
            List<GaiaCompatiblePublisher> publishers = m_extensionMgr.GetPublishers();

            foreach (GaiaCompatiblePublisher publisher in publishers)
            {
                if (publisher.InstalledPackages() > 0)
                {
                    if (publisher.m_installedFoldedOut = EditorGUILayout.Foldout(publisher.m_installedFoldedOut, GetLabel(publisher.m_publisherName), true))
                    {
                        EditorGUI.indentLevel++;

                        packages = publisher.GetPackages();
                        foreach (GaiaCompatiblePackage package in packages)
                        {
                            if (package.m_isInstalled)
                            {
                                if (package.m_installedFoldedOut = EditorGUILayout.Foldout(package.m_installedFoldedOut, GetLabel(package.m_packageName), true))
                                {
                                    EditorGUI.indentLevel++;

                                    //Now loop thru and process
                                    while (methodIdx < package.m_methods.Count)
                                    {
                                        command = package.m_methods[methodIdx];
                                        cmdBreakOut = command.Name.Split('_');

                                        //Ignore if we are not a valid thing
                                        if ((cmdBreakOut.GetLength(0) != 2 && cmdBreakOut.GetLength(0) != 3) || cmdBreakOut[0] != "GX")
                                        {
                                            methodIdx++;
                                            continue;
                                        }

                                        //Get foldout and command name
                                        if (cmdBreakOut.GetLength(0) == 2)
                                        {
                                            currFoldoutName = "";
                                        }
                                        else
                                        {
                                            currFoldoutName = Regex.Replace(cmdBreakOut[1], "(\\B[A-Z])", " $1");
                                        }
                                        cmdName = Regex.Replace(cmdBreakOut[cmdBreakOut.GetLength(0) - 1], "(\\B[A-Z])", " $1");

                                        if (currFoldoutName == "")
                                        {
                                            methodIdx++;
                                            if (DisplayButton(GetLabel(cmdName)))
                                            {
                                                command.Invoke(null, null);
                                            }
                                        }
                                        else
                                        {
                                            prevFoldoutName = currFoldoutName;

                                            //Make sure we have it in our dictionary
                                            if (!package.m_methodGroupFoldouts.ContainsKey(currFoldoutName))
                                            {
                                                package.m_methodGroupFoldouts.Add(currFoldoutName, false);
                                            }

                                            if (package.m_methodGroupFoldouts[currFoldoutName] = EditorGUILayout.Foldout(package.m_methodGroupFoldouts[currFoldoutName], GetLabel(currFoldoutName), true))
                                            {
                                                EditorGUI.indentLevel++;

                                                while (methodIdx < package.m_methods.Count && currFoldoutName == prevFoldoutName)
                                                {
                                                    command = package.m_methods[methodIdx];
                                                    cmdBreakOut = command.Name.Split('_');

                                                    //Drop out if we are not a valid thing
                                                    if ((cmdBreakOut.GetLength(0) != 2 && cmdBreakOut.GetLength(0) != 3) || cmdBreakOut[0] != "GX")
                                                    {
                                                        methodIdx++;
                                                        continue;
                                                    }

                                                    //Get foldout and command name
                                                    if (cmdBreakOut.GetLength(0) == 2)
                                                    {
                                                        currFoldoutName = "";
                                                    }
                                                    else
                                                    {
                                                        currFoldoutName = Regex.Replace(cmdBreakOut[1], "(\\B[A-Z])", " $1");
                                                    }
                                                    cmdName = Regex.Replace(cmdBreakOut[cmdBreakOut.GetLength(0) - 1], "(\\B[A-Z])", " $1");

                                                    if (currFoldoutName != prevFoldoutName)
                                                    {
                                                        continue;
                                                    }

                                                    if (DisplayButton(GetLabel(cmdName)))
                                                    {
                                                        command.Invoke(null, null);
                                                    }

                                                    methodIdx++;
                                                }

                                                EditorGUI.indentLevel--;
                                            }
                                            else
                                            {
                                                while (methodIdx < package.m_methods.Count && currFoldoutName == prevFoldoutName)
                                                {
                                                    command = package.m_methods[methodIdx];
                                                    cmdBreakOut = command.Name.Split('_');

                                                    //Drop out if we are not a valid thing
                                                    if ((cmdBreakOut.GetLength(0) != 2 && cmdBreakOut.GetLength(0) != 3) || cmdBreakOut[0] != "GX")
                                                    {
                                                        methodIdx++;
                                                        continue;
                                                    }

                                                    //Get foldout and command name
                                                    if (cmdBreakOut.GetLength(0) == 2)
                                                    {
                                                        currFoldoutName = "";
                                                    }
                                                    else
                                                    {
                                                        currFoldoutName = Regex.Replace(cmdBreakOut[1], "(\\B[A-Z])", " $1");
                                                    }
                                                    cmdName = Regex.Replace(cmdBreakOut[cmdBreakOut.GetLength(0) - 1], "(\\B[A-Z])", " $1");

                                                    if (currFoldoutName != prevFoldoutName)
                                                    {
                                                        continue;
                                                    }

                                                    methodIdx++;
                                                }
                                            }
                                        }
                                    }

                                    /*
                                    foreach (MethodInfo command in package.m_methods)
                                    {
                                        cmdBreakOut = command.Name.Split('_');

                                        if ((cmdBreakOut.GetLength(0) == 2 || cmdBreakOut.GetLength(0) == 3) && cmdBreakOut[0] == "GX")
                                        {
                                            if (cmdBreakOut.GetLength(0) == 2)
                                            {
                                                currFoldoutName = "";
                                            }
                                            else
                                            {
                                                currFoldoutName = cmdBreakOut[1];
                                                Debug.Log(currFoldoutName);
                                            }

                                            cmdName = Regex.Replace(cmdBreakOut[cmdBreakOut.GetLength(0) - 1], "(\\B[A-Z])", " $1");
                                            if (DisplayButton(GetLabel(cmdName)))
                                            {
                                                command.Invoke(null, null);
                                            }
                                        }
                                    }
                                        */

                                    EditorGUI.indentLevel--;
                                }
                            }
                        }

                        EditorGUI.indentLevel--;
                    }
                }
            }

            EditorGUI.indentLevel--;
        }


        /// <summary>
        /// Draw the show more editor
        /// </summary>
        void DrawMoreEditor()
        {
//            GUILayout.Space(2);
//            EditorGUILayout.BeginHorizontal();
//            m_managerMoreMode = (GaiaConstants.ManagerEditorNewsMode)GUILayout.Toolbar((int)m_managerMoreMode, m_menuStrings2);
//            EditorGUILayout.EndHorizontal();
//            GUILayout.Space(2);

            if (m_managerMoreMode == GaiaConstants.ManagerEditorNewsMode.MoreOnGaia)
            {
                DrawTutorialsAndSupport();
            }
            else
            {
                DrawMoreOnProceduralWorldsEditor();
            }
        }

        void DrawTutorialsAndSupport()
        {
            EditorGUI.indentLevel++;
            DrawBodyText("Review the QuickStart guide and other product documentation in the Gaia / Documentation directory.");
            GUILayout.Space(5f);

            if (m_settings.m_hideHeroMessage)
            {
                if (DrawLinkHeaderText(GetLabel(m_settings.m_latestNewsTitle)))
                {
                    Application.OpenURL(m_settings.m_latestNewsUrl);
                }

                DrawBodyText(m_settings.m_latestNewsBody);
                GUILayout.Space(5f);
            }

            if (DrawLinkHeaderText(GetLabel("Video Tutorials")))
            {
                Application.OpenURL("http://www.procedural-worlds.com/gaia/?section=tutorials");
            }
            DrawBodyText("With over 45 video tutorials we cover everything you need to become an expert.");
            GUILayout.Space(5f);

            if (DrawLinkHeaderText("Join Our Community"))
            {
                Application.OpenURL("https://discord.gg/rtKn8rw");
            }
            DrawBodyText("Whether you need an answer now or feel like a chat our friendly discord community is a great place to learn!");
            GUILayout.Space(5f);

            if (DrawLinkHeaderText(GetLabel("Ticketed Support")))
            {
                Application.OpenURL("https://proceduralworlds.freshdesk.com/support/home");
            }
            DrawBodyText("Don't let your question get lost in the noise. All ticketed requests are answered, and usually within 48 hours.");
            GUILayout.Space(5f);

            if (DrawLinkHeaderText(GetLabel("Help us Grow - Rate & Review!")))
            {
                Application.OpenURL("https://www.assetstore.unity3d.com/#!/content/42618?aid=1101lSqC");
            }
            DrawBodyText("Quality products are a huge investment to create & support. Please take a moment to show your appreciation by leaving a rating & review.");
            GUILayout.Space(5f);

            if (m_settings.m_hideHeroMessage)
            {
                if (DrawLinkHeaderText(GetLabel("Show Hero Message")))
                {
                    m_settings.m_hideHeroMessage = false;
                    EditorUtility.SetDirty(m_settings);
                }
                DrawBodyText("Show latest news and hero messages in Gaia.");
                GUILayout.Space(5f);
            }
            EditorGUI.indentLevel--;
        }

        void DrawMoreOnProceduralWorldsEditor()
        {
            EditorGUI.indentLevel++;
            DrawBodyText("Super charge your development with our amazing partners & extensions.");
            GUILayout.Space(5f);

            if (m_settings.m_hideHeroMessage)
            {
                if (DrawLinkHeaderText(GetLabel(m_settings.m_latestNewsTitle)))
                {
                    Application.OpenURL(m_settings.m_latestNewsUrl);
                }

                DrawBodyText(m_settings.m_latestNewsBody);
                GUILayout.Space(5f);
            }

            if (DrawLinkHeaderText("Our Partners"))
            {
                Application.OpenURL("http://www.procedural-worlds.com/partners/");
            }
            DrawBodyText("The content included with Gaia is an awesome starting point for your game, but that's just the tip of the iceberg. Learn more about how these talented publishers can help you to create amazing environments in Unity.");
            GUILayout.Space(5f);

            if (DrawLinkHeaderText(GetLabel("Gaia eXtensions (GX)")))
            {
                Application.OpenURL("http://www.procedural-worlds.com/gaia/?section=gaia-extensions");
            }
            DrawBodyText("Gaia eXtensions accelerate and simplify your development by automating asset setup in your scene. Check out the quality assets we have integrated for you!");
            GUILayout.Space(5f);

            if (DrawLinkHeaderText(GetLabel("Help Us to Grow - Spread The Word!")))
            {
                Application.OpenURL("https://www.facebook.com/proceduralworlds/");
            }
            DrawBodyText("Get regular news updates and help us to grow by liking and sharing our Facebook page!");
            GUILayout.Space(5f);

            if (m_settings.m_hideHeroMessage)
            {
                if (DrawLinkHeaderText(GetLabel("Show Hero Message")))
                {
                    m_settings.m_hideHeroMessage = false;
                    EditorUtility.SetDirty(m_settings);
                }
                DrawBodyText("Show latest news and hero messages in Gaia.");
                GUILayout.Space(5f);
            }
            EditorGUI.indentLevel--;


        }

        /// <summary>
        /// Create the terrain
        /// </summary>
        void CreateTerrain()
        {
            //Only do this if we have < 1 terrain
            int actualTerrainCount = Gaia.TerrainHelper.GetActiveTerrainCount();
            if (actualTerrainCount != 0)
            {
                EditorUtility.DisplayDialog("OOPS!", string.Format("You currently have {0} active terrains in your scene, but to use this feature you need {1}. Please add or remove terrains.", actualTerrainCount, 0), "OK");
            }
            else
            {
                //Disable automatic light baking - this kills perf on most systems
                Lightmapping.giWorkflowMode = Lightmapping.GIWorkflowMode.OnDemand;

                //Create the terrain
                m_settings.m_currentDefaults.CreateTerrain(m_settings.m_currentResources);
            }
        }

        /// <summary>
        /// Create / show the session manager
        /// </summary>
        GameObject ShowSessionManager(bool pickupExistingTerrain = false)
        {
            GameObject mgrObj = GaiaSessionManager.GetSessionManager(pickupExistingTerrain).gameObject;
            Selection.activeGameObject = mgrObj;
            return mgrObj;
        }

        /// <summary>
        /// Select or create a stamper
        /// </summary>
        void ShowStamper()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return;
            }

            //Make sure we have a session manager
            //m_sessionManager = m_resources.CreateOrFindSessionManager().GetComponent<GaiaSessionManager>();

            //Make sure we have gaia object
            GameObject gaiaObj = m_settings.m_currentResources.CreateOrFindGaia();

            //Create or find the stamper
            GameObject stamperObj = GameObject.Find("Stamper");
            if (stamperObj == null)
            {
                stamperObj = new GameObject("Stamper");
                stamperObj.transform.parent = gaiaObj.transform;
                Stamper stamper = stamperObj.AddComponent<Stamper>();
                stamper.m_resources = m_settings.m_currentResources;
                stamper.FitToTerrain();
                stamperObj.transform.position = new Vector3(stamper.m_x, stamper.m_y, stamper.m_z);
            }
            Selection.activeGameObject = stamperObj;
        }

        /// <summary>
        /// Select or create a scanner
        /// </summary>
        GameObject CreateScanner()
        {
            GameObject gaiaObj = m_settings.m_currentResources.CreateOrFindGaia();
            GameObject scannerObj = GameObject.Find("Scanner");
            if (scannerObj == null)
            {
                scannerObj = new GameObject("Scanner");
                scannerObj.transform.parent = gaiaObj.transform;
                scannerObj.transform.position = Gaia.TerrainHelper.GetActiveTerrainCenter(false);
                Scanner scanner = scannerObj.AddComponent<Scanner>();

                //Load the material to draw it
                string matPath = GetAssetPath("GaiaScannerMaterial");
                if (!string.IsNullOrEmpty(matPath))
                {
                    scanner.m_previewMaterial = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                }
            }
            return scannerObj;
        }

        /// <summary>
        /// Create or select the existing visualiser
        /// </summary>
        /// <returns>New or exsiting visualiser - or null if no terrain</returns>
        GameObject ShowVisualiser()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return null;
            }

            GameObject gaiaObj = m_settings.m_currentResources.CreateOrFindGaia();
            GameObject visualiserObj = GameObject.Find("Visualiser");
            if (visualiserObj == null)
            {
                visualiserObj = new GameObject("Visualiser");
                visualiserObj.AddComponent<ResourceVisualiser>();
                visualiserObj.transform.parent = gaiaObj.transform;

                //Center it on the terrain
                visualiserObj.transform.position = Gaia.TerrainHelper.GetActiveTerrainCenter();
            }
            ResourceVisualiser visualiser = visualiserObj.GetComponent<ResourceVisualiser>();
            visualiser.m_resources = m_settings.m_currentResources;
            return visualiserObj;
        }

        /// <summary>
        /// Show a normal exporter
        /// </summary>
        void ShowNormalMaskExporter()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return;
            }

            var export = EditorWindow.GetWindow<GaiaNormalExporterEditor>(false, "Normalmap Exporter");
            export.Show();
        }

        /// <summary>
        /// Show the terrain height adjuster
        /// </summary>
        void ShowTerrainHeightAdjuster()
        {
            var export = EditorWindow.GetWindow<GaiaTerrainHeightAdjuster>(false, "Height Adjuster");
            export.Show();
        }

        /// <summary>
        /// Show the terrain explorer helper
        /// </summary>
        void ShowTerrainUtilties()
        {
            var export = EditorWindow.GetWindow<GaiaTerrainExplorerEditor>(false, "Terrain Utilities");
            export.Show();
        }

        /// <summary>
        /// Show a texture mask exporter
        /// </summary>
        void ShowTexureMaskExporter()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return;
            }

            var export = EditorWindow.GetWindow<GaiaMaskExporterEditor>(false, "Splatmap Exporter");
            export.Show();
        }

        /// <summary>
        /// Show a grass mask exporter
        /// </summary>
        void ShowGrassMaskExporter()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return;
            }

            var export = EditorWindow.GetWindow<GaiaGrassMaskExporterEditor>(false, "Grassmask Exporter");
            export.Show();
        }

        /// <summary>
        /// Show flowmap exporter
        /// </summary>
        void ShowFlowMapMaskExporter()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return;
            }

            var export = EditorWindow.GetWindow<GaiaWaterflowMapEditor>(false, "Flowmap Exporter");
            export.Show();
        }

        /// <summary>
        /// Show a terrain obj exporter
        /// </summary>
        void ShowTerrainObjExporter()
        {
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return;
            }

            var export = EditorWindow.GetWindow<ExportTerrain>(false, "Export Terrain");
            export.Show();
        }

        /// <summary>
        /// Export the world as a PNG heightmap
        /// </summary>
        void ExportWorldAsHeightmapPNG()
        {
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return;
            }

            GaiaWorldManager mgr = new GaiaWorldManager(Terrain.activeTerrains);
            if (mgr.TileCount > 0)
            {
                string path = "Assets/GaiaMasks/";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = Path.Combine(path, Gaia.Utils.FixFileName(string.Format("Terrain-Heightmap-{0:yyyyMMdd-HHmmss}", DateTime.Now)));
                mgr.ExportWorldAsPng(path);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Export complete", " Your heightmap has been saved to : " + path, "OK");
            }
        }

        /// <summary>
        /// Export the shore mask as a png file
        /// </summary>
        void ExportShoremaskAsPNG()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return;
            }

            var export = EditorWindow.GetWindow<ShorelineMaskerEditor>(false, "Export Shore");
            export.m_seaLevel = m_settings.m_currentResources.m_seaLevel;
            export.Show();
        }

        /// <summary>
        /// Show the extension exporter
        /// </summary>
        void ShowExtensionExporterEditor()
        {
            var export = EditorWindow.GetWindow<GaiaExtensionExporterEditor>(false, "Export GX");
            export.Show();
        }

        /// <summary>
        /// Display an error if there is not exactly one terrain
        /// </summary>
        /// <param name="requiredTerrainCount">The amount required</param>
        /// <param name="feature">The feature name</param>
        /// <returns>True if an error, false otherwise</returns>
        private bool DisplayErrorIfInvalidTerrainCount(int requiredTerrainCount, string feature = "")
        {
            int actualTerrainCount = Gaia.TerrainHelper.GetActiveTerrainCount();
            if (actualTerrainCount != requiredTerrainCount)
            {
                if (string.IsNullOrEmpty(feature))
                {
                    if (actualTerrainCount < requiredTerrainCount)
                    {
                        EditorUtility.DisplayDialog("OOPS!", string.Format("You currently have {0} active terrains in your scene, but to use this feature you need {1}. Please create a terrain!", actualTerrainCount, requiredTerrainCount), "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("OOPS!", string.Format("You currently have {0} active terrains in your scene, but to use this feature you need {1}. Please remove terrain!", actualTerrainCount, requiredTerrainCount), "OK");
                    }
                }
                else
                {
                    if (actualTerrainCount < requiredTerrainCount)
                    {
                        EditorUtility.DisplayDialog("OOPS!", string.Format("You currently have {0} active terrains in your scene, but to use {2} you need {1}. Please create terrain!", actualTerrainCount, requiredTerrainCount, feature), "OK");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("OOPS!", string.Format("You currently have {0} active terrains in your scene, but to use {2} you need {1}. Please remove terrain!", actualTerrainCount, requiredTerrainCount, feature), "OK");
                    }
                }
                
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the range from the terrain
        /// </summary>
        /// <returns></returns>
        private float GetRangeFromTerrain()
        {
            float range = (m_settings.m_currentDefaults.m_terrainSize / 2) * m_settings.m_currentDefaults.m_tilesX;
            Terrain t = Gaia.TerrainHelper.GetActiveTerrain();
            if (t != null)
            {
                range = (Mathf.Max(t.terrainData.size.x, t.terrainData.size.z) / 2f) * m_settings.m_currentDefaults.m_tilesX;
            }
            return range;
        }

        /// <summary>
        /// Create a texture spawner
        /// </summary>
        /// <returns>Spawner</returns>
        GameObject CreateTextureSpawner()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return null;
            }

            return m_settings.m_currentResources.CreateCoverageTextureSpawner(GetRangeFromTerrain(), Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / (float)m_settings.m_currentDefaults.m_controlTextureResolution, 0.2f, 100f));
        }

        /// <summary>
        /// Create a detail spawner
        /// </summary>
        /// <returns>Spawner</returns>
        GameObject CreateDetailSpawner()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return null;
            }

            return m_settings.m_currentResources.CreateCoverageDetailSpawner(GetRangeFromTerrain(), Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / (float)m_settings.m_currentDefaults.m_detailResolution, 0.2f, 100f));
        }

        /// <summary>
        /// Create a clustered detail spawner
        /// </summary>
        /// <returns>Spawner</returns>
        GameObject CreateClusteredDetailSpawner()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return null;
            }

            return m_settings.m_currentResources.CreateClusteredDetailSpawner(GetRangeFromTerrain(), Mathf.Clamp(m_settings.m_currentDefaults.m_terrainSize / (float)m_settings.m_currentDefaults.m_detailResolution, 0.2f, 100f));
        }

        /// <summary>
        /// Create a tree spawner
        /// </summary>
        /// <returns>Spawner</returns>
        GameObject CreateClusteredTreeSpawnerFromTerrainTrees()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return null;
            }
            
            return m_settings.m_currentResources.CreateClusteredTreeSpawner(GetRangeFromTerrain());
        }

        /// <summary>
        /// Create a tree spawner from game objecxts
        /// </summary>
        /// <returns>Spawner</returns>
        GameObject CreateClusteredTreeSpawnerFromGameObjects()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return null;
            }

            return m_settings.m_currentGameObjectResources.CreateClusteredGameObjectSpawnerForTrees(GetRangeFromTerrain());
        }

        /// <summary>
        /// Create a tree spawner
        /// </summary>
        /// <returns>Spawner</returns>
        GameObject CreateCoverageTreeSpawner()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return null;
            }

            return m_settings.m_currentResources.CreateCoverageTreeSpawner(GetRangeFromTerrain());
        }

        /// <summary>
        /// Create a tree spawner
        /// </summary>
        /// <returns>Spawner</returns>
        GameObject CreateCoverageTreeSpawnerFromGameObjects()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return null;
            }

            return m_settings.m_currentGameObjectResources.CreateCoverageGameObjectSpawnerForTrees(GetRangeFromTerrain());
        }

        /// <summary>
        /// Create a game object spawner
        /// </summary>
        /// <returns>Spawner</returns>
        GameObject CreateCoverageGameObjectSpawner()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return null;
            }

            return m_settings.m_currentGameObjectResources.CreateCoverageGameObjectSpawner(GetRangeFromTerrain());
        }

        /// <summary>
        /// Create a game object spawner
        /// </summary>
        /// <returns>Spawner</returns>
        GameObject CreateClusteredGameObjectSpawner()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return null;
            }

            return m_settings.m_currentGameObjectResources.CreateClusteredGameObjectSpawner(GetRangeFromTerrain());
        }

        /// <summary>
        /// Create a player
        /// </summary>
        GameObject CreatePlayer()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return null;
            }

            string playerPrefabName = m_settings.m_currentPlayerPrefabName;
            if (string.IsNullOrEmpty(playerPrefabName))
            {
                playerPrefabName = "FPSController";
            }

            GameObject playerObj = null;
            if (playerPrefabName == "Flycam")
            {
                playerObj = GameObject.Find("Main Camera");
            }
            else
            {
                playerObj = GameObject.Find("Player");
            }

            //Get the centre of world at game height plus a bit
            Vector3 location = Gaia.TerrainHelper.GetActiveTerrainCenter(true);

            //If we have a player then move it
            if (playerObj != null)
            {
                if (playerPrefabName == "Flycam")
                {
                    location.y += 1.8f;

                    FreeCamera freeCam = playerObj.GetComponent<FreeCamera>();
                    if (freeCam == null)
                    {
                        playerObj.AddComponent<FreeCamera>();
                    }
                }
                else
                {
                    location.y += 1f;
                }
                playerObj.transform.position = location;
            }
            //Else create it
            else 
            {
                if (playerPrefabName == "Flycam")
                {
                    playerObj = new GameObject();
                    playerObj.name = "Main Camera";
                    playerObj.tag = "MainCamera";
                    playerObj.AddComponent<FlareLayer>();
                    #if !UNITY_2017_1_OR_NEWER
                    playerObj.AddComponent<GUILayer>();
                    #endif
                    playerObj.AddComponent<AudioListener>();
                    playerObj.AddComponent<FreeCamera>();
                }
                else
                {
                    GameObject fps = Utils.GetAssetPrefab(playerPrefabName);
                    if (fps != null)
                    {
                        playerObj = Instantiate(fps, location, Quaternion.identity) as GameObject;
                        playerObj.name = "Player";
                        playerObj.tag = "Player";

                        if (m_settings.m_currentController == GaiaConstants.EnvironmentControllerType.FirstPerson)
                        {
                            //Find and raise the camera
                            Transform cameraObj = playerObj.transform.Find("FirstPersonCharacter");
                            if (cameraObj != null)
                            {
                                cameraObj.localPosition = new Vector3(cameraObj.localPosition.x, 1.6f, cameraObj.localPosition.z);
                            }
                            //Ok - we have added a new camera into the scene - lets disable the existing one
                            GameObject mainCameraObj = GameObject.Find("Main Camera");
                            if (mainCameraObj != null)
                            {
                                mainCameraObj.SetActive(false);
                            }
                        }
                        else
                        {
                            //Set the main camera up
                            GameObject mainCameraObj = GameObject.Find("Main Camera");
                            if (mainCameraObj != null)
                            {
                                mainCameraObj.SetActive(true);
                            }
                            else
                            {
                                mainCameraObj = new GameObject();
                                mainCameraObj.name = "Main Camera";
                                mainCameraObj.AddComponent<Camera>();
                                mainCameraObj.tag = "MainCamera";
                                mainCameraObj.AddComponent<FlareLayer>();
                                #if !UNITY_2017_1_OR_NEWER
                                mainCameraObj.AddComponent<GUILayer>();
                                #endif
                                mainCameraObj.AddComponent<AudioListener>();
                            }

                            //Add and configure the camera controller
                            CameraController cc = mainCameraObj.GetComponent<CameraController>();
                            if (cc == null)
                            {
                                cc = mainCameraObj.AddComponent<CameraController>();
                            }
                            else
                            {
                                cc.enabled = true;
                            }
                            cc.target = playerObj;
                        }
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("OOPS!", "Unable to locate the character prefab!! Please Import Unity Standard Character Assets and try again.", "OK");
                    }
                }
            }
            return playerObj;
        }

        /// <summary>
        /// Create a scene exporter object
        /// </summary>
        /*
        GameObject ShowSceneExporter()
        {
            GameObject exporterObj = GameObject.Find("Exporter");
            if (exporterObj == null)
            {
                exporterObj = new GameObject("Exporter");
                exporterObj.transform.position = Gaia.TerrainHelper.GetActiveTerrainCenter(false);
                GaiaExporter exporter = exporterObj.AddComponent<GaiaExporter>();
                GameObject gaiaObj = GameObject.Find("Gaia");
                if (gaiaObj != null)
                {
                    exporterObj.transform.parent = gaiaObj.transform;
                    exporter.m_rootObject = gaiaObj;
                }
                exporter.m_defaults = m_defaults;
                exporter.m_resources = m_resources;
                exporter.IngestGaiaSetup();
            }
            return exporterObj;
        }
         */

        /// <summary>
        /// Create a wind zone
        /// </summary>
        GameObject CreateWindZone()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return null;
            }

            GameObject windZoneObj = GameObject.Find("Wind Zone");
            if (windZoneObj == null)
            {
                windZoneObj = new GameObject("Wind Zone");
                windZoneObj.transform.position = Gaia.TerrainHelper.GetActiveTerrainCenter(false);
                WindZone windZone = windZoneObj.AddComponent<WindZone>();
                windZone.windMain = 0.2f;
                windZone.windTurbulence = 0.2f;
                windZone.windPulseMagnitude = 0.2f;
                windZone.windPulseFrequency = 0.05f;
                GameObject gaiaObj = GameObject.Find("Gaia Environment");
                if (gaiaObj == null)
                {
                    gaiaObj = new GameObject("Gaia Environment");
                }
                windZoneObj.transform.parent = gaiaObj.transform;
            }
            return windZoneObj;
        }

        /// <summary>
        /// Create water
        /// </summary>
        GameObject CreateWater()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return null;
            }

            GameObject waterObj = GameObject.Find("Water");
            if (waterObj == null)
            {
                string waterPrefabName = m_settings.m_currentWaterPrefabName;
                if (string.IsNullOrEmpty(waterPrefabName))
                {
                    waterPrefabName = "Water4Advanced";
                }

                GameObject waterPrefab = Utils.GetAssetPrefab(waterPrefabName);
                if (waterPrefab != null)
                {
                    GaiaSceneInfo sceneInfo = GaiaSceneInfo.GetSceneInfo();
                    Terrain terrain = Gaia.TerrainHelper.GetActiveTerrain();
                    Vector3 location = sceneInfo.m_centrePointOnTerrain;
                    location.y = sceneInfo.m_seaLevel;
                    waterObj = Instantiate(waterPrefab, location, Quaternion.identity) as GameObject;
                    if (terrain != null)
                    {
                        if (waterPrefabName == "WaterBasicDaytime")
                        {
                            waterObj.transform.localScale = new Vector3(
                                (Mathf.Max(sceneInfo.m_sceneBounds.size.x, sceneInfo.m_sceneBounds.size.z) * Mathf.Max(m_settings.m_currentDefaults.m_tilesX, m_settings.m_currentDefaults.m_tilesZ)) * 2,
                                0f,
                                (Mathf.Max(sceneInfo.m_sceneBounds.size.x, sceneInfo.m_sceneBounds.size.z) * Mathf.Max(m_settings.m_currentDefaults.m_tilesX, m_settings.m_currentDefaults.m_tilesZ)) * 2);
                        }
                        else
                        {
                            waterObj.transform.localScale = new Vector3(
                                (Mathf.Max(sceneInfo.m_sceneBounds.size.x, sceneInfo.m_sceneBounds.size.z) * Mathf.Max(m_settings.m_currentDefaults.m_tilesX, m_settings.m_currentDefaults.m_tilesZ)) / 100 + 25,
                                0f,
                                (Mathf.Max(sceneInfo.m_sceneBounds.size.x, sceneInfo.m_sceneBounds.size.z) * Mathf.Max(m_settings.m_currentDefaults.m_tilesX, m_settings.m_currentDefaults.m_tilesZ)) / 100 + 25);
                        }
                    }
                    else
                    {
                        waterObj.transform.localScale = new Vector3(
                            (m_settings.m_currentDefaults.m_terrainSize * Mathf.Max(m_settings.m_currentDefaults.m_tilesX, m_settings.m_currentDefaults.m_tilesZ)) / 100 + 25,
                            0f,
                            (m_settings.m_currentDefaults.m_terrainSize * Mathf.Max(m_settings.m_currentDefaults.m_tilesX, m_settings.m_currentDefaults.m_tilesZ)) / 100 + 25);
                    }
                    waterObj.name = "Water";
                }
                else
                {
                    EditorUtility.DisplayDialog("OOPS!", "Unable to locate the water prefab!! Please Import Unity Standard Environment Assets and try again.", "OK");
                }
                GameObject gaiaObj = GameObject.Find("Gaia Environment");
                if (gaiaObj == null)
                {
                    gaiaObj = new GameObject("Gaia Environment");
                }
                if (waterObj != null)
                {
                    waterObj.transform.parent = gaiaObj.transform;
                }
            }
            return waterObj;
        }

        /// <summary>
        /// Create and return a screen shotter object
        /// </summary>
        /// <returns></returns>
        GameObject CreateScreenShotter()
        {
            //Only do this if we have 1 terrain
            if (DisplayErrorIfInvalidTerrainCount(1))
            {
                return null;
            }
            GameObject shotterObj = GameObject.Find("Screen Shotter");
            if (shotterObj == null)
            {
                shotterObj = new GameObject("Screen Shotter");
                Gaia.ScreenShotter shotter = shotterObj.AddComponent<Gaia.ScreenShotter>();
                shotter.m_watermark = Gaia.Utils.GetAsset("Made With Gaia Watermark.png", typeof(Texture2D)) as Texture2D;

                GameObject gaiaObj = GameObject.Find("Gaia Environment");
                if (gaiaObj == null)
                {
                    gaiaObj = new GameObject("Gaia Environment");
                }
                shotterObj.transform.parent = gaiaObj.transform;
                shotterObj.transform.position = Gaia.TerrainHelper.GetActiveTerrainCenter(false);
            }
            else
            {
                Debug.Log("You already have a Screen Shotter in your scene!");
            }
            return shotterObj;
        }

        /// <summary>
        /// Get a clamped size value
        /// </summary>
        /// <param name="newSize"></param>
        /// <returns></returns>
        float GetClampedSize(float newSize)
        {
            return Mathf.Clamp(newSize, 32f, m_settings.m_currentDefaults.m_size);
        }

        /// <summary>
        /// Display a button that takes editor indentation into account
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        bool DisplayButton(GUIContent content)
        {
            TextAnchor oldalignment = GUI.skin.button.alignment;
            GUI.skin.button.alignment = TextAnchor.MiddleLeft;
            Rect btnR = EditorGUILayout.BeginHorizontal();
            btnR.xMin += (EditorGUI.indentLevel * 18f);
            btnR.height += 20f;
            btnR.width -= 4f;
            bool result = GUI.Button(btnR, content);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(22);
            GUI.skin.button.alignment = oldalignment;
            return result;
        }

        /// <summary>
        /// Get a content label - look the tooltip up if possible
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        GUIContent GetLabel(string name)
        {
            string tooltip = "";
            if (m_tooltips.TryGetValue(name, out tooltip))
            {
                return new GUIContent(name, tooltip);
            }
            else
            {
                return new GUIContent(name);
            }
        }

        /// <summary>
        /// Get the asset path of the first thing that matches the name
        /// </summary>
        /// <param name="name">Name to search for</param>
        /// <returns></returns>
        string GetAssetPath(string name)
        {
            string[] assets = AssetDatabase.FindAssets(name, null);
            if (assets.Length > 0)
            {
                return AssetDatabase.GUIDToAssetPath(assets[0]);
            }
            return null;
        }

        /// <summary>
        /// Display image
        /// </summary>
        /// <param name="text"></param>
        void DrawImage(Texture2D image, float width, float height)
        {
            //GUILayout.Label(image, GUILayout.Height(height));
            GUILayout.Label(image, GUILayout.Width(width), GUILayout.Height(height));
        }

        /// <summary>
        /// Display header text in title style
        /// </summary>
        /// <param name="text"></param>
        void DrawTitleText(string text)
        {
            GUILayout.Label(text, m_titleStyle);
        }

        /// <summary>
        /// Display header text in header style
        /// </summary>
        /// <param name="text"></param>
        void DrawHeaderText(string text)
        {
            GUILayout.Label(text, m_headingStyle);
        }

        /// <summary>
        /// Handy wrapper
        /// </summary>
        /// <param name="text"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        bool DrawLinkHeaderText(string text, params GUILayoutOption[] options)
        {
            return DrawLinkHeaderText(GetLabel(text), options);
        }

        /// <summary>
        /// Display clickable header text in header style
        /// </summary>
        /// <param name="text"></param>
        bool DrawLinkHeaderText(GUIContent text, params GUILayoutOption[] options)
        {
            var position = GUILayoutUtility.GetRect(text, m_headingStyle, options);
            Handles.BeginGUI();
            Handles.color = m_headingStyle.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            return GUI.Button(position, text, m_headingStyle);
        }

        /// <summary>
        /// Display body text in body style
        /// </summary>
        /// <param name="bodyText"></param>
        void DrawBodyText(string bodyText)
        {
            GUILayout.Label(bodyText, m_bodyStyle);
        }

        /// <summary>
        /// Display clickable header text in header style
        /// </summary>
        /// <param name="text"></param>
        bool DrawLinkBody(string text)
        {
            var label = GetLabel(text);
            return DrawLinkBody(label);
        }

        /// <summary>
        /// Display clickable header text in header style
        /// </summary>
        /// <param name="text"></param>
        bool DrawLinkBody(GUIContent text, params GUILayoutOption[] options)
        {
            var position = GUILayoutUtility.GetRect(text, m_bodyStyle, options);
//            Handles.BeginGUI();
//            Handles.color = m_headingStyle.normal.textColor;
//            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
//            Handles.color = Color.white;
//            Handles.EndGUI();
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            return GUI.Button(position, text, m_bodyStyle);
        }

        /// <summary>
        /// Handy wrapper
        /// </summary>
        /// <param name="label"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        bool DrawLinkLabel(string label, params GUILayoutOption[] options)
        {
            return DrawLinkLabel(GetLabel(label), options);
        }

        /// <summary>
        /// Draw link label
        /// </summary>
        /// <param name="label">Label</param>
        /// <param name="options">Options</param>
        /// <returns></returns>
        bool DrawLinkLabel(GUIContent label, params GUILayoutOption[] options)
        {
            var position = GUILayoutUtility.GetRect(label, m_linkStyle, options);
            Handles.BeginGUI();
            Handles.color = m_linkStyle.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin, position.yMax), new Vector3(position.xMax, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();
            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);
            return GUI.Button(position, label, m_linkStyle);
        }

        /// <summary>
        /// Get the latest news from the web site at most once every 24 hours
        /// </summary>
        /// <returns></returns>
        IEnumerator GetNewsUpdate()
        {
            TimeSpan elapsed = new TimeSpan(DateTime.Now.Ticks - m_settings.m_lastWebUpdate);
            if (elapsed.TotalHours < 24.0)
            {
                StopEditorUpdates();
            }
            else
            {
                using (WWW www = new WWW("http://www.procedural-worlds.com/gaiajson.php?gv=gaia-" + GaiaConstants.GaiaMajorVersion + "." + GaiaConstants.GaiaMinorVersion))
                {
                    while (!www.isDone)
                    {
                        yield return www;
                    }

                    if (!string.IsNullOrEmpty(www.error))
                    {
                        //Debug.Log(www.error);
                    }
                    else
                    {
                        try
                        {
                            string result = www.text;
                            int first = result.IndexOf("####");
                            if (first > 0)
                            {
                                result = result.Substring(first + 10);
                                first = result.IndexOf("####");
                                if (first > 0)
                                {
                                    result = result.Substring(0, first);
                                    result = result.Replace("<br />", "");
                                    result = result.Replace("&#8221;", "\"");
                                    result = result.Replace("&#8220;", "\"");
                                    var message = JsonUtility.FromJson<GaiaMessages>(result);
                                    m_settings.m_latestNewsTitle = message.title;
                                    m_settings.m_latestNewsBody = message.bodyContent;
                                    m_settings.m_latestNewsUrl = message.url;
                                    m_settings.m_lastWebUpdate = DateTime.Now.Ticks;
                                    EditorUtility.SetDirty(m_settings);
                                }
                            }
                        }
                        catch (Exception)
                        {
                            //Debug.Log(e.Message);
                        }
                    }
                }
            }
            StopEditorUpdates();
        }

        /// <summary>
        /// Start editor updates
        /// </summary>
        public void StartEditorUpdates()
        {
            EditorApplication.update += EditorUpdate;
        }

        //Stop editor updates
        public void StopEditorUpdates()
        {
            EditorApplication.update -= EditorUpdate;
        }

        /// <summary>
        /// This is executed only in the editor - using it to simulate co-routine execution and update execution
        /// </summary>
        void EditorUpdate()
        {
            if (m_updateCoroutine == null)
            {
                StopEditorUpdates();
            }
            else
            {
                m_updateCoroutine.MoveNext();
            }
        }

        #region GAIA eXtensions GX

        public static List<Type> GetTypesInNamespace(string nameSpace)
        {
            List<Type> gaiaTypes = new List<Type>();

            int assyIdx, typeIdx;
            System.Type[] types;
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            for (assyIdx = 0; assyIdx < assemblies.Length; assyIdx++)
            {
                if (assemblies[assyIdx].FullName.StartsWith("Assembly"))
                {
                    types = assemblies[assyIdx].GetTypes();
                    for (typeIdx = 0; typeIdx < types.Length; typeIdx++ )
                    {
                        if (!string.IsNullOrEmpty(types[typeIdx].Namespace))
                        {
                            if (types[typeIdx].Namespace.StartsWith(nameSpace))
                            {
                                gaiaTypes.Add(types[typeIdx]);
                            }
                        }
                    }
                }
            }
            return gaiaTypes;
        }

        /// <summary>
        /// Return true if image FX have been included
        /// </summary>
        /// <returns></returns>
        public static bool GotImageFX()
        {
            List<Type> types = GetTypesInNamespace("UnityStandardAssets.ImageEffects");
            if (types.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        /// <summary>
        /// The tooltips
        /// </summary>
        static Dictionary<string, string> m_tooltips = new Dictionary<string, string>
        {
            { "Execution Mode", "The way this spawner runs. Design time : At design time only. Runtime Interval : At run time on a timed interval. Runtime Triggered Interval : At run time on a timed interval, and only when the tagged game object is closer than the trigger range from the center of the spawner." },
            { "Controller", "The type of control method that will be set up. " },
            { "Environment", "The type of environment that will be set up. This pre-configures your terrain settings to be better suited for the environment you are targeting. You can modify these setting by modifying the relevant terrain default settings." },
            { "Renderer", "The terrain renderer you are targeting. The 2018x renderers are only relevent when using Unity 2018 and above." },
            { "Terrain Size", "The size of the terrain you are setting up. Please be aware that larger terrain sizes are harder for Unity to render, and will result in slow frame rates. You also need to consider your target environment as well. A mobile or VR device will have problems with large terrains." },
            { "Terrain Defaults", "The default settings that will be used when creating new terrains." },
            { "Terrain Resources", "The texture, detail and tree resources that will be used when creating new terrains." },
            { "GameObject Resources", "The game object resources that will be passed to your GameObject spawners when creating new spawners." },
            { "1. Create Terrain & Show Stamper", "Creates your terrain based on the setting in the panel above. You use the stamper to terraform your terrain." },
            { "2. Create Spawners", "Creates the spawners based on your resources in the panel above. You use spawners to inject these resources into your scene." },
            { "3. Create Player, Water and Screenshotter", "Creates the things you most commonly need in your scene to make it playable." },
            { "3. Create Player, Wind, Water and Screenshotter", "Creates the things you most commonly need in your scene to make it playable." },
            { "Show Session Manager", "The session manager records stamping and spawning operations so that you can recreate your terrain later." },
            { "Create Terrain", "Creates a terrain based on your settings." },
            { "Create Coverage Texture Spawner", "Creates a texture spawner so you can paint your terrain." },
            { "Create Coverage Grass Spawner", "Creates a grass (terrain details) spawner so you can cover your terrain with grass." },
            { "Create Clustered Grass Spawner", "Creates a grass (terrain details) spawner so you can cover your terrain with patches with grass." },
            { "Create Coverage Terrain Tree Spawner", "Creates a terrain tree spawner so you can cover your terrain with trees." },
            { "Create Clustered Terrain Tree Spawner", "Creates a terrain tree spawner so you can cover your terrain with clusters with trees." },
            { "Create Coverage Prefab Tree Spawner", "Creates a tree spawner from prefabs so you can cover your terrain with trees." },
            { "Create Clustered Prefab Tree Spawner", "Creates a tree spawner from prefabs so you can cover your terrain with clusters with trees." },
            { "Create Coverage Prefab Spawner", "Creates a spawner from prefabs so you can cover your terrain with instantiations of those prefabs." },
            { "Create Clustered Prefab Spawner", "Creates a spawner from prefabs so you can cover your terrain with clusters of those prefabs." },
            { "Show Stamper", "Shows a stamper. Use the stamper to terraform your terrain." },
            { "Show Scanner", "Shows the scanner. Use the scanner to create new stamps from textures, world machine .r16 files, IBM 16 bit RAW file, MAC 16 bit RAW files, Terrains, and Meshes (with mesh colliders)." },
            { "Show Visualiser", "Shows the visualiser. Use the visualiser to visualise and configure fitness values for your resources." },
            { "Show Terrain Utilities", "Shows terrain utilities. These are a great way to add additional interest to your terrains." },
            { "Show Splatmap Exporter", "Shows splatmap exporter. Exports your texture splatmaps." },
            { "Show Grass Exporter", "Shows grass exporter. Exports your grass control maps." },
            { "Show Mesh Exporter", "Shows mesh exporter. Exports your terrain as a low poly mesh. Use in conjunction with Base Map Exporter and Normal Map Exporter in Terrain Utilties to create cool mesh features to use in the distance." },
            { "Show Shore Exporter", "Shows shore exporter. Exports a mask of your terrain shoreline." },
            { "Show Extension Exporter", "Shows extension exporter. Use extensions to save resource and spawner configurations for later use via the GX tab." },
        };
    }
}