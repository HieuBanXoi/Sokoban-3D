using UnityEngine;

public class GroundGraphicController : MonoBehaviour
{
    public MeshFilter meshFilter;

    public void SetMesh(Mesh mesh)
    {
        if (meshFilter != null)
        {
            meshFilter.mesh = mesh;
        }
        else
        {
            Debug.LogError("GroundGraphicController: No MeshFilter assigned!");
        }
    }
}
