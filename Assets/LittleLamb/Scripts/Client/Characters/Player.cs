using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LittleLamb.Player
{
  public class Player : MonoBehaviour
  {
    private Animator anim;

    private CharacterController controller;

    [Range(1f, 20f)]
    public float movementSpeed = 5.0f;

    [Range(1f, 20f)]
    public float sprintSpeed = 7.0f;

    [Range(1f, 20f)]
    public float jumpSpeed = 10f;
    public float turnSpeed = 400.0f;
    private Vector3 moveDirection = Vector3.zero;
    public float gravity = 20.0f;

    // TODO add "coyote hang"
    // On ground, set counter to 0, on jump, if counter < hangTime && yVelocity < 0, allow jump
    // Allows jumping shortly after freefall state
    public float hangTime = 10f;

    float yVelocity = 0f;

    private bool bufferedJump = false;

    void Start()
    {
      controller = GetComponent<CharacterController>();
      anim = gameObject.GetComponentInChildren<Animator>();
    }

    void Update()
    {
      AimTowardsMouse();
      CheckJump();
      Move();
    }

    void AimTowardsMouse()
    {
      var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity))
      {
        var direction = hitInfo.point - transform.position;
        // Ignore if mouse over self
        if (hitInfo.collider.gameObject == gameObject) return;

        direction.y = 0f;
        direction.Normalize();
        transform.forward = direction;
      }
    }

    void CheckJump()
    {
      if (Input.GetButtonUp("Jump"))
      {
        bufferedJump = false;
        return;
      }
      if (Input.GetButtonDown("Jump"))
      {
        bufferedJump = true;
      }
    }

    void Move()
    {
      float horizontal = Input.GetAxis("Horizontal");
      float vertical = Input.GetAxis("Vertical");

      Vector3 movement = new Vector3(horizontal, 0f, vertical);

      bool isRunning = Input.GetKey(KeyCode.LeftShift);

      movement = Vector3.ClampMagnitude(movement, 1f);

      movement *= isRunning ? sprintSpeed : movementSpeed;

      if (controller.isGrounded)
      {
        yVelocity = -gravity * Time.deltaTime;

        if (bufferedJump)
        {
          yVelocity = jumpSpeed;
          bufferedJump = false;
          // anim.SetFloat("Velocity", 0f); // TODO add jump animation?
        }
      }

      yVelocity -= gravity * Time.deltaTime;

      // Not jumping
      if (controller.isGrounded && yVelocity <= 0)
      {
        // Not moving
        if (movement.magnitude <= 0)
        {
          anim.SetFloat("Velocity", 0f, 0.1f, Time.deltaTime);
          return;
        }
        anim.SetFloat("Velocity", isRunning ? 1f : 0.5f, 0.2f, Time.deltaTime);
      }

      movement.y = yVelocity;
      controller.Move(movement * Time.deltaTime);

    }
  }
}
