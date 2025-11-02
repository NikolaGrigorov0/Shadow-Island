using UnityEngine;
using UnityEngine.SceneManagement;

public class Game_Over : MonoBehaviour
{
    public GameObject gameOverPanel; // assign in Inspector

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            GameOver();
        }
    }

    // If using trigger instead:
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            GameOver();

        }
    }

    void GameOver()
    {
        // Show game over UI
        SceneManager.LoadScene("Game_Over");

        // Optional: stop game time
        Time.timeScale = 0f;
    }
}
