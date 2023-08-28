using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private GameObject spinner;
    [SerializeField] private TMP_Text continueText;
    
    [Tooltip("Scene number to load")]
    [SerializeField] private int sceneNumber;
    
    [SerializeField] private bool loopBackgroundImage = true;
    [SerializeField] private float loopInterval;
    [SerializeField] private GameObject[] backgroundImages;
    
    [Range(0, 1f)]
    [SerializeField] private float vignetteEffectValue;
    
    private AsyncOperation _async;
    private Image _vignetteEffect;

    private void Awake()
    {
        _vignetteEffect = transform.Find("VignetteEfect").GetComponent<Image>();
        _vignetteEffect.color = new Color(_vignetteEffect.color.r,_vignetteEffect.color.g,_vignetteEffect.color.b,vignetteEffectValue);
    }

    private void Start()
    {
        if (loopBackgroundImage)
        {
            StartCoroutine(TransitionImage());
        }
        
        gameObject.SetActive(true);
        StartCoroutine(StartLoading());
    }

    private void Update()
    {
        if (!Input.anyKeyDown)
        {
            return;
        }
        
        _async.allowSceneActivation = true;
    }


    private IEnumerator TransitionImage()
    {
        for (var i = 0; i < backgroundImages.Length; i++)
        {
            yield return new WaitForSeconds(loopInterval);
            
            for (int j = 0; j < backgroundImages.Length; j++)
            {
                backgroundImages[j].SetActive(false);
            }
            
            backgroundImages[i].SetActive(true);           
        }
    }

    private IEnumerator StartLoading()
    {
        _async = SceneManager.LoadSceneAsync(sceneNumber);
        _async.allowSceneActivation = false;

        yield return new WaitUntil(() => Mathf.Approximately(_async.progress, 0.9f));
        
        spinner.SetActive(false);
        continueText.gameObject.SetActive(true);
    }
}
