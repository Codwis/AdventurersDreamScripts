using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Gamemanager : MonoBehaviour
{
    public Transform playerSpawn;
    public GameObject playerPrefab;
    private GameObject currentPlayer;

    public bool inBattle = false;

    public static bool newGame = true;

    public AudioSource globalAudio;
    public AudioSource moodSource;

    public AudioClip[] goodTimeSound;
    public AudioClip[] battleSound;

    private float vol;
    public string saveName = "New Save";

    public Slider loadSlider;
    private CanvasGroup loadSliderCanvas;
    private Text loadingText;

    [HideInInspector] public bool first = true;
    public static Gamemanager instance;
    private bool queue = false;
    private void Awake()
    {
        vol = moodSource.volume;
        if(instance == null)
        {
            instance = this;
        }

        loadSliderCanvas = loadSlider.GetComponentInParent<CanvasGroup>();
        QualitySettings.vSyncCount = 0;
        loadingText = loadSlider.GetComponentInChildren<Text>();
    }

    private void Update()
    {
        if(!moodSource.isPlaying && !queue)
        {
            queue = true;
            PlayTrack();
        }
    }
    public void PlayTrack()
    {
        int rand;
        if(inBattle)
        {
            if(battleSound.Length > 1)
            {
                rand = Random.Range(0, battleSound.Length);
                moodSource.PlayOneShot(battleSound[rand]);
            }
            else
            {
                moodSource.PlayOneShot(battleSound[0]);
            }
        }
        else
        {
            if (goodTimeSound.Length > 1)
            {
                rand = Random.Range(0, goodTimeSound.Length);
                if(first)
                {
                    StartCoroutine(WaitForTrack(30, goodTimeSound[rand]));
                }
                else
                {
                    StartCoroutine(WaitForTrack(Random.Range(0, 30), goodTimeSound[rand]));
                }
            }
            else
            {
                moodSource.PlayOneShot(goodTimeSound[0]);
            }
        }
    }

    private const float audioRise = 50f;
    public IEnumerator WaitForTrack(float seconds, AudioClip toPlay)
    {
        yield return new WaitForSeconds(seconds);

        moodSource.volume = 0;
        moodSource.PlayOneShot(toPlay);
        moodSource.clip = toPlay;

        first = false;
        float s = 0;
        while (s < audioRise)
        {
            yield return new WaitForFixedUpdate();
            moodSource.volume = Mathf.Lerp(moodSource.volume, vol, 1 * Time.deltaTime);
            s++;
        }

        StartCoroutine(FadeOut(moodSource.clip.length - 5));
    }

    public IEnumerator FadeOut(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        float s = 0;
        while (s < audioRise)
        {
            yield return new WaitForFixedUpdate();
            moodSource.volume = Mathf.Lerp(moodSource.volume, 0, audioRise * Time.deltaTime);
            s++;
        }

        queue = false;
    }

    public void Respawn()
    {
        Gamemanager.newGame = false;
        AsyncOperation t = SceneManager.LoadSceneAsync("WorldOne");
        StartCoroutine(LoadingProgress(t));
        //Instantiate(playerPrefab, playerSpawn.position, playerSpawn.rotation);
    }
    public void NewGame()
    {
        Gamemanager.newGame = true;
        AsyncOperation op = SceneManager.LoadSceneAsync("WorldOne");
        StartCoroutine(LoadingProgress(op));
    }

    private IEnumerator LoadingProgress(AsyncOperation op)
    {
        int dotCount = 0;
        loadingText.text = "Loading";
        loadSliderCanvas.alpha = 1;
        while (!op.isDone)
        {
            if(dotCount > 5)
            {
                dotCount = 0;
                loadingText.text = "Loading";
            }
            dotCount++;
            loadingText.text += ".";

            loadSlider.value = op.progress;

            yield return null;
        }
        loadSliderCanvas.alpha = 0;
    }

    public static void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }
    public void Resume()
    {

    }
    public void Exit()
    {
        Application.Quit();
    }
}
