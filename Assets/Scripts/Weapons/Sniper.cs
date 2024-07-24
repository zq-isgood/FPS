using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sniper : WeaponBase
{
    // 狙击镜UI
    [SerializeField] GameObject UISight;
    // 渲染器
    [SerializeField] GameObject[] Renders;
    private bool isAim = false;
    public override void OnEnterPlayerState(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Shoot:
                if (isAim)
                {
                    StopAim();
                }
                OnLeftAttack();
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

                // 换子弹
                // 通用情况：子弹不是满的
                // 1.子弹打完 && 子弹不是满的 &&但是有备用子弹
                if (curr_BulletNum == 0 && curr_BulletNum < curr_MaxBulletNum && standby_BulletNum > 0)
                {
                    owner.ChangePlayerState(PlayerState.Reload);
                    return;
                }

                // 2.子弹没打完，但是R键换
                if (standby_BulletNum > 0 && curr_BulletNum < curr_MaxBulletNum && Input.GetKeyDown(KeyCode.R))
                {
                    owner.ChangePlayerState(PlayerState.Reload);
                    return;
                }

                // 检测射击
                if (canAttack && curr_BulletNum > 0 && Input.GetMouseButtonDown(0))
                {
                    owner.ChangePlayerState(PlayerState.Shoot);
                    return;
                }

                // 检测开镜
                if (canAttack && Input.GetMouseButtonDown(1))
                {
                    isAim = !isAim;
                    if (isAim)
                    {
                        StartAim();
                    }
                    else
                    {
                        StopAim();
                    }
                }
                
                break;
        }
    }

    private void StartAim()
    {
        animator.SetBool("Aim", true);
        wantShootEF = false;
    }

    private void StopAim()
    {
        foreach (var item in Renders)
        {
            item.SetActive(true);
        }
        animator.SetBool("Aim", false);
        wantShootEF = true;
        UISight.SetActive(false);
        // 恢复摄像机的距离
        owner.SetCameraFOV(60);
    }

    #region 动画事件

    private void StartLoad()
    {
        PlayAudio(3);
    }

    private void AimOver()
    {
        StartCoroutine(DoAim());
    }

    IEnumerator DoAim()
    {
        // 隐藏所有渲染器
        foreach (var item in Renders)
        {
            item.SetActive(false);
        }
        yield return new WaitForSeconds(0.1f);
        UISight.SetActive(true);
        // 摄像机拉近
        owner.SetCameraFOV(30);
    }

    private void ShootEnd()
    {
        // 最后一发子弹的情况
        // 没子弹，并且有备用子弹
        if (curr_BulletNum == 0)
        {
            // 有子弹的情况
            if (standby_BulletNum > 0)
            {
                owner.ChangePlayerState(PlayerState.Reload);
            }
            // 没有的情况
            else
            {
                owner.ChangePlayerState(PlayerState.Move);
            }
            if (wantShootEF) shootEF.SetActive(false);
        }
    }
    #endregion

}