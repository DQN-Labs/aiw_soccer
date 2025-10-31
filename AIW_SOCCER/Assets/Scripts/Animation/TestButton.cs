using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }
    // when the mouse hovering on the button
    public void OnPointerEnter(PointerEventData eventData)
    {
        animator.SetBool("isHovering", true);
    }
    // mouse out of the button
    public void OnPointerExit(PointerEventData eventData)
    {
        animator.SetBool("isHovering", false);
    }
    // click
    public void OnPointerDown(PointerEventData eventData)
    {
        animator.SetTrigger("Pressed");
    }
}
