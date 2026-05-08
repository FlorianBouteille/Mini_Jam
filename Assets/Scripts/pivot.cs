using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);
    public float distance = 4f;
    public float sensitivityX = 200f;
    public float sensitivityY = 200f;
    public float minPitch = -40f;
    public float maxPitch = 80f;
    public bool invertY = false;
    public bool lockCursor = true;
    public bool useAsPivot = false; // when true, this object becomes the pivot for a Cinemachine vcam
    public bool requireMouseButton = false; // require holding a mouse button to rotate
    public int mouseButton = 1; // 0=left,1=right,2=middle

    float yaw = 0f;
    float pitch = 20f;

    void Start()
    {
        if (lockCursor) Cursor.lockState = CursorLockMode.Locked;
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (requireMouseButton && !Input.GetMouseButton(mouseButton))
        {
            // if acting as pivot, still keep pivot at target position
            if (useAsPivot)
            {
                transform.position = target.position + targetOffset;
            }
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityY * Time.deltaTime;

        yaw += mouseX;
        pitch -= (invertY ? -mouseY : mouseY);
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        if (useAsPivot)
        {
            // place pivot at target and rotate it; Cinemachine vcam can Follow this pivot
            transform.position = target.position + targetOffset;
            transform.rotation = rot;
        }
        else
        {
            Vector3 desiredPos = target.position + targetOffset - rot * Vector3.forward * distance;
            transform.position = desiredPos;
            transform.LookAt(target.position + targetOffset);
        }
    }
}
