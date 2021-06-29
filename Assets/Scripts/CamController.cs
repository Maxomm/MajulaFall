using UnityEngine;

namespace Assets.Scripts
{
    public class CamController : MonoBehaviour
    {
        private float mouseX, mouseY;
        [SerializeField] private float rotSpeed = 1;
        [SerializeField] private Transform target, player;

        // Start is called before the first frame update
        private void Start()
        {
            Cursor.visible = false; 
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Update is called once per frame
        private void Update()
        {
            CamControl();
        }

        private void CamControl()
        {
            mouseX += Input.GetAxis("Mouse X") * rotSpeed;
            mouseY -= Input.GetAxis("Mouse Y") * rotSpeed;

            mouseY = Mathf.Clamp(mouseY, -75, 75);

            transform.LookAt(target);

            target.rotation = Quaternion.Euler(mouseY, mouseX, 0);
        }
    }
}
