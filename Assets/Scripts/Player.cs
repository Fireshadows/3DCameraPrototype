using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float m_moveSpeed = 10;
    public float m_jumpForce = 10;

    public CharacterController m_controller;

    private Vector3 m_moveDirection;
    public float m_gravityScale = 1;

    public Animator m_anim;

    public float m_turnSpeed = 10;

    private Transform m_model;

    [SerializeField]
    Transform m_playerInputSpace = default;

    private bool m_wasGrounded = true;

    void Start()
    {
        m_controller = GetComponent<CharacterController>();
        m_model = transform.GetChild(0);
    }

    private void Update()
    {
        float m_yStore = m_moveDirection.y;
        Vector2 m_playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        m_moveDirection = (transform.forward * m_playerInput.y) + (transform.right * m_playerInput.x);
        m_moveDirection = m_moveDirection.normalized * m_moveSpeed;
        m_moveDirection.y = m_yStore;

        if (m_controller.isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                m_moveDirection.y = m_jumpForce;
            }
        }
        else
            m_moveDirection.y = m_moveDirection.y + (Physics.gravity.y * m_gravityScale * Time.deltaTime);

        //On Landed
        if (!m_wasGrounded && m_controller.isGrounded)
        {

            Debug.Log("Landed");
        }
        //On Airborne
        else if (m_wasGrounded && !m_controller.isGrounded)
        {
            if (!Input.GetButton("Jump"))
            {
                m_moveDirection.y = -1.0f;
                Debug.Log("Falling");
            }
            else
                Debug.Log("Jumped");
        }
        if (m_wasGrounded != m_controller.isGrounded)
            m_wasGrounded = m_controller.isGrounded;

        m_controller.Move(m_moveDirection * Time.deltaTime);

        //Move the player in different directions based on camera look direction
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            if (m_playerInputSpace)
            {
                transform.rotation = Quaternion.Euler(0f, m_playerInputSpace.rotation.eulerAngles.y, 0);
                Quaternion m_newRotation = Quaternion.LookRotation(new Vector3(m_moveDirection.x, 0f, m_moveDirection.z));
                m_model.rotation = Quaternion.Slerp(m_model.rotation, m_newRotation, m_turnSpeed * Time.deltaTime);
            }
        }

        m_anim.SetBool("IsGrounded", m_controller.isGrounded);
        m_anim.SetFloat("Speed", (Mathf.Abs(Input.GetAxisRaw("Vertical")) + Mathf.Abs(Input.GetAxisRaw("Horizontal"))));
    }
}
