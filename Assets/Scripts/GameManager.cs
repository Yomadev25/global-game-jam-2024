using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const string MessageOnGameover = "On Game Over";
    public const string MessageUpdateAlliesCount = "Update Allies Count";

    public enum OverType
    {
        Gameover,
        Totem,
        Allies,
    }

    public static GameManager instance;
    public DateTime startTime;

    [SerializeField]
    private GameObject[] _enemyPrefabs;
    [SerializeField]
    private GameObject _overviewCamera;
    [SerializeField]
    private ParticleSystem _totemFx;
    [SerializeField]
    private ParticleSystem _darkFx;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        
    }

    private void Start()
    {
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.SceneFadeOut();
        }
        startTime = DateTime.Now;

        for (int i = 0; i < 40; i++)
        {
            Vector3 pos;
            pos.x = UnityEngine.Random.Range(-7f, 28f);
            pos.y = 2f;
            pos.z = UnityEngine.Random.Range(-7f, 25f);

            Quaternion rot = Quaternion.Euler(pos.x, UnityEngine.Random.Range(0f, 360f), pos.z);

            Instantiate(_enemyPrefabs[UnityEngine.Random.Range(0, _enemyPrefabs.Length)], pos, rot);
        }
    }

    public void Gameover(OverType type)
    {
        StartCoroutine(GameoverCoroutine(type));     
    }

    IEnumerator GameoverCoroutine(OverType type)
    {
        GameObject[] mobs = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] allies = mobs.Where(x => x.GetComponent<EnemyManager>().type == EnemyManager.Type.Ally).ToArray();
        GameObject[] enemies = mobs.Where(x => x.GetComponent<EnemyManager>().type == EnemyManager.Type.Enemy).ToArray();

        switch (type)
        {
            case OverType.Gameover:
                _darkFx.Play();
                foreach (GameObject ally in allies)
                {
                    ally.GetComponent<EnemyManager>().TakeDamage(100);
                }
                break;
            case OverType.Totem:
                foreach (GameObject enemy in enemies)
                {
                    enemy.GetComponent<EnemyManager>().TakeLaughDamage(100);
                }
                break;
            case OverType.Allies:
                _totemFx.Play();
                break;
            default:
                break;
        }

        _overviewCamera.SetActive(true);
        yield return new WaitForSeconds(3f);
        _overviewCamera.SetActive(false);
        MessagingCenter.Send(this, MessageOnGameover, type);
    }

    public void AlliesCount()
    {
        GameObject[] mobs = GameObject.FindGameObjectsWithTag("Enemy");
        int allies = mobs.Count(x => x.GetComponent<EnemyManager>().type == EnemyManager.Type.Ally);
        int enemies = mobs.Count(x => x.GetComponent<EnemyManager>().type == EnemyManager.Type.Enemy);

        if (enemies == 0)
        {
            Gameover(OverType.Allies);
        }

        MessagingCenter.Send(this, MessageUpdateAlliesCount, allies);
    }
}
