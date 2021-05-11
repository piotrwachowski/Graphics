using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;
using UnityEditorInternal;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;

namespace UnityEditor.Rendering.Universal
{
    using CED = CoreEditorDrawer<SerializedUniversalRenderPipelineGlobalSettings>;

    class UniversalGlobalSettingsPanelProvider
    {
        static UniversalGlobalSettingsPanelIMGUI s_IMGUIImpl = new UniversalGlobalSettingsPanelIMGUI();

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider("Project/Graphics/URP Global Settings", SettingsScope.Project)
            {
                activateHandler = s_IMGUIImpl.OnActivate,
                keywords = SettingsProvider.GetSearchKeywordsFromGUIContentProperties<UniversalGlobalSettingsPanelIMGUI.Styles>().ToArray(),
                guiHandler = s_IMGUIImpl.DoGUI
            };
        }
    }

    internal partial class UniversalGlobalSettingsPanelIMGUI
    {
        /// <summary>
        /// Like EditorGUILayout.DrawTextField but for delayed text field
        /// </summary>
        internal static void DrawDelayedTextField(GUIContent label, SerializedProperty property)
        {
            Rect lineRect = GUILayoutUtility.GetRect(1, EditorGUIUtility.singleLineHeight);
            EditorGUI.BeginProperty(lineRect, label, property);
            EditorGUI.BeginChangeCheck();
            string value = EditorGUI.DelayedTextField(lineRect, label, property.stringValue);
            if (EditorGUI.EndChangeCheck())
                property.stringValue = value;
            EditorGUI.EndProperty();
        }

        public static readonly CED.IDrawer Inspector;

        public class DocumentationUrls
        {
            public static readonly string k_LightLayers = "Light-Layers";
        }

        static UniversalGlobalSettingsPanelIMGUI()
        {
            Inspector = CED.Group(
                LightLayerNamesSection
            );
        }

        SerializedUniversalRenderPipelineGlobalSettings serializedSettings;
        UniversalRenderPipelineGlobalSettings settingsSerialized;
        public void DoGUI(string searchContext)
        {
            // When the asset being serialized has been deleted before its reconstruction
            if (serializedSettings != null && serializedSettings.serializedObject.targetObject == null)
            {
                serializedSettings = null;
                settingsSerialized = null;
            }

            if (serializedSettings == null || settingsSerialized != UniversalRenderPipelineGlobalSettings.instance)
            {
                if (UniversalRenderPipeline.asset != null || UniversalRenderPipelineGlobalSettings.instance != null)
                {
                    settingsSerialized = UniversalRenderPipelineGlobalSettings.Ensure();
                    var serializedObject = new SerializedObject(settingsSerialized);
                    serializedSettings = new SerializedUniversalRenderPipelineGlobalSettings(serializedObject);
                }
            }
            else if (settingsSerialized != null && serializedSettings != null)
            {
                serializedSettings.serializedObject.Update();
            }

            DrawAssetSelection(ref serializedSettings, null);
            DrawWarnings(ref serializedSettings, null);
            if (settingsSerialized != null && serializedSettings != null)
            {
                EditorGUILayout.Space();
                Inspector.Draw(serializedSettings, null);
                serializedSettings.serializedObject?.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Executed when activate is called from the settings provider.
        /// </summary>
        /// <param name="searchContext"></param>
        /// <param name="rootElement"></param>
        public void OnActivate(string searchContext, VisualElement rootElement)
        {
        }

        void DrawWarnings(ref SerializedUniversalRenderPipelineGlobalSettings serialized, Editor owner)
        {
            bool isURPinUse = UniversalRenderPipeline.asset != null;
            if (isURPinUse && serialized != null)
                return;

            if (isURPinUse)
            {
                EditorGUILayout.HelpBox(Styles.warningGlobalSettingsMissing, MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox(Styles.warningUrpNotActive, MessageType.Warning);
                if (serialized == null)
                    EditorGUILayout.HelpBox(Styles.infoGlobalSettingsMissing, MessageType.Info);
            }
        }

        #region Universal Global Settings asset selection
        void DrawAssetSelection(ref SerializedUniversalRenderPipelineGlobalSettings serialized, Editor owner)
        {
            var oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = Styles.labelWidth;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                var newAsset = (UniversalRenderPipelineGlobalSettings)EditorGUILayout.ObjectField(settingsSerialized, typeof(UniversalRenderPipelineGlobalSettings), false);
                if (EditorGUI.EndChangeCheck())
                {
                    UniversalRenderPipelineGlobalSettings.UpdateGraphicsSettings(newAsset);
                    if (settingsSerialized != null && !settingsSerialized.Equals(null))
                        EditorUtility.SetDirty(settingsSerialized);
                }

                if (GUILayout.Button(Styles.newAssetButtonLabel, GUILayout.Width(45), GUILayout.Height(18)))
                {
                    UniversalGlobalSettingsCreator.Create(useProjectSettingsFolder: true, activateAsset: true);
                }

                bool guiEnabled = GUI.enabled;
                GUI.enabled = guiEnabled && (settingsSerialized != null);
                if (GUILayout.Button(Styles.cloneAssetButtonLabel, GUILayout.Width(45), GUILayout.Height(18)))
                {
                    UniversalGlobalSettingsCreator.Clone(settingsSerialized, activateAsset: true);
                }
                GUI.enabled = guiEnabled;
            }
            EditorGUIUtility.labelWidth = oldWidth;
            EditorGUILayout.Space();
        }

        #endregion
        #region Rendering Layer Names

        static readonly CED.IDrawer LightLayerNamesSection = CED.Group(
            CED.Group((serialized, owner) => CoreEditorUtils.DrawSectionHeader(Styles.lightLayersLabel, contextAction: pos => OnContextClickLightLayerNames(pos, serialized))),
            CED.Group((serialized, owner) => EditorGUILayout.Space()),
            CED.Group(DrawLightLayerNames),
            CED.Group((serialized, owner) => EditorGUILayout.Space())
        );

        static void DrawLightLayerNames(SerializedUniversalRenderPipelineGlobalSettings serialized, Editor owner)
        {
            var oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = Styles.labelWidth;

            using (new EditorGUI.IndentLevelScope())
            {
                DrawDelayedTextField(Styles.lightLayerName0, serialized.lightLayerName0);
                GUILayout.Space(2);
                DrawDelayedTextField(Styles.lightLayerName1, serialized.lightLayerName1);
                GUILayout.Space(2);
                DrawDelayedTextField(Styles.lightLayerName2, serialized.lightLayerName2);
                GUILayout.Space(2);
                DrawDelayedTextField(Styles.lightLayerName3, serialized.lightLayerName3);
                GUILayout.Space(2);
                DrawDelayedTextField(Styles.lightLayerName4, serialized.lightLayerName4);
                GUILayout.Space(2);
                DrawDelayedTextField(Styles.lightLayerName5, serialized.lightLayerName5);
                GUILayout.Space(2);
                DrawDelayedTextField(Styles.lightLayerName6, serialized.lightLayerName6);
                GUILayout.Space(2);
                DrawDelayedTextField(Styles.lightLayerName7, serialized.lightLayerName7);
                EditorGUILayout.Space();
            }

            EditorGUIUtility.labelWidth = oldWidth;
        }

        static void OnContextClickLightLayerNames(Vector2 position, SerializedUniversalRenderPipelineGlobalSettings serialized)
        {
            var menu = new GenericMenu();
            menu.AddItem(CoreEditorStyles.resetButtonLabel, false, () =>
            {
                var globalSettings = (serialized.serializedObject.targetObject as UniversalRenderPipelineGlobalSettings);
                globalSettings.ResetRenderingLayerNames();
            });
            menu.DropDown(new Rect(position, Vector2.zero));
        }

        #endregion
    }
}
