using UnityEngine;

[System.Serializable]
[CreateAssetMenu]
public class Settings : ScriptableObject
{
    public enum ResolutionPresets { BOX, HD }

    [Tooltip("The file name of the settings preset.")]
    public string m_presetName = "Untitled";

    [Header("Texture Properties")]
    [Tooltip("HD restricts the resolution to multiples of 9:16, and Box restricts the to multiples of 1:1.")]
    public ResolutionPresets m_resolutionType = ResolutionPresets.HD;
    [Tooltip("Changes width of map texture in pixels.")]
    [Delayed]
    public int m_width = 16;
    [Tooltip("Changes height of map texture in pixels.")]
    [Delayed]
    public int m_height = 9;
    [Tooltip("Changes the speed the agent's trail decays.")]
    [Min(0.0f)]
    public float m_decayFactor = 0.1f;
    [Tooltip("Changes the speed the agent's trail diffuses into other pixels.")]
    [Min(0.0f)]
    public float m_diffuseFactor = 1.0f;
    [Tooltip("Changes the smoothness of the trail diffusion.")]
    [Min(0)]
    public int m_diffuseKernelSize = 1;

    [Header("Simulation Properties")]
    [Tooltip("Changes the maximum number of agents that can exist in the simulation at once.")]
    [Delayed]
    public int m_maxAgents = 5000000;
    [HideInInspector]
    public int m_numAgents = 0;
    [Tooltip("Changes the speed of the simulation.")]
    [Min(1)]
    public int m_numSteps = 1;

    [Header("Agent Properties")]
    [Tooltip("Changes the speed that the agent moves. Note that 50 means it moves one pixel per step.")]
    [Min(0.0f)]
    public float m_agentSpeed = 50.0f;
    [Tooltip("Changes the speed that the agent turns toward other color pixels.")]
    [Min(0.0f)]
    public float m_agentRotationSpeed = 10.0f;
    [Tooltip("Changes the color of the agents.")]
    public Color m_agentColor = Color.white;

    [Header("Agent Sensor Properties")]
    [Tooltip("Changes the distance that the agent's look in front of them for colored pixels.")]
    public int m_sensorDistance = 9;
    [Tooltip("Changes the angle between the forward facing sensor and each sensor at its left and right.")]
    [Range(0.0f, Mathf.PI / 2.0f)]
    public float m_sensorAngle = Mathf.PI / 4.0f;
    [Tooltip("Changes the width of the square of pixels that the sensor reads for color.")]
    [Min(0)]
    public int m_sensorWidth = 1;

    [Header("Click Create Properties")]
    [Tooltip("Changes the radius for the circle around the mouse where agents are created.")]
    [Min(0.01f)]
    public float m_createRadius = 1.0f;
    [Tooltip("Changes the number of the agents created per step while the left mouse button is being pressed.")]
    [Min(0)]
    public int m_numAgentsPerStep = 1;

    [HideInInspector]
    [System.NonSerialized]
    public bool m_clearSimulation = false;
    [HideInInspector]
    [System.NonSerialized]
    public bool m_runSimulation = false;

    int m_prevWidth = -1;
    int m_prevHeight = -1;

    void OnValidate()
    {
        ClampSettings();
    }

    public void ClampSettings()
    {
        if (m_width <= 0)
        {
            m_width = 1;
        }

        if (m_height <= 0)
        {
            m_height = 1;
        }

        if (m_resolutionType == ResolutionPresets.BOX)
        {
            if (m_height != m_width)
            {
                if (m_width != m_prevWidth)
                {
                    m_height = m_width;
                }
                else
                {
                    m_width = m_height;
                }

                m_prevWidth = m_width;
                m_prevHeight = m_width;
            }
        }
        else if (m_resolutionType == ResolutionPresets.HD)
        {
            if (m_height == m_width)
            {
                m_prevHeight = -1;
            }

            if (m_width != m_prevWidth)
            {
                int resolutionMultiplier = Mathf.Max(1, m_width / 16);
                m_width = 16 * resolutionMultiplier;
                m_prevWidth = m_width;
                m_height = 9 * resolutionMultiplier;
                m_prevHeight = m_height;
            }
            else if (m_height != m_prevHeight)
            {
                int resolutionMultiplier = Mathf.Max(1, m_height / 9);
                m_height = 9 * resolutionMultiplier;
                m_prevHeight = m_height;
                m_width = 16 * resolutionMultiplier;
                m_prevWidth = m_width;
            }
        }

        if (m_maxAgents > 8000000)
        {
            m_maxAgents = 8000000;
        }
        else if (m_maxAgents <= 0)
        {
            m_maxAgents = 1;
        }

        if (m_numAgents > m_maxAgents)
        {
            m_numAgents = m_maxAgents;
        }
    }
}
