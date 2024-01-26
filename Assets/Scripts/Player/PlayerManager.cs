using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField]
    private float _maxHp;
    [SerializeField]
    private float _hp;
    [SerializeField]
    private float _laughGauge;

    void Start()
    {
        _hp = _maxHp;
    }

    public void TakeDamage(float damage)
    {
        _hp -= damage;
    }
    
    public void GetLaugh(float laugh)
    {
        _laughGauge += laugh;

        if (_laughGauge >= 100)
        {
            _laughGauge = 100f;
        }
    }

    public void GiveLaugh(float laugh)
    {
        _laughGauge -= laugh;

        if (_laughGauge >= 0)
        {
            _laughGauge = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Laugh"))
        {
            GetLaugh(20f);
            Destroy(other.gameObject);
        }
    }
}
