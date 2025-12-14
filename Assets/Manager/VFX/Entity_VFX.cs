using System.Collections;
using UnityEngine;

public class Entity_VFX : MonoBehaviour
{
    private Material originalMat;
    private SpriteRenderer sr;

    [SerializeField] private Material onDamageVfxMat;
    [SerializeField] private float onDamageVfxDuration = .15f;
    private Coroutine onDamageVfxCoroutine;


    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalMat = sr.material;
    }

    public void PlayOnDamageVfx()
    {
        if (onDamageVfxCoroutine != null) StopCoroutine(onDamageVfxCoroutine);

        onDamageVfxCoroutine = StartCoroutine(OnDamageVfxCoroutine());
    }

    private IEnumerator OnDamageVfxCoroutine()
    {
        sr.material = onDamageVfxMat;

        yield return new WaitForSeconds(onDamageVfxDuration);

        sr.material = originalMat;
    }   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
