using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public Camera camera;
    private float xRotation = 0f;

    public float XSensitivity = 30f;
    public float YSensitivity = 30f;
    

    public void ManageLook(Vector2 input)
    {
        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= (mouseY * Time.deltaTime) * YSensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * XSensitivity);
    }
}
