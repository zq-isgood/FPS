using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knife : WeaponBase
{
    private bool isLeftAttack = true;
    [SerializeField] Knife_Collider knife_Collider;

    public override void Init(Player_Controller player_Controller)
    {
        base.Init(player_Controller);
        knife_Collider.Init(this);
    }

    public override void OnEnterPlayerState(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Shoot:
                if (isLeftAttack)
                {
                    OnLeftAttack();
                }
                else
                {
                    OnRightAttack();
                }
                break;
            case PlayerState.Reload:
                PlayAudio(2);
                animator.SetBool("Reload", true);
                break;
        }
    }

    public override void OnUpdatePlayerState(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Move:
                // ×ó¼ü¹¥»÷
                if (canAttack && Input.GetMouseButton(0))
                {
                    isLeftAttack = true;
                    owner.ChangePlayerState(PlayerState.Shoot);
                    return;
                }

                // ÓÒ¼ü¹¥»÷
                if (canAttack && Input.GetMouseButton(1))
                {
                    isLeftAttack = false;
                    owner.ChangePlayerState(PlayerState.Shoot);
                }
                break;
        }
    }

    protected override void OnLeftAttack()
    {
        PlayAudio(1);
        animator.SetTrigger("Shoot");
        animator.SetBool("IsLeftAttack", true);
        knife_Collider.StartAttack();
    }
    private void OnRightAttack()
    {
        PlayAudio(1);
        animator.SetTrigger("Shoot");
        animator.SetBool("IsLeftAttack", false);
        knife_Collider.StartAttack();
    }

    public void HitTarget(GameObject hitObj,Vector3 hitPoint)
    {
        if (hitObj.CompareTag("Zombie"))
        {
            PlayAudio(2);
            // ¹¥»÷Ð§¹û
            GameObject go = Instantiate(bulletPrefab[1], hitPoint, Quaternion.identity);
            go.transform.LookAt(Camera.main.transform);
            // ½©Ê¬Âß¼­
            ZombieController zombie = hitObj.GetComponent<ZombieController>();
            if (zombie == null) zombie = hitObj.gameObject.GetComponentInParent<ZombieController>();
            if (isLeftAttack)
            {
                zombie.Hurt(attackValue);
            }
            else
            {
                zombie.Hurt(attackValue * 4);
            }
        }
        else if (hitObj.gameObject != owner.gameObject)
        {
            // ¹¥»÷Ð§¹û
            GameObject go = Instantiate(bulletPrefab[0], hitPoint, Quaternion.identity);
            go.transform.LookAt(Camera.main.transform);
        }

    }

    #region ¶¯»­ÊÂ¼þ
    protected override void ShootOver()
    {
        base.ShootOver();
        knife_Collider.StopAttack();
    }
    #endregion
}
