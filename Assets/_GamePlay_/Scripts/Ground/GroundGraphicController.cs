using UnityEngine;

namespace Sokoban.Presentation
{
    public class GroundGraphicController : MonoBehaviour
    {
        public MeshFilter meshFilter;

        public void SetMesh(Mesh mesh)
        {
            if (meshFilter != null)
            {
                meshFilter.mesh = mesh;
            }
        }
    }
}