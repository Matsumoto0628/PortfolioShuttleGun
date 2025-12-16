using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] Transform description;
    private void Start()
    {
        description.DOScale(1.5f, 0.75f)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            SceneManager.LoadScene("Stage");
        }
        
        Camera.main.transform.Rotate(0, Time.deltaTime, 0);
    }
}
