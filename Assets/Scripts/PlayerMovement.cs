using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = System.Diagnostics.Debug;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private Sprite[] _sprites;
    
    private Animator _anim;
    // PLAYER HEALTH REF
    //private PlayerHealth _ph;
    
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;

    private Vector2 _moveDirection;
    private Vector3 _mousePos;
    
    void Awake()
    {
        // Assigning Refs
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponentInChildren<SpriteRenderer>();
        _anim = GetComponentInChildren<Animator>();
        // ASSIGN PLAYER HEALTH REF
        //_ph = GetComponent<PlayerHealth>();
    }
    
    void Update()
    {
        // Getting Movement From Inputs
        
        // IF PLAYER IS IN DOWNED STATE
        //if (_ph.IsDowned) return;
        _moveDirection.x = Input.GetAxisRaw("Horizontal");
        _moveDirection.y = Input.GetAxisRaw("Vertical");
        
        Aim();
    }

    private void FixedUpdate()
    {
        // Changing Direction Sprites
        if (_moveDirection != Vector2.zero)
        {
            /*switch (_moveDirection.x)
            {
                case < 0:
                    _sr.sprite = _moveDirection.y > 0 ? _sprites[0] : _sprites[3];
                    break;
                case > 0:
                    _sr.sprite = _moveDirection.y > 0 ? _sprites[2] : _sprites[5];
                    break;
                default:
                    _sr.sprite = _moveDirection.y > 0 ? _sprites[1] : _sprites[4];
                    break;
            }*/
        }
        
        // STOP ANIM WHEN DOWNED
        //if (!_ph.IsDowned)
        //{
            _anim.SetBool("isMoving", _moveDirection != Vector2.zero);
        //}

        // Actual Movement
        _rb.MovePosition(_rb.position + _moveDirection * _moveSpeed * Time.fixedDeltaTime);
    }
    
    private void Aim()
    {
        // Mouse position from screen to world point
        _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _mousePos.z = 0f;

        // Aiming calculations
        Vector3 aimDir = (_mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        
        // Flip sprite based on where you are aiming
        if (angle > 90 || angle < -90)
        {
            _sr.flipX = true;
        }
        else
        {
            _sr.flipX = false;
        }
    }
    
    /*public void Knockback(Vector2 force, float duration)
    {
        StartCoroutine(KnockbackStart(force, duration));
    }*/
    
    // Player can't move while knocked back
    // Red is when player can't move, magenta is for iFrames since that code turns the sprite white again
    /*private IEnumerator KnockbackStart(Vector2 force, float duration)
    {
        _isKnockback = true;
        _rb.AddForce(force * 10, ForceMode2D.Impulse);
        _sr.color = Color.red;
        yield return new WaitForSeconds(duration);
        _sr.color = Color.white;
        _isKnockback = false;
    }*/
    
    private void PlayerDowned()
    {
        _rb.constraints = RigidbodyConstraints2D.FreezeAll;
        _anim.SetBool("isMoving", false);
    }
}
