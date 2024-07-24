using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
/// <summary>
/// ���״̬
/// </summary>
public enum PlayerState
{
    Move,
    Shoot,
    Reload
}

/// <summary>
/// ��ҿ�����
/// </summary>
public class Player_Controller : MonoBehaviour
{
    public static Player_Controller Instance;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Camera[] cameras;
    [SerializeField] Image crossImage;
    [SerializeField] FirstPersonController firstPersonController;
    private int hp = 100;


    #region �������
    [SerializeField] WeaponBase[] weapons;
    private int currWeaponIndex = -1;       // ��ǰ����
    private int previousWeaponIndex = -1;   // ��һ������
    private bool canChangeWeapon = true;    // �ܷ��л�����
    #endregion

    public PlayerState playerState { get; private set; }

    private void Awake()
    {
        Instance = this;
        // ��ʼ������
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].Init(this);  //����帳ֵ
        }
        // Ӧ��Ĭ������
        ChangeWeapon(2);
    }


    private void Update()
    {
        // ����������
        weapons[currWeaponIndex].OnUpdatePlayerState(playerState);  //ÿһ֡��ȥ��飬���״̬һ�䣬�ͻ�����������
        // ��ǹ���
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
            // ���������һ�����������л�
            if (previousWeaponIndex >=0)
            {
                ChangeWeapon(previousWeaponIndex);
            }
        }

    }

    /// <summary>
    /// �޸����״̬
    /// </summary>
    public void ChangePlayerState(PlayerState newState)
    {
        playerState = newState;
        // ֪ͨ��ȥ�޸�״̬
        weapons[currWeaponIndex].OnEnterPlayerState(playerState);
    }

    /// <summary>
    /// �л�����
    /// </summary>
    private void ChangeWeapon(int newWeaponIndex)
    {
        // �ǲ����ظ��ٰ�ͬһ����
        if (currWeaponIndex == newWeaponIndex) return;
        // ��һ������������ = ��ǰ����������
        previousWeaponIndex = currWeaponIndex;
        // ��¼�µ���������
        currWeaponIndex = newWeaponIndex;

        // ����ǵ�һ��ʹ������ �������˳������Ķ���
        if (previousWeaponIndex < 0)
        {
            // ֱ�ӽ���������
            weapons[currWeaponIndex].Enter();
        }
        // ��ǰ������������
        else
        {
            // �˳���ǰ����,����������˳��ɹ��󣬲��ܽ���������
            weapons[previousWeaponIndex].Exit(OnWeaponExitOver);
            canChangeWeapon = false;
        }
    }

    /// <summary>
    /// ����һ�������ɹ��˳���ִ�е�Action
    /// </summary>
    private void OnWeaponExitOver()
    {
        // ֱ�ӽ���������
        weapons[currWeaponIndex].Enter();
        canChangeWeapon = true;
    }

    /// <summary>
    /// Ϊ����������ʼ��
    /// </summary>
    public void InitForEnterWeapon(bool wantCorsshair,bool wantBullet)
    {
        crossImage.gameObject.SetActive(wantCorsshair);
        UI_MainPanel.Instance.InitForEnterWeapon(wantBullet);
    }

    /// <summary>
    /// �����ӵ�UI
    /// </summary>
    public void UpdateBulletUI(int curr_BulletNum,int curr_MaxBulletNum,int stanby_BulletNum)
    {
        UI_MainPanel.Instance.UpdateCurrBullet_Text(curr_BulletNum,curr_MaxBulletNum);
        UI_MainPanel.Instance.UpdateStandByBullet_Text(stanby_BulletNum);
    }


    #region ������

    Coroutine shootRecoil_CrossCoroutine;
    Coroutine shootRecoil_CameraCoroutine;
    public void StartShootRecoil(float recoil = 1)
    {
        // ����֮ͣǰ��
        if (shootRecoil_CrossCoroutine!=null) StopCoroutine(shootRecoil_CrossCoroutine);
        if (shootRecoil_CameraCoroutine != null) StopCoroutine(shootRecoil_CameraCoroutine);

        // �����µ�
        // ��׼���ĺ�����
        shootRecoil_CrossCoroutine = StartCoroutine(ShootRecoil_Cross(recoil));
        // �ӽǵĺ�����
        shootRecoil_CameraCoroutine = StartCoroutine(ShootRecoil_Camera(recoil));
    }

    // ����������׼��Ч��
    IEnumerator ShootRecoil_Cross(float recoil = 1)
    {
        Vector2 scale = crossImage.transform.localScale;
        // �Ŵ�
        while (scale.x < 1.3f * recoil)
        {
            yield return null;
            scale.x += Time.deltaTime * 3;
            scale.y = scale.x;
            crossImage.transform.localScale = scale;
        }
        // ��С
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

    // �����������Ч��
    IEnumerator ShootRecoil_Camera(float recoil = 1)
    {
        // ȷ��ƫ����
        float xOffset = Random.Range(0.3f, 0.6f) * recoil;
        float yOffset = Random.Range(-0.15f, 0.15f) * recoil;
        firstPersonController.xRotOffset = xOffset;
        firstPersonController.yRotOffset = yOffset;
        // �ȴ�6֡
        for (int i = 0; i < 6; i++)
        {
            yield return null;
        }
        // ��λ
        firstPersonController.xRotOffset = 0;
        firstPersonController.yRotOffset = 0;
    }

    #endregion

    /// <summary>
    /// ����
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
