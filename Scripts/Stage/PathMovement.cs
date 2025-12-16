using UnityEngine;
using DG.Tweening;

public class PathMovement : MonoBehaviour
{
    private LineRenderer lineRenderer;
    [SerializeField] private Transform endPoint;
    private const float DURATION = 3f;

    private Transform parent;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.SetPosition(0, transform.localPosition);
        lineRenderer.SetPosition(1, endPoint.position);

        transform.DOLocalMove(endPoint.position, DURATION)
            .SetLoops(-1, LoopType.Yoyo);
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            parent = collision.transform.parent;
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(parent);
        }
    }
}