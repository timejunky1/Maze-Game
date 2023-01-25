using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharecterController : MonoBehaviour
{
    [SerializeField] InputAction movement;

    [SerializeField] float movementSpeed = 1;
    [SerializeField] float jumpHeight = 1;
    [SerializeField, Range(1, 2)] int use = 1;
    Rigidbody rb;
    // Start is called before the first frame update
    private void OnEnable()
    {
        movement.Enable();
    }
    private void OnDisable()
    {
        movement.Disable();
    }

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void FixedUpdate()
    {
        if (use == 1) { MoveCharecterType1(); } else { MoveCharecterType2(); }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    void MoveCharecterType1()
    {
        if (movement.ReadValue<Vector3>().x > 0 || movement.ReadValue<Vector3>().z > 0)
        {
            float camRotationY = Camera.main.transform.rotation.eulerAngles.y;
            transform.localRotation = Quaternion.Euler(0, camRotationY, 0);
        }
        rb.velocity = movement.ReadValue<Vector3>().z * DirFromAngle(Camera.main.transform.rotation.eulerAngles.y, true) * movementSpeed +
        movement.ReadValue<Vector3>().x * DirFromAngle(Camera.main.transform.localRotation.eulerAngles.y + 90, true) * movementSpeed / 2 +
        movement.ReadValue<Vector3>().y * Vector3.up * movementSpeed;
    }

    void MoveCharecterType2()
    {
        Vector3 direction = movement.ReadValue<Vector3>().z * DirFromAngle(Camera.main.transform.localRotation.eulerAngles.y, true) +
        movement.ReadValue<Vector3>().x * DirFromAngle(Camera.main.transform.localRotation.eulerAngles.y + 90, true) +
        movement.ReadValue<Vector3>().y * Vector3.up;
        if (movement.ReadValue<Vector3>().x != 0 || movement.ReadValue<Vector3>().z != 0)
        {
            transform.LookAt(transform.position + new Vector3(direction.x * 10, 1.5f, direction.z * 10));
        }
        rb.velocity = direction * movementSpeed;


    }
}
