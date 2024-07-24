using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
/// <summary>
/// 玩家状态
/// </summary>
public enum PlayerState
{
    Move,
    Shoot,
    Reload
}

/// <summary>
/// 玩家控制器
/// </summary>
public class Player_Controller : MonoBehaviour
{
    public static Player_Controller Instance;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Camera[] cameras;
    [SerializeField] Image crossImage;
    [SerializeField] FirstPersonController firstPersonController;
    private int hp = 100;


    #region 武器相关
    [SerializeField] WeaponBase[] weapons;
    private int currWeaponIndex = -1;       // 当前武器
    private int previousWeaponIndex = -1;   // 上一个武器
    private bool canChangeWeapon = true;    // 能否切换武器
    #endregion

    public PlayerState playerState { get; private set; }

    private void Awake()
    {
        Instance = this;
        // 初始化武器
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].Init(this);  //在面板赋值
        }
        // 应用默认武器
        ChangeWeapon(2);
    }


    private void Update()
    {
        // 驱动武器层
        weapons[currWeaponIndex].OnUpdatePlayerState(playerState);  //每一帧都去检查，玩家状态一变，就会驱动武器层
        // 切枪检测
        if (canChangeWeapon == false) return;
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeWeapon(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeWeapon(2);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            // 如果存在上一把武器才能切换
            if (previousWeaponIndex >=0)
            {
                ChangeWeapon(previousWeaponIndex);
            }
        }

    }

    /// <summary>
    /// 修改玩家状态
    /// </summary>
    public void ChangePlayerState(PlayerState newState)
    {
        playerState = newState;
        // 通知回去修改状态
        weapons[currWeaponIndex].OnEnterPlayerState(playerState);
    }

    /// <summary>
    /// 切换武器
    /// </summary>
    private void ChangeWeapon(int newWeaponIndex)
    {
        // 是不是重复再按同一个键
        if (currWeaponIndex == newWeaponIndex) return;
        // 上一个武器的索引 = 当前武器的索引
        previousWeaponIndex = currWeaponIndex;
        // 记录新的武器索引
        currWeaponIndex = newWeaponIndex;

        // 如果是第一次使用武器 不存在退出武器的动画
        if (previousWeaponIndex < 0)
        {
            // 直接进入新武器
            weapons[currWeaponIndex].Enter();
        }
        // 当前手里是有武器
        else
        {
            // 退出当前武器,当这个武器退出成功后，才能进入新武器
            weapons[previousWeaponIndex].Exit(OnWeaponExitOver);
            canChangeWeapon = false;
        }
    }

    /// <summary>
    /// 当上一个武器成功退出后执行的Action
    /// </summary>
    private void OnWeaponExitOver()
    {
        // 直接进入新武器
        weapons[currWeaponIndex].Enter();
        canChangeWeapon = true;
    }

    /// <summary>
    /// 为新武器做初始化
    /// </summary>
    public void InitForEnterWeapon(bool wantCorsshair,bool wantBullet)
    {
        crossImage.gameObject.SetActive(wantCorsshair);
        UI_MainPanel.Instance.InitForEnterWeapon(wantBullet);
    }

    /// <summary>
    /// 更新子弹UI
    /// </summary>
    public void UpdateBulletUI(int curr_BulletNum,int curr_MaxBulletNum,int stanby_BulletNum)
    {
        UI_MainPanel.Instance.UpdateCurrBullet_Text(curr_BulletNum,curr_MaxBulletNum);
        UI_MainPanel.Instance.UpdateStandByBullet_Text(stanby_BulletNum);
    }


    #region 后坐力

    Coroutine shootRecoil_CrossCoroutine;
    Coroutine shootRecoil_CameraCoroutine;
    public void StartShootRecoil(float recoil = 1)
    {
        // 先暂停之前的
        if (shootRecoil_CrossCoroutine!=null) StopCoroutine(shootRecoil_CrossCoroutine);
        if (shootRecoil_CameraCoroutine != null) StopCoroutine(shootRecoil_CameraCoroutine);

        // 开启新的
        // 瞄准器的后坐力
        shootRecoil_CrossCoroutine = StartCoroutine(ShootRecoil_Cross(recoil));
        // 视角的后坐力
        shootRecoil_CameraCoroutine = StartCoroutine(ShootRecoil_Camera(recoil));
    }

    // 后坐力的瞄准器效果
    IEnumerator ShootRecoil_Cross(float recoil = 1)
    {
        Vector2 scale = crossImage.transform.localScale;
        // 放大
        while (scale.x < 1.3f * recoil)
        {
            yield return null;
            scale.x += Time.deltaTime * 3;
            scale.y = scale.x;
            crossImage.transform.localScale = scale;
        }
        // 缩小
        while (scale.x > 1)
        {
            yield return null;
            scale.x -= Time.deltaTime * 3;
            scale.y = scale.x;
            crossImage.transform.localScale = scale;
        }
        crossImage.transform.localScale = Vector2.one;
        yield break;
    }

    // 后坐力的相机效果
    IEnumerator ShootRecoil_Camera(float recoil = 1)
    {
        // 确认偏移量
        float xOffset = Random.Range(0.3f, 0.6f) * recoil;
        float yOffset = Random.Range(-0.15f, 0.15f) * recoil;
        firstPersonController.xRotOffset = xOffset;
        firstPersonController.yRotOffset = yOffset;
        // 等待6帧
        for (int i = 0; i < 6; i++)
        {
            yield return null;
        }
        // 归位
        firstPersonController.xRotOffset = 0;
        firstPersonController.yRotOffset = 0;
    }

    #endregion

    /// <summary>
    /// 受伤
    /// </summary>
    public void Hurt(int damage)
    {
        hp = Mathf.Clamp(hp - damage, 0, 100);
        UI_MainPanel.Instance.UpdateHP_Text(hp);
    }

    public void SetCameraFOV(int value)
    {
        cameras[0].fieldOfView = value;
        cameras[1].fieldOfView = value;
    }
}
