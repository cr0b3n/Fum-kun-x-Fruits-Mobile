using UnityEngine;

[DisallowMultipleComponent]
public class AttackButton : MonoBehaviour
{
    public Animator animator;
    private Player player;

    static readonly int shootID = Animator.StringToHash("shoot"); 

    private void Start() {
        GameManager.instance.OnGameStart += SetPlayer;
    }

#if UNITY_EDITOR
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            Attack();
        }
    }
#endif

    private void OnDestroy() {
        GameManager.instance.OnGameStart -= SetPlayer;
    }

    private void SetPlayer() {
        player = Player.instance;
    }

    public void Attack() {
        bool success = player.PlayerAttack();

        if(success) {
            animator.ResetTrigger(shootID);
            animator.SetTrigger(shootID);
        }       
    }
}
