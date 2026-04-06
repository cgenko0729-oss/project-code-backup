using QFSW.MOP2;
using UnityEngine;

public class ObjectLifeController : MonoBehaviour
{

    public float lifeTime = 5f;
    public float lifeTimeMax = 5f;

    public bool isPooled = false;
    public ObjectPool objectPool;

    public bool isUnscaledTime = false;

    public void OnEnable()
    {
        lifeTime = lifeTimeMax; 
    }

    void Start()
    {
        lifeTime = lifeTimeMax;
        
    }

    void Update()
    {
        if (isUnscaledTime) lifeTime -= Time.unscaledDeltaTime;
        else lifeTime -= Time.deltaTime;

        if(lifeTime <= 0f)
        {
            if(!isPooled) Destroy(gameObject);
            else objectPool.Release(gameObject); 
        }

    }

    public void ReleaseToObjectPool()
    {
        objectPool.Release(gameObject);

    }
}
