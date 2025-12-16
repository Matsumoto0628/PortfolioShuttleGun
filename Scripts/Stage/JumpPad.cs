using Cysharp.Threading.Tasks;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    private const float FORCE_H = 0;
    private const float FORCE_V = 35f;
    private const float FORCE_H_CROUCH = 15f;
    private const float FORCE_V_CROUCH = 20f;

    private const float MAX_FORCE_H = 20f;
    private const float MAX_FORCE_H_CROUCH = 30f;

    Rigidbody playerRb;
    PlayerMovement pm;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerRb = collision.gameObject.GetComponent<Rigidbody>();
            pm = collision.gameObject.GetComponent<PlayerMovement>();

            pm.EnableDoubleJump().Forget();

            float fixedForceH = Mathf.Clamp(pm.FlatVel.magnitude + FORCE_H, 0, MAX_FORCE_H);
            float fixedForceHCrouch = Mathf.Clamp(pm.FlatVel.magnitude + FORCE_H_CROUCH, 0, MAX_FORCE_H_CROUCH);

            playerRb.velocity = Vector3.zero;
            
            if (pm.State == PlayerMovement.MovementState.Crouching ||
                pm.State == PlayerMovement.MovementState.Sliding)
            {
                AddJumpForce(fixedForceHCrouch, FORCE_V_CROUCH);
            }
            else
            {
                AddJumpForce(fixedForceH, FORCE_V);
            }
        }
    }

    private void AddJumpForce(float forceH, float forceV)
    {
        Vector3 jumpForce = pm.FlatVel.normalized * forceH + Vector3.up * forceV;
        playerRb.AddForce(jumpForce, ForceMode.Impulse);
    }
}
