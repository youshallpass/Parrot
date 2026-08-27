using UnityEngine;

[ExecuteAlways]
public class SetMainLightMatrix: MonoBehaviour
{
    private int property = Shader.PropertyToID("_WorldToMainLightMatrix");

    private void Update()
    {
        Matrix4x4 matrix = Matrix4x4.Rotate(transform.rotation);

        Shader.SetGlobalMatrix(property, matrix);
    }
}
