using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour, IDeath {

    [Range(0.0f, 20.0f)]
    public float speed = 3;
    public ObjectPooler bulletPool;
    public Transform bulletSpawn;
    [Range(0.0f, 5.0f)]
    public float minYPosition = -3.5f;
    [Range(0.0f, 5.0f)]
    public float maxYposition = 1.2f;
    //[Range(0.0f, 1.0f)]
    //public float attackRate = .2f;
    public Animator animator;
    public GameObject tears;
    public AudioSource audioSource;
    public AudioClip shootClip;
    public AudioClip[] hitClips;
    public SpriteRenderer planeBody;
    public SpriteRenderer planeCanon;
    public LayerMask playerLayerMask;
    public PlayerHitPoint hitPoint;

    private Rigidbody2D rb;
    private float movement;
    //private float timer;
    private bool canAttack;
    //private bool canMove;
    private bool gameOver;
    private float pointA = .2f;
    private float pointB = -.2f;
    private bool goingUp;
    private Camera cam;
    private bool isReadyToDrag = false;
    private Vector2 touchPosition;

    const string PLAYER_TAG = "Player";
    static readonly int attackID = Animator.StringToHash("attack");

    #region Singleton

    public static Player instance;

    private void Awake() {

        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        rb = GetComponent<Rigidbody2D>();
        //timer = attackRate;
        canAttack = true;
        //canMove = true;
        gameOver = true;

        //pointA = new Vector3(transform.position.x, transform.position.y + .4f);
        //pointB = new Vector3(transform.position.x, transform.position.y - .4f);
    }

    #endregion /Singleton

    #region Execution Order

    private void Start() {
        cam = Camera.main;
        bulletPool = GameManager.instance.playerBullet;
        GameManager.instance.OnGameStart += StartGame;

        SetPlaneDetails(UserDataManager.Instance.userData.planeIndex);
    }

    private void Update() {

        if (gameOver) return;

        MoveCharacter();

#if UNITY_EDITOR
        movement = Input.GetAxisRaw("Vertical") * speed;      
#endif
    }

    private void FixedUpdate() {

        if(gameOver) {
            AutoAnimation();
            return;
        }

#if UNITY_EDITOR
        rb.MovePosition(rb.position + new Vector2(0, movement * Time.fixedDeltaTime ));
#endif
    }

    private void OnTriggerEnter2D(Collider2D collision) {

        if (!gameOver) return;

        audioSource.PlayOneShot(hitClips[Random.Range(0, hitClips.Length)]);
        //Debug.Log(collision.name);
    }

    private void OnDestroy() {
        GameManager.instance.OnGameStart -= StartGame;
    }

    #endregion /Execution Order

    public void MoveCharacter() {

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) {

            RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, playerLayerMask);

            if (!hit.collider) return;

            if (hit.collider.CompareTag(PLAYER_TAG))
                isReadyToDrag = true;
            else
                isReadyToDrag = false;

        } else if (Input.GetMouseButtonUp(0)) {
            isReadyToDrag = false;
        }
#else
        if (Input.touchCount > 0) {
            Debug.Log("Touch detected");
            Touch myTouch = Input.touches[0];

            if (myTouch.phase == TouchPhase.Began) {

                RaycastHit2D hit = Physics2D.Raycast(cam.ScreenToWorldPoint(myTouch.position), Vector2.zero, playerLayerMask);
                touchPosition = myTouch.position;

                if (!hit.collider) return;

                if (hit.collider.CompareTag(PLAYER_TAG))
                    isReadyToDrag = true;
                else
                    isReadyToDrag = false;

            } else if (myTouch.phase == TouchPhase.Ended || myTouch.phase == TouchPhase.Canceled) {
                isReadyToDrag = false;
            } else {
                touchPosition = myTouch.position;
            }
        }
#endif

        if (!isReadyToDrag) return;

#if UNITY_EDITOR
        Vector2 currentPosition = cam.ScreenToWorldPoint(Input.mousePosition);
#else
        Vector2 currentPosition = cam.ScreenToWorldPoint(touchPosition);
#endif

        transform.position = new Vector3(transform.position.x, currentPosition.y, transform.position.z);

        if(transform.position.y < minYPosition)
            transform.position = new Vector3(transform.position.x, minYPosition, transform.position.z);
        else if (transform.position.y > maxYposition)
            transform.position = new Vector3(transform.position.x, maxYposition, transform.position.z);
    }

    public bool PlayerAttack() {

        if (!canAttack) return false; 

        animator.SetTrigger(attackID);
        canAttack = false;
        audioSource.PlayOneShot(shootClip);
        //canMove = false;

        return true;
    }

    public void ReleaseAttack() {
        bulletPool.GetPooledObject(bulletSpawn.position, Quaternion.identity);
        //timer = 0;        
    }

    public void AttackComplete() {
        canAttack = true;
        //canMove = true;
    }

    private void AutoAnimation() {

        if(!goingUp) {

            rb.MovePosition(rb.position + new Vector2(0, -1 * Time.fixedDeltaTime));

            if (rb.position.y <= pointB) goingUp = true;
        }
            
        else {

            rb.MovePosition(rb.position + new Vector2(0, 1 * Time.fixedDeltaTime));

            if (rb.position.y >= pointA) goingUp = false;
        }            
    }

    public void SetPlaneDetails(int index) {
        ShopManager.instance.GetPlaneDetails(planeBody, planeCanon, index );
    }

    private void StartGame() {
        gameOver = false;
    }

    public void Revive() {
        StartGame();
        hitPoint.OnRiveve();
        tears.SetActive(false);
    }

    public void OnDeath() {

        if (gameOver) return;

        pointA = transform.position.y + .2f; 
        pointB = transform.position.y - .2f;
        gameOver = true;
        GameManager.instance.EndGame();
        tears.SetActive(true);
        //Debug.Log("Player Is Dead!");
    }
}
