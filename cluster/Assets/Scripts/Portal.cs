using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public string nextSceneName;
    private bool isColliding = false;

    private void Start() {
        PlayerPrefs.SetString("CurrentSceneName", SceneManager.GetActiveScene().name);
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        isColliding = true;
    }

    private void OnTriggerExit2D(Collider2D collision) {
        isColliding = false;
    }

    private void Update() {
        if (isColliding && Input.GetKeyDown(KeyCode.Space)) {
            SceneManager.LoadScene(nextSceneName);
        }
    }   
}