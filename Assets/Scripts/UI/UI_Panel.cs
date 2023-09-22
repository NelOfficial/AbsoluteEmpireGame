using UnityEngine;
using System.Collections;

public class UI_Panel : MonoBehaviour
{
    private Animator _panelAnimator;

    private bool _isActive = false;

    private void Awake()
    {
        _panelAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        PlayAnimation();
    }

    public void ClosePanel()
    {
        Close();

        StartCoroutine(ClosePanel_Co());
    }

    public void PlayAnimation()
    {
        _isActive = this.gameObject.activeSelf;

        if (_isActive)
        {
            Open();
        }
        else if (!_isActive)
        {
            Close();
        }
    }

    private void Close()
    {
        _panelAnimator.Play("close");
    }

    private void Open()
    {
        _panelAnimator.Play("open");
    }

    private IEnumerator ClosePanel_Co()
    {
        float animationDuration = 0.2f;

        yield return new WaitForSeconds(animationDuration);

        this.gameObject.SetActive(false);
        yield break;
    }
}
