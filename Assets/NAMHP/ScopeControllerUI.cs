using UnityEngine;
using UnityEngine.UI; // Cần cho truy cập Material của Image

[ExecuteInEditMode] // Chạy cả trong Editor
public class ScopeControllerUI : MonoBehaviour
{
    // Kéo Material UI Scope vào đây
    public Material scopeMaterial;

    [Range(0f, 1f)]
    public float scopeRadius = 0.4f;
    public Vector2 scopeCenter = new Vector2(0.5f, 0.5f);
    [Range(0f, 0.2f)]
    public float edgeSoftness = 0.05f;

    private int scopeCenterID;
    private int scopeRadiusID;
    private int aspectRatioID;
    private int edgeSoftnessID;

    void Awake()
    {
        // Lấy ID của các thuộc tính Shader để tối ưu hiệu suất
        scopeCenterID = Shader.PropertyToID("_ScopeCenter");
        scopeRadiusID = Shader.PropertyToID("_ScopeRadius");
        aspectRatioID = Shader.PropertyToID("_AspectRatio");
        edgeSoftnessID = Shader.PropertyToID("_EdgeSoftness");
    }

    void Update()
    {
        if (scopeMaterial != null)
        {
            // Cập nhật các thuộc tính của material
            scopeMaterial.SetVector(scopeCenterID, new Vector4(scopeCenter.x, scopeCenter.y, 0, 0));
            scopeMaterial.SetFloat(scopeRadiusID, scopeRadius);
            scopeMaterial.SetFloat(edgeSoftnessID, edgeSoftness);

            // Cập nhật tỷ lệ khung hình màn hình
            // Screen.width và Screen.height đáng tin cậy hơn trong Update
            float aspectRatio = (float)Screen.width / Screen.height;
            scopeMaterial.SetFloat(aspectRatioID, aspectRatio);
        }
        else
        {
             #if UNITY_EDITOR
             // Nhắc nhở trong Editor nếu quên gán Material
             if(Application.isPlaying == false)
             {
                  //Debug.LogWarning("Scope Material chưa được gán cho ScopeControllerUI.", this);
             }
             #endif
        }
    }

    // Có thể thêm OnValidate để cập nhật trong Editor khi thay đổi giá trị Inspector
    #if UNITY_EDITOR
    void OnValidate()
    {
         // Cần lấy ID lại trong OnValidate vì Awake không chạy khi chỉ thay đổi Inspector
         scopeCenterID = Shader.PropertyToID("_ScopeCenter");
         scopeRadiusID = Shader.PropertyToID("_ScopeRadius");
         aspectRatioID = Shader.PropertyToID("_AspectRatio");
         edgeSoftnessID = Shader.PropertyToID("_EdgeSoftness");
         // Gọi Update để cập nhật ngay lập tức trong Editor
         Update();
    }
    #endif
}