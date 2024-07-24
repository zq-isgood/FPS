using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家武器基类
/// </summary>
public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected Animator animator;
    [SerializeField] AudioSource audioSource;
    public Player_Controller owner;
    #region 子弹
    protected int curr_BulletNum;           // 当前子弹数量
    public int curr_MaxBulletNum;           // 当前子弹上限
    protected int standby_BulletNum;        // 备用子弹数量
    public int standby_MaxBulletNum;        // 备用子弹上限
    #endregion

    #region 射击参数
    protected bool canAttack = false;        // 能否攻击
    public int attackValue;                 // 攻击力
    public bool wantCrosshair;               // 是否有准星
    public bool wantBullet;                  // 是否有子弹
    public bool wantRecoil;                 // 是否有后坐力
    public float recoilStrength;            // 后坐力强度
    public bool canThroughWall = false;     // 是否能够穿墙
    #endregion

    #region 效果
    public AudioClip[] audioClips;
    public GameObject[] bulletPrefab;
    // 需要射击火花粒子
    protected bool wantShootEF;
    // 射击时的火花，直接显示或隐藏
    public GameObject shootEF;
    #endregion

    // 这把武器切换玩家状态时要做的事情
    public abstract void OnEnterPlayerState(PlayerState playerState);
    // 这把武器在不同的玩家状态下要做的事情
    public abstract void OnUpdatePlayerState(PlayerState playerState);


    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init(Player_Controller player_Controller)
    {
        owner = player_Controller;
        // 初始化子弹
        curr_BulletNum = curr_MaxBulletNum;
        standby_BulletNum = standby_MaxBulletNum;
        wantShootEF = shootEF != null;
    }

    private bool wantReloadOnEnter = false;
    /// <summary>
    /// 进入武器
    /// </summary>
    public virtual void Enter()
    { 
        gameObject.SetActive(true); // Animator自动播放默认动画 Enter
        owner.ChangePlayerState(PlayerState.Move);
        canAttack = false;  // 让玩家无法开火
        // 初始化 是否要子弹 是否要准星
        owner.InitForEnterWeapon(wantCrosshair, wantBullet);
        // 更新子弹
        if (wantBullet)
        {
            owner.UpdateBulletUI(curr_BulletNum, curr_MaxBulletNum, standby_BulletNum);
            // 如果有子弹 才上膛
            if (curr_BulletNum>0)
            {
                PlayAudio(0);
            }
            // 没子弹，直接进入换弹状态
            else
            {
                wantReloadOnEnter = true;
            }
        }
        else
        {
            PlayAudio(0);
        }

        // 重置射击特效物体
        if (wantShootEF) shootEF.SetActive(false);
        
    }

    private Action onExitOver;
    /// <summary>
    /// 退出武器
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
    /// 鼠标左键攻击
    /// </summary>
    protected virtual void OnLeftAttack()
    {
        // 需不需要子弹
        if (wantBullet)
        {
            curr_BulletNum--;
            owner.UpdateBulletUI(curr_BulletNum, curr_MaxBulletNum, standby_BulletNum);
        }
        canAttack = false;
        // 射击动画
        animator.SetTrigger("Shoot");
        // 火花
        if (wantShootEF) shootEF.SetActive(true);
        // 后坐力表现
        if (wantRecoil) owner.StartShootRecoil(recoilStrength);
        // 音效
        PlayAudio(1);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (canThroughWall)
        {
            // 穿墙
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
            // 攻击效果
            GameObject go = Instantiate(bulletPrefab[1], hitInfo.point, Quaternion.identity);
            go.transform.LookAt(Camera.main.transform);
            // 僵尸逻辑
            ZombieController zombie = hitInfo.collider.GetComponent<ZombieController>();
            if (zombie == null) zombie = hitInfo.collider.gameObject.GetComponentInParent<ZombieController>();
            zombie.Hurt(attackValue);

        }
        else if (hitInfo.collider.gameObject!=owner.gameObject)
        {
            // 攻击效果
            GameObject go = Instantiate(bulletPrefab[0], hitInfo.point, Quaternion.identity);
            go.transform.LookAt(Camera.main.transform);
        }
    }


    #region 动画事件
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
        // 填充子弹
        // 子弹不够、子弹足够
        int want = curr_MaxBulletNum - curr_BulletNum;
        if ((standby_BulletNum - want)<0)
        {
            want = standby_BulletNum;
        }
        standby_BulletNum -= want;
        curr_BulletNum += want;

        // 更新UI
        owner.UpdateBulletUI(curr_BulletNum, curr_MaxBulletNum, standby_BulletNum);
        animator.SetBool("Reload", false);
        owner.ChangePlayerState(PlayerState.Move);
    }
    #endregion
}
