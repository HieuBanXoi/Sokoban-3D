using UnityEngine;

public class BoxGraphicController : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public void SetMaterial(Material mat)
    {
        meshRenderer.material = mat;
    }
}
