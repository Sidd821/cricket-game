using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Threading.Tasks;

public class SinglePlayerSetup : MonoBehaviour
{
    public Button T5Button;
    public Button T10Button;
    public Button T20Button;
    public Button ODIButton;
    public Button Heads;
    public Button Tails;
    public Button Bat;
    public Button Bowl;
    public Text TitleText;
    public Text headsText;
    public Text tailsText;
    public static int maxOvers;
    public static bool bowling = false;
    public bool headsClicked = false;
    public bool tailsClicked = false;
    void Start()
    {
        Heads.gameObject.SetActive(false);
        Tails.gameObject.SetActive(false);
        Bat.gameObject.SetActive(false);
        Bowl.gameObject.SetActive(false);
        T5Button.onClick.AddListener(T5Match);
        T10Button.onClick.AddListener(T10Match);
        T20Button.onClick.AddListener(T20Match);
        ODIButton.onClick.AddListener(ODIMatch);
        Heads.onClick.AddListener(HeadsButton);
        Tails.onClick.AddListener(TailsButton);
        Bat.onClick.AddListener(BatButton);
        Bowl.onClick.AddListener(BowlButton);

    }
        // Update is called once per frame
    void Update()
    {

    }
    
    void T5Match()
    {
        OnButtonClick(T5Button);
        maxOvers = 5;
    }
    void T10Match()
    {
        OnButtonClick(T10Button);
        maxOvers = 10;
    }
    void T20Match()
    {
        OnButtonClick(T20Button);
        maxOvers = 20;
    }
    void ODIMatch()
    {
        OnButtonClick(ODIButton);
        maxOvers = 50;
    }

    void CoinTossMatch(int choice)
    {
        System.Random r = new System.Random();
        int Result = r.Next(0, 2);
        if (Result == 0 && choice == 0)
        {
            TitleText.text = ("You Win the Toss");

            Bat.gameObject.SetActive(true);
            Bowl.gameObject.SetActive(true);

        }
        else
        {
            int batOrBowl = r.Next(0, 2);
            if (batOrBowl == 0)
            {

                TitleText.text = ("You Lose the Toss and You are batting first ");
                TitleText.gameObject.SetActive(true);

                bowling = false;
                Heads.gameObject.SetActive(false);
                Tails.gameObject.SetActive(false);
                Heads.interactable = false;
                Tails.interactable = false;
                NextScene();

            }
            else
            {

                TitleText.text = ("You Lose the Toss and \n You are bowling first ");
                TitleText.gameObject.SetActive(true);

                bowling = true;
                Heads.gameObject.SetActive(false);
                Tails.gameObject.SetActive(false);
                Heads.interactable = false;
                Tails.interactable = false;
                NextScene();    
            }
        }
    }
    void HeadsButton()
    {

        CoinTossMatch(0);
    }
    void TailsButton()
    {

        CoinTossMatch(1);
    }
    void BowlButton()
    {
        TitleText.text = ("You are Bowling first");
        TitleText.gameObject.SetActive(true);

        bowling = true;
        NextScene();
    }
    void BatButton()
    {

        TitleText.text = ("You are batting first");
        TitleText.gameObject.SetActive(true);

        bowling = false;
        NextScene();
    }   
    async Task NextScene()
    {
        await Task.Delay(2000);
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
    void OnButtonClick(Button b)
    {
        TitleText.gameObject.SetActive(false);
        b.interactable = false;
        Heads.gameObject.SetActive(true);
        Tails.gameObject.SetActive(true);

        switch (b.name)
        {
            case "T5Button":
                T10Button.gameObject.SetActive(false);
                T20Button.gameObject.SetActive(false);
                ODIButton.gameObject.SetActive(false);
                break;
            case "T10Button":
                T5Button.gameObject.SetActive(false);
                T20Button.gameObject.SetActive(false);
                ODIButton.gameObject.SetActive(false);
                break;
            case "T20Button":
                T5Button.gameObject.SetActive(false);
                T10Button.gameObject.SetActive(false);
                ODIButton.gameObject.SetActive(false);
                break;
            case "ODIButton":
                T5Button.gameObject.SetActive(false);
                T10Button.gameObject.SetActive(false);
                T20Button.gameObject.SetActive(false);
                break;
        }
    }

}
