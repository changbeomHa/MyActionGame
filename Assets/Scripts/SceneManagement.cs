using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagement : MonoBehaviour
{
    public static SceneManagement instance = null;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += LoadedsceneEvent;
    }

    void Awake()
    {
        if (instance == null)
            instance = this;

        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
        
    }

    private void LoadedsceneEvent(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "TutorialRoom" && scene.name != "TrainingRoom")
        {
            Destroy(gameObject);
        }
    }
}
