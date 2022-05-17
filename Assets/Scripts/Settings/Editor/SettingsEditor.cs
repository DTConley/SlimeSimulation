using UnityEditor;
using UnityEngine;
using System.IO;

[CustomEditor(typeof(Settings))]
public class SettingsEditor : Editor
{
    Settings m_settings;

    private const string PRESET_DIRECTORY = "SettingsPresets";
    private int m_loadIndex = 0;

    public override void OnInspectorGUI()
    {
        DisplayProperties();

        EditorGUILayout.Space();

        bool saveSettings = GUILayout.Button("Save") && m_settings.m_presetName.Length > 0;

        GUILayout.BeginHorizontal();
        string[] loadOptions = GetLoadOptions();
        m_loadIndex = EditorGUILayout.Popup(m_loadIndex, loadOptions);

        string selectedFile = loadOptions[m_loadIndex];
        bool loadSettings = GUILayout.Button("Load") && m_loadIndex > 0;
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (saveSettings)
        {
            SaveSettings();
        }

        if (loadSettings)
        {
            LoadSettings(selectedFile);
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear"))
        {
            m_settings.m_clearSimulation = true;
        }

        m_settings.m_runSimulation = GUILayout.Toggle(m_settings.m_runSimulation, "Run", GUI.skin.button);
        
        if (!Application.isPlaying)
        {
            m_settings.m_runSimulation = false;
        }
        GUILayout.EndHorizontal();
    }

    void OnEnable()
    {
        m_settings = (Settings)target;
    }

    void DisplayProperties()
    {
        serializedObject.UpdateIfRequiredOrScript();

        int propertyIndex = 0;
        SerializedProperty currentProperty = serializedObject.GetIterator();
        bool expand = true;
        while (currentProperty.NextVisible(expand))
        {
            EditorGUILayout.PropertyField(currentProperty);

            if (propertyIndex == 8)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(new GUIContent("Num Agents", "The number of agents currently active in the simulation."));
                EditorGUILayout.BeginVertical("TextField");
                EditorGUI.ProgressBar(GUILayoutUtility.GetRect(1, 16), (float)m_settings.m_numAgents / m_settings.m_maxAgents, m_settings.m_numAgents + " / " + m_settings.m_maxAgents);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            expand = false;
            propertyIndex++;
        }

        serializedObject.ApplyModifiedProperties();
    }

    string[] GetLoadOptions()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(Application.dataPath + Path.AltDirectorySeparatorChar + PRESET_DIRECTORY);
        FileInfo[] fileInfos = null;
        int numOptions = 1;

        if (directoryInfo.Exists)
        {
            fileInfos = directoryInfo.GetFiles("*.json");
            numOptions += fileInfos.Length;
        }

        string[] loadOptions = new string[numOptions];
        loadOptions[0] = "None";
        for (int i = 1; i < numOptions; ++i)
        {
            loadOptions[i] = fileInfos[i - 1].Name;
        }

        return loadOptions;
    }

    void SaveSettings()
    {
        if (EditorUtility.DisplayDialog("Save Settings", "Are you sure you want to save these settings?", "Yes", "No"))
        {
            string saveFilePath = PRESET_DIRECTORY + Path.AltDirectorySeparatorChar + m_settings.m_presetName + ".json";
            string settingsJson = JsonUtility.ToJson(m_settings, true);

            StreamWriter writer = new StreamWriter(Application.dataPath + Path.AltDirectorySeparatorChar + saveFilePath);
            writer.Write(settingsJson);
            writer.Close();

            AssetDatabase.ImportAsset("Assets" + Path.AltDirectorySeparatorChar + saveFilePath);
        }
    }

    void LoadSettings(string selectedFile)
    {
        if (EditorUtility.DisplayDialog("Load Settings", "Are you sure you want to load " + selectedFile + "?", "Yes", "No"))
        {
            StreamReader reader = new StreamReader(Application.dataPath + Path.AltDirectorySeparatorChar + PRESET_DIRECTORY + Path.AltDirectorySeparatorChar + selectedFile);
            JsonUtility.FromJsonOverwrite(reader.ReadToEnd(), m_settings);
            reader.Close();
            
            m_settings.m_presetName = selectedFile.Substring(0, selectedFile.LastIndexOf('.'));
            m_settings.m_clearSimulation = true;
            m_settings.ClampSettings();
            m_loadIndex = 0;
        }
    }
}
