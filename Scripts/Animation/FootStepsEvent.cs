using UnityEngine;

public class FootStepsEvent : MonoBehaviour
{
    [SerializeField] private Transform spine;
    [SerializeField] private Transform leftFoot;
    [SerializeField] private Transform rightFoot;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioSource audioSource;
    private const float RAY_DISTANCE = 0.2f;
    private const float SPAN = 0.15f;
    private bool leftGrounded;
    private bool rightGrounded;
    private float lastSoundTime;

    void Update()
    {
        CheckFoot(leftFoot, ref leftGrounded);
        CheckFoot(rightFoot, ref rightGrounded);
    }

    void CheckFoot(Transform foot, ref bool wasGrounded)
    {
        if (foot == null)
            return;

        Vector3 direction = (foot.position - spine.position).normalized;
        RaycastHit hit;
        if (Physics.Raycast(foot.position, direction, out hit, RAY_DISTANCE))
        {
            if (!wasGrounded && Time.time - lastSoundTime > SPAN)
            {
                PlayFootstep();
                lastSoundTime = Time.time;
            }

            wasGrounded = true;
        }
        else
        {
            wasGrounded = false;
        }
    }

    void PlayFootstep()
    {
        audioSource.PlayOneShot(audioClip);
    }
}
