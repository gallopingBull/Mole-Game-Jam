using UnityEngine;

public class HideComponent : MonoBehaviour
{
    private bool _isVisible = true;
    //private float _staminaCost = .25f;
    public GameObject _meshRenderer;
    [SerializeField]
    private GameObject _mound;
    private Animator _animator;

    public bool IsVisible { get => _isVisible; set => _isVisible = value; }

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    public void Hide()
    {
        _animator.SetTrigger("DigInit");
        gameObject.layer = LayerMask.NameToLayer("Stealth");
        _meshRenderer.SetActive(false);
        _mound.SetActive(true);
        _isVisible = false;
        //  change speed 
    }

    public void UnHide()
    {
        Debug.Log("Unhide being ");
        _meshRenderer.SetActive(true);
        _mound.SetActive(false);
        _isVisible = true;
        gameObject.layer = LayerMask.NameToLayer("Default");
        //  change speed
    }
}
