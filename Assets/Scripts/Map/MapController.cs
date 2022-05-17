using UnityEngine;

public class MapController : MonoBehaviour
{
    public Camera m_mainCamera;
    public Settings m_simulationSettings;

    public ComputeShader m_clearShader;

    private RenderTexture m_mapTexture;
    private RenderTexture m_diffuseTexture;

    public void ResetMap()
    {
        Vector3 newScale = new Vector3(
            2.0f * m_mainCamera.orthographicSize,
            2.0f * m_mainCamera.orthographicSize,
            1.0f);

        if (m_simulationSettings.m_resolutionType == Settings.ResolutionPresets.HD)
        {
            newScale.x *= (float)Screen.width / Screen.height;
        }
        transform.localScale = newScale;

        ResetMapTexture(ref m_mapTexture);
        ResetMapTexture(ref m_diffuseTexture);

        GetComponent<MeshRenderer>().material.mainTexture = m_mapTexture;
    }

    public void ClearMap()
    {
        ClearTexture(m_mapTexture, m_simulationSettings.m_width, m_simulationSettings.m_height);
        ClearTexture(m_diffuseTexture, m_simulationSettings.m_width, m_simulationSettings.m_height);
    }

    public RenderTexture GetMapTexture()
    {
        return m_mapTexture;
    }

    public RenderTexture GetDiffuseTexture()
    {
        return m_diffuseTexture;
    }

    public Vector3 MouseToMap(Vector3 mousePosition)
    {
        Vector3 worldPosition = m_mainCamera.ScreenToWorldPoint(mousePosition);
        float fullOrthographicSize = 2.0f * m_mainCamera.orthographicSize;
        
        Vector3 mapPosition = new Vector3(
            m_simulationSettings.m_width * ((worldPosition.x / fullOrthographicSize) + 0.5f),
            m_simulationSettings.m_height * ((worldPosition.y / fullOrthographicSize) + 0.5f),
            worldPosition.z);

        return mapPosition;
    }

    void ResetMapTexture(ref RenderTexture mapTexture)
    {
        if (mapTexture && mapTexture.IsCreated())
        {
            mapTexture.Release();
        }
        mapTexture = CreateMapTexture(m_simulationSettings.m_width, m_simulationSettings.m_height);
    }

    RenderTexture CreateMapTexture(int width, int height)
    {
        RenderTexture mapTexture = new RenderTexture(width, height, 0);
        mapTexture.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat;
        mapTexture.enableRandomWrite = true;
        mapTexture.autoGenerateMips = false;
        mapTexture.Create();

        mapTexture.wrapMode = TextureWrapMode.Clamp;
        mapTexture.filterMode = FilterMode.Point;

        return mapTexture;
    }

    void ClearTexture(RenderTexture texture, int width, int height)
    {
        if (texture == null || !texture.IsCreated())
        {
            return;
        }

        m_clearShader.SetTexture(0, "dirtyTexture", texture);
        m_clearShader.SetInt("width", width);
        m_clearShader.SetInt("height", height);

        m_clearShader.Dispatch(0, Mathf.CeilToInt((float)width / 8), Mathf.CeilToInt((float)height / 8), 1);
    }
}
