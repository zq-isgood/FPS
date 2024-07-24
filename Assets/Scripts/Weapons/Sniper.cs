using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sniper : WeaponBase
{
    // �ѻ���UI
    [SerializeField] GameObject UISight;
    // ��Ⱦ��
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

                // ���ӵ�
                // ͨ��������ӵ���������
                // 1.�ӵ����� && �ӵ��������� &&�����б����ӵ�
                if (curr_BulletNum == 0 && curr_BulletNum < curr_MaxBulletNum && standby_BulletNum > 0)
                {
                    owner.ChangePlayerState(PlayerState.Reload);
                    return;
                }

                // 2.�ӵ�û���꣬����R����
                if (standby_BulletNum > 0 && curr_BulletNum < curr_MaxBulletNum && Input.GetKeyDown(KeyCode.R))
                {
                    owner.ChangePlayerState(PlayerState.Reload);
                    return;
                }

                // ������
                if (canAttack && curr_BulletNum > 0 && Input.GetMouseButtonDown(0))
                {
                    owner.ChangePlayerState(PlayerState.Shoot);
                    return;
                }

                // ��⿪��
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
        // �ָ�������ľ���
        owner.SetCameraFOV(60);
    }

    #region �����¼�

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
        // ����������Ⱦ��
        foreach (var item in Renders)
        {
            item.SetActive(false);
        }
        yield return new WaitForSeconds(0.1f);
        UISight.SetActive(true);
        // ���������
        owner.SetCameraFOV(30);
    }

    private void ShootEnd()
    {
        // ���һ���ӵ������
        // û�ӵ��������б����ӵ�
        if (curr_BulletNum == 0)
        {
            // ���ӵ������
            if (standby_BulletNum > 0)
            {
                owner.ChangePlayerState(PlayerState.Reload);
            }
            // û�е����
            else
            {
                owner.ChangePlayerState(PlayerState.Move);
            }
            if (wantShootEF) shootEF.SetActive(false);
        }
    }
    #endregion

}