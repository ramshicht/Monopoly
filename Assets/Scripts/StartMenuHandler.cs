using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class StartMenuHandler : MonoBehaviour
    {

        public bool haveBot;
        private static StartMenuHandler instance;
        void Start()
        {
            instance = this;
            SetButtons();
        }
        public static StartMenuHandler GetInstance()
        {
            return instance;
        }
        private void SetButtons()
        {
            Button button;
            button = GameObject.Find("ExitButton").GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                Application.Quit();
            });
            button = GameObject.Find("StartRegularGame").GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("NewGame", LoadSceneMode.Single);

            //GameObject.Find("CanvasMenu").active = false;
            //Application.LoadLevel();

        });
            button = GameObject.Find("StartBotGame").GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                haveBot = true;
                DontDestroyOnLoad(this);
                SceneManager.LoadScene("NewGame", LoadSceneMode.Single);


            //GameObject.Find("CanvasMenu").active = false;
            //Application.LoadLevel();

        });
        }



    }
}
