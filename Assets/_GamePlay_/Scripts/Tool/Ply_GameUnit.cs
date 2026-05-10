using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ply_GameUnit : MonoBehaviour
{
    public Transform tf;

    // Cached transform accessor for convenience and performance
    public Transform TF { get { if (tf == null) tf = transform; return tf; } }

}
