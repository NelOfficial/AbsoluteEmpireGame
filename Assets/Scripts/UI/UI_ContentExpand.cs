using UnityEngine;

public class UI_ContentExpand : MonoBehaviour
{
    private Animator animator;
    private bool _expanded = false;


    private void Awake()
    {
        animator = this.GetComponent<Animator>();
        _expanded = false;
    }

    public void ToggleContent()
    {
        if (!_expanded)
        {
            animator.Play("contentExpand");
        }
        else if (_expanded)
        {
            animator.Play("contentCollapse");
        }
        _expanded = !_expanded;
    }
}
