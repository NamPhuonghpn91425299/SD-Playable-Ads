using System.Collections.Generic;
using UnityEngine;

public class WallFracture : MonoBehaviour
{
    public Material fragmentMaterial;
    public int fractureIterations = 1;
    public float explosionForce = 2f;
    public float explosionRadius = 1f;
    public float fragmentLifetime = 5f;

    private Mesh originalMesh;

    void Start()
    {
        originalMesh = GetComponent<MeshFilter>().mesh;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) // Nhấn F để test phá vỡ ở trung tâm
        {
            TakeBulletHit(transform.position, Vector3.up);
        }
        
        if (Input.GetKeyDown(KeyCode.G)) // Nhấn G để test phá vỡ ở vị trí ngẫu nhiên
        {
            Vector3 randomPoint = transform.position + new Vector3(
                Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
            TakeBulletHit(randomPoint, Vector3.up);
        }
    }

    public void TakeBulletHit(Vector3 hitPoint, Vector3 hitNormal)
    {
        FractureMesh(hitPoint);
        Destroy(gameObject);
    }

void FractureMesh(Vector3 impactPoint)
{
    Queue<Mesh> meshQueue = new Queue<Mesh>();
    meshQueue.Enqueue(originalMesh);

    for (int i = 0; i < fractureIterations; i++) // Cắt nhiều lần
    {
        int count = meshQueue.Count;
        for (int j = 0; j < count; j++)
        {
            Mesh mesh = meshQueue.Dequeue();
            Plane fracturePlane = new Plane(Random.onUnitSphere, impactPoint); // Mặt phẳng cắt ngẫu nhiên
            List<Mesh> fragments = CutMesh(mesh, fracturePlane);
            foreach (Mesh fragment in fragments)
            {
                meshQueue.Enqueue(fragment);
            }
        }
    }

    foreach (Mesh mesh in meshQueue)
    {
        CreateFragment(mesh, impactPoint);
    }
}

// ✅ Cập nhật hàm CutMesh để nhận `Plane` làm tham số
List<Mesh> CutMesh(Mesh mesh, Plane fracturePlane)
{
    List<Vector3> part1Vertices = new List<Vector3>();
    List<int> part1Triangles = new List<int>();
    List<Vector3> part2Vertices = new List<Vector3>();
    List<int> part2Triangles = new List<int>();

    Vector3[] vertices = mesh.vertices;
    int[] triangles = mesh.triangles;

    for (int i = 0; i < triangles.Length; i += 3)
    {
        Vector3 v1 = vertices[triangles[i]];
        Vector3 v2 = vertices[triangles[i + 1]];
        Vector3 v3 = vertices[triangles[i + 2]];

        bool above1 = fracturePlane.GetSide(v1);
        bool above2 = fracturePlane.GetSide(v2);
        bool above3 = fracturePlane.GetSide(v3);

        if (above1 && above2 && above3)
        {
            AddTriangle(part1Vertices, part1Triangles, v1, v2, v3);
        }
        else if (!above1 && !above2 && !above3)
        {
            AddTriangle(part2Vertices, part2Triangles, v1, v2, v3);
        }
        else
        {
            if (above1) AddTriangle(part1Vertices, part1Triangles, v1, v2, v3);
            else AddTriangle(part2Vertices, part2Triangles, v1, v2, v3);
        }
    }

    List<Mesh> results = new List<Mesh>();
    if (part1Vertices.Count > 0) results.Add(CreateMesh(part1Vertices, part1Triangles));
    if (part2Vertices.Count > 0) results.Add(CreateMesh(part2Vertices, part2Triangles));
    return results;
}


    void AddTriangle(List<Vector3> vertList, List<int> triList, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int index = vertList.Count;
        vertList.Add(v1);
        vertList.Add(v2);
        vertList.Add(v3);
        triList.Add(index);
        triList.Add(index + 1);
        triList.Add(index + 2);
    }

    Mesh CreateMesh(List<Vector3> vertices, List<int> triangles)
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals(); // Sửa lỗi hiển thị
        mesh.RecalculateBounds();  // Đảm bảo vật thể có vùng hiển thị đúng
        return mesh;
    }


    void CreateFragment(Mesh fragmentMesh, Vector3 explosionPosition)
    {
        GameObject fragment = new GameObject("Fragment");
        fragment.transform.position = transform.position;
        fragment.transform.rotation = transform.rotation;

        fragment.AddComponent<MeshFilter>().mesh = fragmentMesh;

        MeshRenderer originalRenderer = GetComponent<MeshRenderer>();
        MeshRenderer fragmentRenderer = fragment.AddComponent<MeshRenderer>();
        fragmentRenderer.sharedMaterials = originalRenderer.sharedMaterials; // Sao chép Material chính xác

        fragment.AddComponent<MeshCollider>().convex = true;

        Rigidbody rb = fragment.AddComponent<Rigidbody>();
        rb.mass = 0.5f;
        rb.AddExplosionForce(explosionForce, explosionPosition, explosionRadius, 0.1f, ForceMode.Impulse);

        Destroy(fragment, fragmentLifetime); // Mảnh vỡ tự động biến mất

        Debug.Log($"Fragment {fragment.name} created with Material: {fragmentRenderer.sharedMaterials.Length}");
    }


}
