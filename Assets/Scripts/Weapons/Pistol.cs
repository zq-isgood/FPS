using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : WeaponBase
{
    public override void OnEnterPlayerState(PlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerState.Shoot:
                OnLeftAttack();  //������������������ֻ�û���ķ���
                break;
            case PlayerState.Reload: //��Ҳ㴫����״̬���ٵ��������������Ҫ������
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
                if (curr_BulletNum == 0 && curr_BulletNum < curr_MaxBulletNum && standby_BulletNum >0)
                {
                    owner.ChangePlayerState(PlayerState.Reload);  //���������Ҳ㴫�ݻ��ӵ���״̬
                    return;
                }

                // 2.�ӵ�û���꣬����R����
                if (standby_BulletNum > 0 && curr_BulletNum < curr_MaxBulletNum && Input.GetKeyDown(KeyCode.R))
                {
                    owner.ChangePlayerState(PlayerState.Reload);
                    return;
                }

                // ������
                if (canAttack && curr_BulletNum>0&&Input.GetMouseButton(0))
                {
                    owner.ChangePlayerState(PlayerState.Shoot);
                }
                break;
        }
    }
}

