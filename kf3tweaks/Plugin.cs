using BepInEx;
using UnityEngine;
using MonoMod.RuntimeDetour;
using UnityEngine.Rendering;
using System.Reflection;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

namespace kf3tweaks
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("けもフレ３.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static int RENDER_TEXTURE_RESOLUTION_MULT = 2; // Multiplier for texture sizes used with camera render-to-texture. The game usually uses 1024x for these

        // Keys for issuing commands to friends in combat
        // index 0 = friend 1 (from left to right)
        public static KeyCode[] FRIEND_ACT_KEYCODES = new KeyCode[]
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
        };

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            On.CanvasManager.Initialize += CanvasManager_Initialize;
            On.RenderTextureChara.SetupRenderTexture += RenderTextureChara_SetupRenderTexture;
            On.UserOptionData.SetDisplayQuality += UserOptionData_SetDisplayQuality;
            On.SceneManager.InitializeOption += SceneManager_InitializeOption;
            On.SceneBattle.Update += SceneBattle_Update;
            On.SceneHome.OnEnableScene += SceneHome_OnEnableScene;
            On.SceneHome.Update += SceneHome_Update;
        }

        private void SceneManager_InitializeOption(On.SceneManager.orig_InitializeOption orig)
        {
            orig();
            SceneManager.screenSize = new Resolution() { width = 1600, height = 900 };
            Logger.LogInfo("Patched screensize");
        }

        // Increase AA quality and enable aniso filtering
        // Large increase in graphics fidelity
        private void UserOptionData_SetDisplayQuality(On.UserOptionData.orig_SetDisplayQuality orig, UserOptionData self)
        {
            orig(self); // Not really necessary, on PC this method does nothing else

            QualitySettings.antiAliasing = 8; // Default is 1 in low-quality mode, 2 in high-quality
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
        }

        // Increase resolution of textures used with camera render-to-texture
        // Affects friend growth screen (super blurry normally!), newcomer gacha screen, daily login assistant, friend details, possibly more
        private void RenderTextureChara_SetupRenderTexture(On.RenderTextureChara.orig_SetupRenderTexture orig, RenderTextureChara self, int w, int h)
        {
            // Call orig with increased texture size
            orig(self, w * RENDER_TEXTURE_RESOLUTION_MULT, h * RENDER_TEXTURE_RESOLUTION_MULT);

            // Set size of the UI element back to the intended one - necessary as the end of SetupRenderTexture() calls SetNativeSize()
            FieldInfo field = typeof(RenderTextureChara).GetField("m_dispTexture", BindingFlags.NonPublic | BindingFlags.Instance);
            RawImage dispTexture = field.GetValue(self) as RawImage;
            RectTransform rect = dispTexture.transform as RectTransform;
            rect.sizeDelta = new Vector2(w, h); 
        }

        private void Update()
        {
            // Poll keybinds
            if (Input.GetKeyDown(KeyCode.LeftAlt)) // Toggle FPS cap
            {
                bool isUncapped = QualitySettings.vSyncCount != 0;
                QualitySettings.vSyncCount = isUncapped ? 0 : 1;
            }
            if (Input.GetKey(KeyCode.RightAlt) && Input.GetKeyDown(KeyCode.Return)) // Toggle borderless windowed
            {
                Resolution bestRes = Screen.resolutions[Screen.resolutions.Length - 1];
                Screen.SetResolution(bestRes.width, bestRes.height, Screen.fullScreenMode == FullScreenMode.FullScreenWindow ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow);
            }
        }

        void OnApplicationQuit()
        {
            // Necessary to prevent strange letterboxing from taking place if you previously closed the game with an unusual aspect ratio
            // Ideally that letterboxing would be disabled, but I'm not sure where exactly it happens - most likely in CanvasManager
            Screen.SetResolution(1600, 900, false);
        }

        // Disable resolution lock
        private void CanvasManager_Initialize(On.CanvasManager.orig_Initialize orig)
        {
            Type type = typeof(CanvasManager);
            FieldInfo info = type.GetField("oldWndProc", BindingFlags.NonPublic | BindingFlags.Static);

            // Prevents SetWindowProc from running, since the check in Update() is != zero. This field is not used for anything else, so it needn't be any valid pointer.
            info.SetValue(null, IntPtr.Zero + 1);

            orig();
        }

        private void SceneBattle_Update(On.SceneBattle.orig_Update orig, SceneBattle self)
        {
            // Check keybinds
            try
            {
                for (int i = 0; i < FRIEND_ACT_KEYCODES.Length; ++i)
                {
                    KeyCode keyCode = FRIEND_ACT_KEYCODES[i];
                    if (Input.GetKeyDown(keyCode))
                    {
                        bool useMiracle = Input.GetKey(KeyCode.LeftShift);
                        SceneBattle.GUI gui = GetField<SceneBattle, SceneBattle.GUI>(self, "guiData", BindingFlags.NonPublic | BindingFlags.Instance);
                        List<Transform> characterClickboxes = useMiracle ?  gui.TouchArts : gui.TouchChara; // Touch miracles instead if shift is held

                        if (i < characterClickboxes.Count)
                        {
                            SetField<SceneBattle>(self, "currentTouch", BindingFlags.NonPublic | BindingFlags.Instance, characterClickboxes[i]);
                        }
                    }
                }
                if (Input.GetKeyDown(KeyCode.Escape)) // Cancel flag order
                {
                    SetField<SceneBattle>(self, "cancelCardBtn", BindingFlags.NonPublic | BindingFlags.Instance, true);
                }
                if (Input.GetKeyDown(KeyCode.Space)) // Use refill
                {
                    SceneBattle.GUI gui = GetField<SceneBattle, SceneBattle.GUI>(self, "guiData", BindingFlags.NonPublic | BindingFlags.Instance);
                    SetField<SceneBattle>(self, "currentTouch", BindingFlags.NonPublic | BindingFlags.Instance, gui.TouchActGage);
                }
                if (Input.GetKeyDown(KeyCode.F)) // Toggle fast mode
                {
                    SceneBattle.GUI gui = GetField<SceneBattle, SceneBattle.GUI>(self, "guiData", BindingFlags.NonPublic | BindingFlags.Instance);
                    gui.BtnFast.m_Button.onClick?.Invoke();
                }
                if (Input.GetKeyDown(KeyCode.A)) // Toggle autoplay
                {
                    SceneBattle.GUI gui = GetField<SceneBattle, SceneBattle.GUI>(self, "guiData", BindingFlags.NonPublic | BindingFlags.Instance);
                    gui.BtnAuto.m_Button.onClick?.Invoke();
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }

            orig(self);
        }

        private void SceneHome_OnEnableScene(On.SceneHome.orig_OnEnableScene orig, SceneHome self, object args)
        {
            orig(self, args);
            UnrestrictHomeCamera(self);
        }

        private void SceneHome_Update(On.SceneHome.orig_Update orig, SceneHome self)
        {
            bool viewPositionChanged = GetField<SceneHome, bool>(self, "viewChg");

            orig(self); // Polls viewChg, set from change position button click

            if (viewPositionChanged)
            {
                UnrestrictHomeCamera(self);
            }
        }

        private void UnrestrictHomeCamera(SceneHome scene)
        {
            SetField<SceneHome>(scene, "viewMin", BindingFlags.NonPublic | BindingFlags.Instance, new Vector3(-180, -180, -180));
            SetField<SceneHome>(scene, "viewMax", BindingFlags.NonPublic | BindingFlags.Instance, new Vector3(180, 180, 180));
        }

        private T GetField<C, T>(C instance, string fieldName, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance)
        {
            return (T)(typeof(C).GetField(fieldName, flags).GetValue(instance));
        }

        private void SetField<C>(C instance, string fieldName, BindingFlags flags, object value)
        {
            typeof(C).GetField(fieldName, flags).SetValue(instance, value);
        }
    }
}
