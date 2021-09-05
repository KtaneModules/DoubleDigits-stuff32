using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class DoubleDigitScript : MonoBehaviour {

    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable Button;
    public TextMesh[] screenTexts;

    private int[] digits = new int[2];
    private int[] correctDigits = new int[2];
    private int answer;

    private int[,] TableOne = {
        {8, 5, 8, 3, 4, 1, 1, 9, 8, 3},
        {9, 7, 4, 1, 5, 2, 1, 4, 4, 2},
        {1, 8, 6, 1, 4, 6, 6, 8, 9, 4},
        {1, 8, 7, 5, 1, 6, 5, 5, 7, 5},
        {9, 5, 3, 6, 4, 3, 7, 2, 7, 2},
        {1, 1, 8, 8, 6, 3, 1, 7, 7, 6}
    };

    private int[,] TableTwo = {
        {5, 9, 9, 5, 5, 9, 3, 8, 9, 2},
        {6, 7, 9, 6, 8, 8, 7, 5, 9, 7},
        {8, 1, 3, 1, 8, 1, 9, 1, 7, 2},
        {9, 8, 8, 8, 1, 7, 9, 5, 5, 5},
        {9, 2, 5, 4, 6, 9, 2, 5, 7, 3},
        {9, 2, 5, 7, 6, 2, 2, 5, 7, 3}
    };

    void Awake()
    {
        _moduleID = _moduleIdCounter++;
    }

    // Use this for initialization
    void Start () {
        Button.OnInteract = ButtonPressed();
        Generate();
    }

    private KMSelectable.OnInteractHandler ButtonPressed()
    {
        return delegate
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, Button.transform);
            Button.AddInteractionPunch();
            Debug.Log("Button Pressed");
            if ((int)Bomb.GetTime() % 10 == answer)
            {
                Module.HandlePass();
            } 
            else
            {
                Module.HandleStrike();
            }

            return false;
        };
    }

    public static int ClipMax(int value, int maximumValue)
    {
        return value >= maximumValue ? maximumValue : value;
    }
    private void Generate()
    {
        for (int i = 0; i < screenTexts.Length; i++)
        {
            digits[i] = Rnd.Range(0, 10);
            screenTexts[i].text = digits[i].ToString();
        }

        correctDigits[0] = TableOne[ClipMax(Bomb.GetBatteryCount(), 5), digits[0]];
        correctDigits[1] = TableTwo[ClipMax(Bomb.GetBatteryCount(), 5), digits[1]];
        answer = (correctDigits[0] * correctDigits[1]) % 10;
        Debug.Log(correctDigits[0]);
        Debug.Log(correctDigits[1]);
        Debug.Log(answer);
    }
}
