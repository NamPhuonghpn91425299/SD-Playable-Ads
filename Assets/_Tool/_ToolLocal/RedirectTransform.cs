using UnityEngine;

public class MultiHandleParent : MonoBehaviour
{
    [Tooltip("Bao gồm cả chính đối tượng này?")]
    public bool includeSelf = true;

    [Tooltip("Có hiển thị handle cho các đối tượng con không?")]
    public bool includeChildren = true;
}