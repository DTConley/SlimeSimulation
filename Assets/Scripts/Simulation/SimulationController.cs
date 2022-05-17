using System.Runtime.InteropServices;
using UnityEngine;

public class SimulationController : MonoBehaviour
{
    public Settings m_simulationSettings;
    public MapController m_mapController;

    public ComputeShader m_simulationComputeShader;

    const int SLIME_UPDATE_KERNEL = 0;
    const int MAP_UPDATE_KERNEL = 1;
    const int CREATE_AGENT_KERNEL = 2;

    Agent[] m_agentList;
    ComputeBuffer m_agentBuffer;

    void Start()
    {
        InitializeSimulatuion();
    }
    
    void FixedUpdate()
    {
        if (m_simulationSettings.m_clearSimulation)
        {
            m_simulationSettings.m_numAgents = 0;
            m_simulationComputeShader.SetInt("numAgents", m_simulationSettings.m_numAgents);

            m_mapController.ClearMap();

            m_simulationSettings.m_runSimulation = false;
            m_simulationSettings.m_clearSimulation = false;
        }

        if (m_simulationSettings.m_height != m_mapController.GetMapTexture().height ||
            m_simulationSettings.m_width != m_mapController.GetMapTexture().width ||
            m_simulationSettings.m_maxAgents != m_agentList.Length)
        {
            ResetSimulation();
        }

        if (m_simulationSettings.m_numAgents == 0)
        {
            return;
        }

        m_simulationComputeShader.SetFloat("decayFactor", m_simulationSettings.m_decayFactor);
        m_simulationComputeShader.SetFloat("diffuseFactor", m_simulationSettings.m_diffuseFactor);
        m_simulationComputeShader.SetInt("diffuseKernelSize", m_simulationSettings.m_diffuseKernelSize);

        m_simulationComputeShader.SetFloat("agentMovementSpeed", m_simulationSettings.m_agentSpeed);
        m_simulationComputeShader.SetFloat("agentRotationSpeed", m_simulationSettings.m_agentRotationSpeed);
        m_simulationComputeShader.SetVector("agentColor", m_simulationSettings.m_agentColor);

        m_simulationComputeShader.SetInt("sensorDistance", m_simulationSettings.m_sensorDistance);
        m_simulationComputeShader.SetFloat("sensorAngle", m_simulationSettings.m_sensorAngle);
        m_simulationComputeShader.SetInt("sensorWidth", m_simulationSettings.m_sensorWidth);

        m_simulationComputeShader.SetBool("runSimulation", m_simulationSettings.m_runSimulation);

        for (int i = 0; i < m_simulationSettings.m_numSteps; ++i)
        {
            m_simulationComputeShader.SetFloat("time", Time.fixedTime);

            m_simulationComputeShader.Dispatch(SLIME_UPDATE_KERNEL, Mathf.CeilToInt((float)m_simulationSettings.m_numAgents / 128), 1, 1);
            m_simulationComputeShader.Dispatch(MAP_UPDATE_KERNEL, Mathf.CeilToInt((float)m_simulationSettings.m_width / 8), Mathf.CeilToInt((float)m_simulationSettings.m_height / 8), 1);

            Graphics.Blit(m_mapController.GetDiffuseTexture(), m_mapController.GetMapTexture());
        }
    }

    void OnDestroy()
    {
        CleanupSimulation();
    }

    public void ResetSimulation()
    {
        CleanupSimulation();
        InitializeSimulatuion();
    }

    public void CreateAgents(Vector2 position, float angle, bool ignoreAngle = false)
    {
        Vector2 clampedPosition;
        clampedPosition.x = Mathf.Min(m_simulationSettings.m_width - 1, Mathf.Max(0.0f, position.x));
        clampedPosition.y = Mathf.Min(m_simulationSettings.m_height - 1, Mathf.Max(0.0f, position.y));

        int numCreated = m_simulationSettings.m_numSteps * m_simulationSettings.m_numAgentsPerStep;
        numCreated -= Mathf.Max(0, m_simulationSettings.m_numAgents + numCreated - m_simulationSettings.m_maxAgents);
        if (numCreated <= 0)
        {
            return;
        }

        m_simulationComputeShader.SetFloat("createRadius", m_simulationSettings.m_createRadius);
        m_simulationComputeShader.SetVector("createPosition", clampedPosition);
        m_simulationComputeShader.SetFloat("createAngle", angle);
        m_simulationComputeShader.SetBool("ignoreCreateAngle", ignoreAngle);
        m_simulationComputeShader.SetFloat("time", Time.fixedTime);
        m_simulationComputeShader.SetInt("numCreateAgents", numCreated);
        m_simulationComputeShader.Dispatch(CREATE_AGENT_KERNEL, Mathf.CeilToInt((float)numCreated / 128), 1, 1);

        m_simulationSettings.m_numAgents += numCreated;
        m_simulationComputeShader.SetInt("numAgents", m_simulationSettings.m_numAgents);
    }

    void InitializeSimulatuion()
    {
        m_simulationSettings.m_runSimulation = false;

        m_mapController.ResetMap();

        m_agentList = new Agent[m_simulationSettings.m_maxAgents];
        m_agentBuffer = new ComputeBuffer(m_simulationSettings.m_maxAgents, Marshal.SizeOf(typeof(Agent)));
        m_agentBuffer.SetData(m_agentList);


        m_simulationComputeShader.SetBuffer(SLIME_UPDATE_KERNEL, "agents", m_agentBuffer);
        m_simulationComputeShader.SetBuffer(CREATE_AGENT_KERNEL, "agents", m_agentBuffer);

        m_simulationComputeShader.SetFloat("deltaTime", Time.fixedDeltaTime);

        m_simulationComputeShader.SetInt("mapWidth", m_simulationSettings.m_width);
        m_simulationComputeShader.SetInt("mapHeight", m_simulationSettings.m_height);

        m_simulationComputeShader.SetInt("numAgents", m_simulationSettings.m_numAgents);

        m_simulationComputeShader.SetTexture(SLIME_UPDATE_KERNEL, "simulationMap", m_mapController.GetMapTexture());
        m_simulationComputeShader.SetTexture(MAP_UPDATE_KERNEL, "simulationMap", m_mapController.GetMapTexture());
        m_simulationComputeShader.SetTexture(MAP_UPDATE_KERNEL, "diffuseMap", m_mapController.GetDiffuseTexture());
    }

    void CleanupSimulation()
    {
        m_agentBuffer.Dispose();
        m_simulationSettings.m_numAgents = 0;
    }

    public struct Agent
    {
        public Vector2 m_position;
        public float m_angle;
    }
}
