using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �����������
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected Animator animator;
    [SerializeField] AudioSource audioSource;
    public Player_Controller owner;
    #region �ӵ�
    protected int curr_BulletNum;           // ��ǰ�ӵ�����
    public int curr_MaxBulletNum;           // ��ǰ�ӵ�����
    protected int standby_BulletNum;        // �����ӵ�����
    public int standby_MaxBulletNum;        // �����ӵ�����
    #endregion

    #region �������
    protected bool canAttack = false;        // �ܷ񹥻�
    public int attackValue;                 // ������
    public bool wantCrosshair;               // �Ƿ���׼��
    public bool wantBullet;                  // �Ƿ����ӵ�
    public bool wantRecoil;                 // �Ƿ��к�����
    public float recoilStrength;            // ������ǿ��
    public bool canThroughWall = false;     // �Ƿ��ܹ���ǽ
    #endregion

    #region Ч��
    public AudioClip[] audioClips;
    public GameObject[] bulletPrefab;
    // ��Ҫ���������
    protected bool wantShootEF;
    // ���ʱ�Ļ𻨣�ֱ����ʾ������
    public GameObject shootEF;
    #endregion

    // ��������л����״̬ʱҪ��������
    public abstract void OnEnterPlayerState(PlayerState playerState);
    // ��������ڲ�ͬ�����״̬��Ҫ��������
    public abstract void OnUpdatePlayerState(PlayerState playerState);


    /// <summary>
    /// ��ʼ��
    /// </summary>
    public virtual void Init(Player_Controller player_Controller)
    {
        owner = player_Controller;
        // ��ʼ���ӵ�
        curr_BulletNum = curr_MaxBulletNum;
        standby_BulletNum = standby_MaxBulletNum;
        wantShootEF = shootEF != null;
    }

    private bool wantReloadOnEnter = false;
    /// <summary>
    /// ��������
    /// </summary>
    public virtual void Enter()
    { 
        gameObject.SetActive(true); // Animator�Զ�����Ĭ�϶��� Enter
        owner.ChangePlayerState(PlayerState.Move);
        canAttack = false;  // ������޷�����
        // ��ʼ�� �Ƿ�Ҫ�ӵ� �Ƿ�Ҫ׼��
        owner.InitForEnterWeapon(wantCrosshair, wantBullet);
        // �����ӵ�
        if (wantBullet)
        {
            owner.UpdateBulletUI(curr_BulletNum, curr_MaxBulletNum, standby_BulletNum);
            // ������ӵ� ������
            if (curr_BulletNum>0)
            {
                PlayAudio(0);
            }
            // û�ӵ���ֱ�ӽ��뻻��״̬
            else
            {
                wantReloadOnEnter = true;
            }
        }
        else
        {
            PlayAudio(0);
        }

        // ���������Ч����
        if (wantShootEF) shootEF.SetActive(false);
        
    }

    private Action onExitOver;
    /// <summary>
    /// �˳�����
    /// </summary>
    public virtual void Exit(Action onExitOver)
    {
        animator.SetTrigger("Exit");
        this.onExitOver = onExitOver;
    }

    public void PlayAudio(int index)
    {
        audioSource.PlayOneShot(audioClips[index]);
    }

    /// <summary>
    /// ����������
    /// </summary>
    protected virtual void OnLeftAttack()
    {
        // �費��Ҫ�ӵ�
        if (wantBullet)
        {
            curr_BulletNum--;
            owner.UpdateBulletUI(curr_BulletNum, curr_MaxBulletNum, standby_BulletNum);
        }
        canAttack = false;
        // �������
        animator.SetTrigger("Shoot");
        // ��
        if (wantShootEF) shootEF.SetActive(true);
        // ����������
        if (wantRecoil) owner.StartShootRecoil(recoilStrength);
        // ��Ч
        PlayAudio(1);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (canThroughWall)
        {
            // ��ǽ
            RaycastHit[] hitInfos = Physics.RaycastAll(ray, 1500f);
            for (int i = 0; i < hitInfos.Length; i++)
            {
                HitGameObject(hitInfos[i]);
            }
        }
        else
        {
            if (Physics.Raycast(ray,out RaycastHit hitInfo,1500f))
            {
                HitGameObject(hitInfo);
            }
        }
    }

    protected void HitGameObject(RaycastHit hitInfo)
    {
        if (hitInfo.collider.CompareTag("Zombie"))
        {
            // ����Ч��
            GameObject go = Instantiate(bulletPrefab[1], hitInfo.point, Quaternion.identity);
            go.transform.LookAt(Camera.main.transform);
            // ��ʬ�߼�
            ZombieController zombie = hitInfo.collider.GetComponent<ZombieController>();
            if (zombie == null) zombie = hitInfo.collider.gameObject.GetComponentInParent<ZombieController>();
            zombie.Hurt(attackValue);

        }
        else if (hitInfo.collider.gameObject!=owner.gameObject)
        {
            // ����Ч��
            GameObject go = Instantiate(bulletPrefab[0], hitInfo.point, Quaternion.identity);
            go.transform.LookAt(Camera.main.transform);
        }
    }


    #region �����¼�
    private void EnterOver()
    {
        canAttack = true;
        if (wantReloadOnEnter)
        {
            owner.ChangePlayerState(PlayerState.Move);
        }
    }

    private void ExitOver()
    {
        gameObject.SetActive(false);
        onExitOver?.Invoke();
    }

    protected virtual void ShootOver()
    {
        canAttack = true;
        if (wantShootEF) shootEF.SetActive(false);
        if (owner.playerState == PlayerState.Shoot)
        {
            owner.ChangePlayerState(PlayerState.Move);
        }
    }

    private void ReloadOver()
    {
        // ����ӵ�
        // �ӵ��������ӵ��㹻
        int want = curr_MaxBulletNum - curr_BulletNum;
        if ((standby_BulletNum - want)<0)
        {
            want = standby_BulletNum;
        }
        standby_BulletNum -= want;
        curr_BulletNum += want;

        // ����UI
        owner.UpdateBulletUI(curr_BulletNum, curr_MaxBulletNum, standby_BulletNum);
        animator.SetBool("Reload", false);
        owner.ChangePlayerState(PlayerState.Move);
    }
    #endregion
}
