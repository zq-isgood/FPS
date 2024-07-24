using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��ս������ײ����
/// </summary>
public class Knife_Collider : MonoBehaviour
{
    private Knife knife;
    private bool canAttack = false;
    private List<GameObject> attackTargets = new List<GameObject>();
    public void Init(Knife knife)
    {
        this.knife = knife;
    }

    public void StartAttack()
    {
        canAttack = true;
    }

    public void StopAttack()
    {
        canAttack = false;
        attackTargets.Clear();
    }

    private void OnTriggerStay(Collider other)
    {
        if (canAttack == false) return;

        // �����û��������λ���򸽼�һ���˺�
        if (!attackTargets.Contains(other.gameObject))
        {
            attackTargets.Add(other.gameObject);
            // �˺�
            knife.HitTarget(other.gameObject, other.ClosestPoint(transform.position));
        }
    }
}
