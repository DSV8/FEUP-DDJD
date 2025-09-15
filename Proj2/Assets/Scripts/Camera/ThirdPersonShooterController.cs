using UnityEngine;
public class ThirdPersonShooterController : MonoBehaviour
{
    [SerializeField] private float mouseSens = 5;
    [SerializeField] private Transform camParent;
    [SerializeField] private Transform cam;

    public LayerMask playerLayer;

    private void Update()
    {
        if (PauseMenu.gameIsPaused) return;
        
        float rotY = mouseSens * Input.GetAxis("Mouse X");
        float rotX = -mouseSens * Input.GetAxis("Mouse Y");

        transform.Rotate(new Vector3(0, rotY, 0));
        camParent.localEulerAngles = new Vector3(camParent.localEulerAngles.x + rotX, 0, 0);

        Vector3 camOffset = new Vector3(0, 0.7f, -7);

        RaycastHit hit;
        if (Physics.SphereCast(camParent.position - new Vector3(0, 0.7f, 0), 0.2f, camParent.rotation * camOffset.normalized, out hit, 7, ~playerLayer))
        {
            cam.position = hit.point + hit.normal * 0.2f;
        }
        else
        {
            cam.position = camParent.position - camParent.forward * 7;
        }
    } 
}