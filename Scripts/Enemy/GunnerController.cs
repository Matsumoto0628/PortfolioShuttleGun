using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using UniRx;

namespace Gunner
{
    public enum State
    {
        Patrolling,
        Tracking,
        Attacking
    }

    public class GunnerController : MonoBehaviour
    {
        private FSM<State, GunnerController> fsm;
        private NavMeshAgent agent;
        private Enemy enemy;
        [SerializeField] private Animator animator;
        [SerializeField] private Transform body;
        [SerializeField] private GunHolder gunHolder;
        [SerializeField] private RigBuilder rigBuilder;
        [SerializeField] private Gun gun;
        [SerializeField] private Transform aim;
        [SerializeField] private Transform aimHolder;

        private Transform target;

        private void Start()
        {
            target = GameObject.FindWithTag("Player").transform;
            agent = GetComponent<NavMeshAgent>();
            enemy = GetComponent<Enemy>();

            enemy.OnDamage
                .Subscribe(_ =>
                {
                    if (fsm.Current == State.Patrolling)
                    {
                        fsm.Transit(State.Tracking);
                    }
                });

            gunHolder.SetOnTransit(() =>
            {
                rigBuilder.Build();
                rigBuilder.Evaluate(Time.deltaTime);
            });

            fsm = new FSM<State, GunnerController>(this, new IState<State, GunnerController>[]
            {
                new Patrolling(),
                new Tracking(),
                new Attacking()
            });
            fsm.Transit(State.Patrolling);
        }

        private void Update()
        {
            fsm.Update();
            AnimationHandler();
        }

        private void AnimationHandler()
        {
            Vector3 localVel = body.InverseTransformDirection(agent.velocity);
            animator.SetFloat("velocityX", localVel.x, 0.1f, Time.deltaTime);
            animator.SetFloat("velocityY", localVel.y, 0.1f, Time.deltaTime);
            animator.SetFloat("velocityZ", localVel.z, 0.1f, Time.deltaTime);

            animator.SetBool("isGround", true);
        }

        [SerializeField] private Transform[] points;
        [SerializeField] private Transform eye;
        private const float SAMPLE_RANGE = 100f;
        private const float DETECT_RANGE = 25f;
        private const float SHOOT_RANGE = 15f;
        private void SetDestination(Transform destination)
        {
            if (destination != null)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(destination.position, out hit, SAMPLE_RANGE, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            }
        }
        private bool IsDestination()
        {
            return !agent.pathPending
                && agent.remainingDistance <= agent.stoppingDistance
                && (!agent.hasPath || agent.velocity.sqrMagnitude == 0);
        }
        private bool IsRange(float range)
        {
            if (Vector3.Distance(target.position, transform.position) <= range)
            {
                Vector3 toSelf = transform.position - target.position;
                Vector3 axis = Vector3.Cross(transform.forward, toSelf);
                float viewAngle = Vector3.Angle(transform.forward, toSelf) * (axis.y < 0 ? -1 : 1);
                viewAngle += 180;
                if (viewAngle < 45 || viewAngle > 315)
                {
                    Vector3 toTarget = target.position - transform.position;
                    float distance = toTarget.magnitude;
                    Vector3 direction = toTarget.normalized;
                    RaycastHit hit;
                    if (Physics.Raycast(eye.position, direction, out hit, distance))
                    {
                        if (hit.collider.CompareTag("Player"))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        private void GunHolderHandler(GunHoldState gunHoldState)
        {
            gunHolder.Transit(gunHoldState);
        }

        private class Patrolling : IState<State, GunnerController>
        {
            public State State => State.Patrolling;
            private int pointsIdx;
            private const float SPEED = 3.5f;

            public void Enter(GunnerController controller)
            {
                pointsIdx = 0;
                controller.SetDestination(controller.points[pointsIdx]);
                controller.agent.speed = SPEED;
            }

            public void Update(GunnerController controller)
            {
                if (controller.IsDestination())
                {
                    pointsIdx++;
                    if (pointsIdx >= controller.points.Length)
                    {
                        pointsIdx = 0;
                    }
                    controller.SetDestination(controller.points[pointsIdx]);
                    Stop(controller).Forget();
                }

                if (controller.IsRange(DETECT_RANGE))
                {
                    controller.fsm.Transit(State.Tracking);
                }

                controller.GunHolderHandler(GunHoldState.NEUTRAL);
            }

            public void Exit(GunnerController controller)
            {

            }

            private async UniTask Stop(GunnerController controller)
            {
                float speed = controller.agent.speed;
                controller.agent.speed = 0;
                await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: controller.GetCancellationTokenOnDestroy());
                controller.agent.speed = speed;
            }
        }

        private class Tracking : IState<State, GunnerController>
        {
            public State State => State.Tracking;

            private Tween rotateTween;
            private const float ROTATE_DURATION = 1f;
            private const float MISS_DURATION = 3f;
            private float missTimer;
            private const float SPEED = 7f;

            public void Enter(GunnerController controller)
            {
                missTimer = 0;
                controller.SetDestination(controller.target);
                controller.agent.speed = SPEED;
            }

            public void Update(GunnerController controller)
            {
                if (controller.IsRange(DETECT_RANGE))
                {
                    controller.SetDestination(controller.target);
                    CancelRotate();
                    missTimer = 0;
                }
                else if (controller.IsDestination())
                {
                    if (rotateTween == null)
                    {
                        rotateTween = controller.transform.DORotate(new Vector3(0, 360f, 0), ROTATE_DURATION, RotateMode.FastBeyond360)
                            .OnComplete(() => rotateTween = null);
                    }

                    missTimer += Time.deltaTime;
                    if (missTimer >= MISS_DURATION)
                    {
                        missTimer = 0;
                        controller.fsm.Transit(State.Patrolling);
                    }
                }

                if (controller.IsRange(SHOOT_RANGE))
                {
                    controller.fsm.Transit(State.Attacking);
                }

                controller.GunHolderHandler(GunHoldState.NEUTRAL);
            }

            public void Exit(GunnerController controller)
            {
                CancelRotate();
            }

            private void CancelRotate()
            {
                if (rotateTween != null)
                {
                    if (rotateTween.IsActive())
                    {
                        rotateTween.Kill();
                    }
                    rotateTween = null;
                }
            }
        }

        private class Attacking : IState<State, GunnerController>
        {
            public State State => State.Attacking;
            private const float SHOOT_SPAN = 1f;
            private float shootTimer;
            private const float BODY_ROTATION_STEP = 2f;
            private const float AIM_ROTATION_STEP = 3f;

            public void Enter(GunnerController controller)
            {
                shootTimer = 0;
                controller.agent.speed = 0;
            }

            public void Update(GunnerController controller)
            {
                shootTimer += Time.deltaTime;
                if (shootTimer >= SHOOT_SPAN)
                {
                    shootTimer = 0;
                    controller.gun.Fire(controller.aim, controller.eye);
                }

                if (!controller.IsRange(SHOOT_RANGE))
                {
                    controller.fsm.Transit(State.Tracking);
                }

                Quaternion lookRotation = Quaternion.LookRotation(controller.target.position - controller.transform.position, Vector3.up);
                controller.transform.rotation = Quaternion.Lerp(controller.transform.rotation, lookRotation, Time.deltaTime * BODY_ROTATION_STEP);
                controller.aimHolder.rotation = Quaternion.Lerp(controller.aimHolder.rotation, lookRotation, Time.deltaTime * AIM_ROTATION_STEP);
                
                controller.GunHolderHandler(GunHoldState.AIMING);
            }

            public void Exit(GunnerController controller)
            {

            }
        }
    }
}