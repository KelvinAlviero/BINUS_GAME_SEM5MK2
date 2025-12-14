using System.Collections;
using UnityEngine;

public class Hitstop : MonoBehaviour
{
    public static Hitstop instance;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    public void Stop(float duration)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(StopCoroutine(duration));
        }
    }

    private IEnumerator StopCoroutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
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
