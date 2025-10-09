using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
public class SceneManager : MonoBehaviour
{
    public GameObject[] tempVecObjs;        // the objects that is used to calculate the visbility
    public static bool isPlayAnimation = false;
    public static bool onClickAddFrame = false;
    public static int checkFrame = 0;

    private static SceneManager s_Instance;

    public static SceneManager Instance
    {
        get
        {
            if (s_Instance != null) return s_Instance;

            s_Instance = GameObject.FindObjectOfType<SceneManager>();
            return s_Instance;
        }
    }

    [SerializeField] RayTracingRenderPipelineAsset rayTracingRenderPipelineAsset;

    public void Awake()
    {
        if (Application.isPlaying)
            DontDestroyOnLoad(this);

        isPlayAnimation = true;
        rayTracingRenderPipelineAsset.RenderMode = RayTracingRenderPipelineAsset.RTRenderSetting.RTRenderMode.VSAT;
        rayTracingRenderPipelineAsset.EnableIndirect = true;
        RayTracingRenderPipelineAsset.enableDenoise = false;
    }

    public void OnDisable()
    {
        isPlayAnimation = false;
    }
}