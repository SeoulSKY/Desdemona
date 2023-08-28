using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class LoadingScreenBarSystem : MonoBehaviour
{
    [Tooltip("Scene number to load")]
    [SerializeField] private int sceneNumber;
    public bool backGroundImageAndLoop;
    public float LoopTime;
    public GameObject[] backgroundImages;
    [Range(0,1f)]public float vignetteEfectVolue; // Must be a value between 0 and 1
    AsyncOperation async;
    Image vignetteEfect;

    private void Awake()
    {
        vignetteEfect = transform.Find("VignetteEfect").GetComponent<Image>();
        vignetteEfect.color = new Color(vignetteEfect.color.r,vignetteEfect.color.g,vignetteEfect.color.b,vignetteEfectVolue);
    }

    private void Start()
    {
        if (backGroundImageAndLoop)
            StartCoroutine(transitionImage());
        
        gameObject.SetActive(true);
        StartCoroutine(Loading(sceneNumber));
    }


    // The pictures change according to the time of
    IEnumerator transitionImage()
    {
        for (int i = 0; i < backgroundImages.Length; i++)
        {
            yield return new WaitForSeconds(LoopTime);
            for (int j = 0; j < backgroundImages.Length; j++)
                backgroundImages[j].SetActive(false);
            backgroundImages[i].SetActive(true);           
        }
    }

    // Activate the scene 
    IEnumerator Loading (int sceneNo)
    {
        async = SceneManager.LoadSceneAsync(sceneNo);
        async.allowSceneActivation = false;

        // Continue until the installation is completed
        while (!async.isDone)
        {
            if (Mathf.Approximately(async.progress, 0.9f))
            {
                async.allowSceneActivation = true;
            }
            yield return new WaitForEndOfFrame();
        }
    }

}
