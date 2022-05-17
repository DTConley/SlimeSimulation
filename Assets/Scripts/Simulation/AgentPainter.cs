using UnityEngine;

public class AgentPainter : MonoBehaviour
{
    public Settings m_simulationSettings;
    public SimulationController m_simulationController;
    public MapController m_mapController;

    private Vector3 m_prevMousePosition;
    private bool m_gainedFocus = false;

    void Start()
    {
        m_prevMousePosition = Input.mousePosition;   
    }

    void OnApplicationFocus(bool focus)
    {
        m_gainedFocus = focus;
    }

    void FixedUpdate()
    {
        Vector3 mouseDelta = Input.mousePosition - m_prevMousePosition;
        
        if (Input.GetMouseButton(0) && !m_gainedFocus)
        {
            Vector2 mouseMapPosition = m_mapController.MouseToMap(Input.mousePosition);
            
            float agentAngle = Vector3.SignedAngle(Vector3.right, mouseDelta, Vector3.forward) * Mathf.Deg2Rad;
            bool mouseMoved = Mathf.Abs(mouseDelta.magnitude) < Mathf.Epsilon;

            m_simulationController.CreateAgents(mouseMapPosition, agentAngle, mouseMoved);
        }

        m_prevMousePosition = Input.mousePosition;
        m_gainedFocus = false;
    }
}
