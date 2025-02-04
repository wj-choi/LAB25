﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance
    {
        get
        {
            if (instance != null)
                return instance;
            instance = FindObjectOfType<ParticleManager>();
            return instance;
        }
    }

    private static ParticleManager instance;

    // Prefabs
    public GameObject hitSparkPrefab;
    public GameObject hitHolePrefab;
    public GameObject bulletCasingPrefab;
    public GameObject bloodParticlePrefab;
    public GameObject bloodTracePrefab;

    // ObjectPool
    private MemoryPool bulletHolePool = new MemoryPool();
    private MemoryPool flarePool = new MemoryPool();
    private MemoryPool bulletCasingPool = new MemoryPool();
    private MemoryPool bloodParticlePool = new MemoryPool();
    private MemoryPool bloodTraceParticlePool = new MemoryPool();

    // Transform
    private Transform fireTraceParent;
    public Transform bulletCasingPoint;
    public Transform PlayerTr;

    private void Awake()
    {
        fireTraceParent = GameObject.Find("ObjectManager").transform;
    }

    private void Start()
    {
        bulletHolePool.Create(hitHolePrefab, 60, fireTraceParent);
        flarePool.Create(hitSparkPrefab, 60, fireTraceParent);
        bulletCasingPool.Create(bulletCasingPrefab, 110, fireTraceParent);
        bloodParticlePool.Create(bloodParticlePrefab, 200, fireTraceParent);
        bloodTraceParticlePool.Create(bloodTracePrefab, 200, fireTraceParent);
    }
    public IEnumerator BulletEffect()
    {
        GameObject bulletCasingObject;
        bulletCasingObject = bulletCasingPool.NewItem();
        bulletCasingObject.GetComponent<Rigidbody>().velocity = Vector3.zero;

        if (bulletCasingObject)
        {
            Rigidbody bulletRigid = bulletCasingObject.GetComponent<Rigidbody>();

            bulletCasingObject.transform.position = bulletCasingPoint.position;
            bulletCasingObject.transform.rotation = PlayerTr.rotation;
            bulletCasingObject.transform.Rotate(Random.Range(-45, 45), Random.Range(-90, 30), 0);
            bulletRigid.velocity = Vector3.zero;
            bulletRigid.AddForce((PlayerTr.transform.up * 0.5f + PlayerTr.transform.right) * 5);
        }
        yield return new WaitForSeconds(1f);

        bulletCasingObject.transform.SetParent(fireTraceParent);
        bulletCasingPool.RemoveItem(bulletCasingObject, bulletCasingPrefab, fireTraceParent);
    }

    public IEnumerator BloodTraceEffect(Vector3 pos)
    {
        GameObject bloodTraceObject;
        bloodTraceObject = bloodTraceParticlePool.NewItem();

    
        if (bloodTraceObject)
        {
            bloodTraceObject.transform.position = pos;
        }
        yield return new WaitForSeconds(15f);
        bloodTraceObject.transform.SetParent(fireTraceParent);
        bloodTraceParticlePool.RemoveItem(bloodTraceObject, bloodTracePrefab, fireTraceParent);
    }

    public IEnumerator FireEffect(Vector3 pos, Quaternion rot)
    {
        GameObject bulletHole, flare;
        bulletHole = bulletHolePool.NewItem();
        flare = flarePool.NewItem();

        if (bulletHole)
        {
            bulletHole.transform.position = pos;
            bulletHole.transform.SetParent(fireTraceParent);
            bulletHole.transform.rotation = rot;

            flare.transform.position = pos;
            flare.transform.SetParent(fireTraceParent);
            flare.transform.rotation = rot;
        }
        yield return new WaitForSeconds(0.5f);
        flarePool.RemoveItem(flare, hitSparkPrefab, fireTraceParent);

        yield return new WaitForSeconds(0.5f);
        bulletHolePool.RemoveItem(bulletHole, hitHolePrefab, fireTraceParent);
    }

    public IEnumerator BloodEffect(Vector3 pos)
    {
        GameObject bloodParticleTemp;
        bloodParticleTemp = bloodParticlePool.NewItem();
        if (bloodParticleTemp)
        {
            bloodParticleTemp.transform.position = pos;
            bloodParticleTemp.transform.SetParent(fireTraceParent);
        }
        yield return new WaitForSeconds(1.0f);
        bloodParticlePool.RemoveItem(bloodParticleTemp, bloodParticlePrefab, fireTraceParent);
    }

    void OnApplicationQuit()
    {
        bloodParticlePool.Dispose();
        bulletHolePool.Dispose();
        flarePool.Dispose();
        bulletCasingPool.Dispose();
        bloodTraceParticlePool.Dispose();
    }
}
