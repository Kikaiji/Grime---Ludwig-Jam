using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameState { Pause, Play, Menu, Cutscene}
public class GameManager : MonoBehaviour
{
    [SerializeField]
    GameObject player;
    [SerializeField]
    GameObject timer;
    [SerializeField]
    GameObject deathCounter;

    [SerializeField]
    GameObject menu;
    [SerializeField]
    GameObject settings;

    [SerializeField]
    GameObject slider1;
    public float sfxVolume;

    [SerializeField]
    GameObject slider2;
    public float musVolume;

    [SerializeField]
    GameObject slider3;
    public float retSens = 0.5f;
    
    enum MenuState { Menu, Settings}
    MenuState menuState = MenuState.Menu;
    public GameState gameState = GameState.Menu;

    [SerializeField]
    Button contButton;

    [SerializeField]
    GameObject blackOutSquare;

    [SerializeField]
    AudioSource cameraSource;
    public float curVolume;

    [SerializeField]
    GameObject pauseMenu;

    private void Awake()
    {
        SettingsData data = new SettingsData(GetComponent<GameManager>());
        data = SaveSystem.LoadSettings(GetComponent<GameManager>());
        if(data != null)
        {
            Debug.Log(data.sfxVolume + ", " + data.musVolume + ", " + data.retSens);
            sfxVolume = data.sfxVolume;
            musVolume = data.musVolume;
            retSens = data.retSens;
        }
        else
        {
            Debug.Log("No settings file, one has been created");
            sfxVolume = 1;
            musVolume = 1;
            retSens = 0.5f;
            SaveSystem.SaveSettings(GetComponent<GameManager>());
        }

        PlayerData playerData = new PlayerData(player.GetComponent<PlayerController>(), timer.GetComponent<Timer>());
        playerData = SaveSystem.LoadPlayer(GetComponent<GameManager>());
        if(playerData != null)
        {
            contButton.interactable = true;
        }
        else
        {
            contButton.interactable = false;
        }
        cameraSource.volume = musVolume;
    }

    public void StartButton()
    {
        gameState = GameState.Cutscene;
        menu.SetActive(false);
        StartCoroutine(StartFade(cameraSource, 2f, 0f));
        StartCoroutine(WaitForSeconds());
        
        
    }

    public void ContinueButton()
    {
        gameState = GameState.Cutscene;
        player.SetActive(true);
        StartCoroutine(FadeBlackOutSquare(GameState.Play));
        
        gameState = GameState.Play;
        menu.SetActive(false);
        timer.SetActive(true);


    }
    public void SettingsButton()
    {
        menuState = MenuState.Settings;
        slider1.GetComponent<Slider>().value = sfxVolume;
        slider2.GetComponent<Slider>().value = musVolume;
        slider3.GetComponent<Slider>().value = retSens;
        settings.SetActive(true);
        menu.SetActive(false);
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void BackButton()
    {
        if(menuState == MenuState.Settings)
        {
            menuState = MenuState.Menu;
            sfxVolume = slider1.GetComponent<Slider>().value;
            musVolume = slider2.GetComponent<Slider>().value;
            retSens = slider3.GetComponent<Slider>().value;

            cameraSource.volume = musVolume;

            SaveSystem.SaveSettings(GetComponent<GameManager>());

            settings.SetActive(false);
            menu.SetActive(true);
        }
    }

    public void MenuButton()
    {
        if(gameState == GameState.Pause)
        {
            StartCoroutine(FadeBlackOutSquare(GameState.Menu));
        }
    }

    IEnumerator WaitForSeconds()
    {
        yield return new WaitForSeconds(2f);
        player.transform.position = new Vector3(-28, 3.1f, 0);
        timer.GetComponent<Timer>().currentTime = 0f;
        player.SetActive(true);
        timer.SetActive(true);
        yield return new WaitForSeconds(1.18f);
        gameState = GameState.Play;

    }

    public IEnumerator FadeBlackOutSquare(GameState next, bool fadeToBlack = true, int fadeSpeed = 5 )
    {
        Color objectColor = blackOutSquare.GetComponent<Image>().color;
        float fadeAmount;
        if (fadeToBlack)
        {
            Debug.Log("Fading Out");
            while (blackOutSquare.GetComponent<Image>().color.a < 1)
            {
                fadeAmount = objectColor.a + (fadeSpeed * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                blackOutSquare.GetComponent<Image>().color = objectColor;
                
                yield return null;
            }
            switch (next)
            {
                case GameState.Play:
                    SetPlayer();
                    StartCoroutine(StartFade(cameraSource, 2f, 0f));
                    
                    break;
                case GameState.Menu:
                    SaveSystem.SavePlayer(player.GetComponent<PlayerController>(), timer.GetComponent<Timer>());
                    contButton.interactable = true;
                    player.SetActive(false);
                    gameState = GameState.Menu;
                    pauseMenu.SetActive(false);
                    menu.SetActive(true);
                    cameraSource.volume = musVolume;
                    timer.GetComponent<Timer>().currentTime = 0f;
                    player.GetComponent<PlayerController>().deaths = 0;
                    deathCounter.GetComponent<Text>().text = player.GetComponent<PlayerController>().deaths.ToString();
                    timer.SetActive(false);
                    contButton.interactable = true;
                    break;
            }
            StartCoroutine(FadeBlackOutSquare(next, false));
            Debug.Log("Finished");
        }
        else
        {
            Debug.Log("Fading in");
            while (blackOutSquare.GetComponent<Image>().color.a > 0)
            {
                fadeAmount = objectColor.a - (fadeSpeed * Time.deltaTime);

                objectColor = new Color(objectColor.r, objectColor.g, objectColor.b, fadeAmount);
                blackOutSquare.GetComponent<Image>().color = objectColor;
                yield return null;
            }
            Debug.Log("Finished");
        }
        yield return new WaitForEndOfFrame();
    }

    public static IEnumerator StartFade(AudioSource audioSource, float duration, float targetVolume)
    {
        float currentTime = 0;
        float start = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        yield break;
    }

    public IEnumerator EndScene(Text timeText, Text deathsText, GameObject endScreen )
    {
        Debug.Log("end");
        gameState = GameState.Cutscene;
        endScreen.SetActive(true);
        float currentTime = timer.GetComponent<Timer>().currentTime;
        timeText.text = (Mathf.FloorToInt(currentTime/60) + "m " + (currentTime % 60).ToString("F2") + "s ");

        deathsText.text = player.GetComponent<PlayerController>().deaths.ToString();
        yield return new WaitForSeconds(10f);
        endScreen.SetActive(false);

        player.transform.position = new Vector3(-28, 3.1f, 0f);
        player.GetComponent<PlayerController>().lastSafePosition = new Vector3(-28, 3.1f, 0f);
        timer.GetComponent<Timer>().currentTime = 0f;
        player.GetComponent<PlayerController>().deaths = 0;
        

        SaveSystem.SavePlayer(player.GetComponent<PlayerController>(), timer.GetComponent<Timer>());
        player.SetActive(false);
        gameState = GameState.Menu;
        menu.SetActive(true);
        cameraSource.volume = musVolume;
        timer.SetActive(false);
    }
    private void SetPlayer()
    {
        PlayerData data = SaveSystem.LoadPlayer(GetComponent<GameManager>());
        Debug.Log(data.position[0] + " " + data.position[1] + " " + data.time);
        player.transform.position = new Vector3(data.position[0], data.position[1], 0);
        player.gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(data.velocity[0], data.velocity[1]);
        timer.GetComponent<Timer>().currentTime = data.time;
        player.GetComponent<PlayerController>().deaths = data.deaths;
        player.GetComponent<PlayerController>().deaths = 0;
    }
}
