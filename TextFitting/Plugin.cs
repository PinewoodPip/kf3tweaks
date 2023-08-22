using BepInEx;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine;
using System;

namespace TextFitting
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("けもフレ３.exe")]
    public class Plugin : BaseUnityPlugin
    {
        public static int MINIMUM_FONT_SIZE = 10;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            On.TypewriterEffect.SetCurrentText += TypewriterEffect_SetCurrentText;
            On.SceneQuest.GUI.ChapterSelect.InactiveParts += ChapterSelect_InactiveParts;
            On.CommunicationCtrl.CharaViewGuiData.ctor += OnFriendMemoryCreated;
            On.GachaAuthCtrl.GachaAeGreeting.ctor += OnGachaGreetingCreated;
            On.SelLoginBonus.GUI.ctor += OnLoginBonusUICreated;
            On.CharaUtil.GUISkillInfo.Setup += OnSkillInfoCreated;
            On.SelShopCtrl.ShopBtn.ctor += OnShopButtonCreated;

        private void OnDestroy()
        {
            On.TypewriterEffect.SetCurrentText -= TypewriterEffect_SetCurrentText;
            On.SceneQuest.GUI.ChapterSelect.InactiveParts -= ChapterSelect_InactiveParts;
            On.CommunicationCtrl.CharaViewGuiData.ctor -= OnFriendMemoryCreated;
            On.GachaAuthCtrl.GachaAeGreeting.ctor -= OnGachaGreetingCreated;
            On.SelLoginBonus.GUI.ctor -= OnLoginBonusUICreated;
            On.CharaUtil.GUISkillInfo.Setup -= OnSkillInfoCreated;
            On.SelShopCtrl.ShopBtn.ctor -= OnShopButtonCreated;

        }

        public static void FitText(Text text, int maxSize = -1)
        {
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = MINIMUM_FONT_SIZE;
            text.resizeTextMaxSize = maxSize == -1 ? text.fontSize : maxSize;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
        }

        private void ChapterSelect_InactiveParts(On.SceneQuest.GUI.ChapterSelect.orig_InactiveParts orig, SceneQuest.GUI.ChapterSelect self)
        {
            orig(self);
            foreach (SceneQuest.GUI.ChapterSelect.Parts part in self.parts)
            {
                FitText(part.Txt_Serif.m_Text);
                //FitText(part.Txt_CharaName.m_Text); // Possibly unnecessary / ugly
            }
            Logger.LogInfo("Fitted text for ChapterSelect parts");
        }

        private void TypewriterEffect_SetCurrentText(On.TypewriterEffect.orig_SetCurrentText orig, TypewriterEffect self, string text, int size, int spd)
        {
            orig(self, text, size, spd);
            Text textField = GetField<TypewriterEffect, Text>(self, "mTestText");
            textField.rectTransform.sizeDelta = new Vector2(675, textField.rectTransform.sizeDelta.y); // Expand the dialogue box a bit, since it is surprisingly short horizontally. Default width is 567. It could also be moved to the left a bit to make more space and keep the margin on the sides equal - TODO
            FitText(textField);
            Logger.LogInfo("Fitting text");
        }

        private void OnFriendMemoryCreated(On.CommunicationCtrl.CharaViewGuiData.orig_ctor orig, CommunicationCtrl.CharaViewGuiData self, GameObject go)
        {
            orig(self, go);

            try
            {
                GameObject textObject = go.transform.Find("Auth_HeartLvUp/AEImage_Info/Serif_Info03/Txt").gameObject;
                Text text = textObject.GetComponent<PguiTextCtrl>().m_Text;
                ContentSizeFitter contentFitter = textObject.GetComponent<ContentSizeFitter>();
                contentFitter.enabled = false;
                text.rectTransform.sizeDelta = new Vector2(1500, 100);
                Logger.LogInfo(text);
                FitText(text, 35);
                text.alignment = TextAnchor.UpperCenter;
                Logger.LogInfo("Fitting growth level up text fields");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }

        private void OnGachaGreetingCreated(On.GachaAuthCtrl.GachaAeGreeting.orig_ctor orig, GachaAuthCtrl.GachaAeGreeting self, Transform baseTr)
        {
            orig(self, baseTr);
            FitText(self.Txt_Serif.m_Text);
        }

        private void OnLoginBonusUICreated(On.SelLoginBonus.GUI.orig_ctor orig, SelLoginBonus.GUI self, Transform baseTr)
        {
            orig(self, baseTr);
            Text text = self.Txt_Serif.m_Text;
            FitText(text);
        }

        private void OnSkillInfoCreated(On.CharaUtil.GUISkillInfo.orig_Setup orig, CharaUtil.GUISkillInfo self, CharaUtil.GUISkillInfo.SetupParam setupParam)
        {
            orig(self, setupParam);
            Text skillDescriptionText = self.Txt_Info.m_Text;
            RectTransform descriptionParent = skillDescriptionText.rectTransform.parent as RectTransform;

            skillDescriptionText.rectTransform.sizeDelta = new Vector2(skillDescriptionText.rectTransform.sizeDelta.x, descriptionParent.rect.height - 48); // Normally these text fields are very tall; we set them to roughly the visual size of the box to make downsizing text work

            FitText(self.Txt_Info.m_Text);
            FitText(self.Txt_Name.m_Text);
        }

        private void OnShopButtonCreated(On.SelShopCtrl.ShopBtn.orig_ctor orig, SelShopCtrl.ShopBtn self, Transform baseTr)
        {
            orig(self, baseTr);
            Text text = self.Txt.m_Text;
            RectTransform rect = text.rectTransform;

            rect.sizeDelta = new Vector2(rect.sizeDelta.x, 50); // Default is 30; not enough for wrapping
            FitText(text);
        }

        public static T GetField<C, T>(C instance, string fieldName, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance)
        {
            return (T)(typeof(C).GetField(fieldName, flags).GetValue(instance));
        }

        public static void SetField<C>(C instance, string fieldName, object value, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance)
        {
            typeof(C).GetField(fieldName, flags).SetValue(instance, value);
        }
    }
}
