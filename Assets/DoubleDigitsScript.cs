using KModkit;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class DoubleDigitsScript : MonoBehaviour
{

    private static int _moduleIdCounter = 1;
    private int _moduleID = 0;
    private bool _moduleSolved;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable Button;
    public TextMesh[] screenTexts;

    private int[] digits = new int[2];
    private readonly int[] correctDigits = new int[2];
    private int answer;


    private static readonly int[,] TableOne = {
        {8, 5, 8, 3, 4, 1, 1, 9, 8, 3},
        {9, 7, 4, 1, 5, 2, 1, 4, 4, 2},
        {1, 8, 6, 1, 4, 6, 6, 8, 9, 4},
        {1, 8, 7, 5, 1, 6, 5, 5, 7, 5},
        {9, 5, 3, 6, 4, 3, 7, 2, 7, 2},
        {1, 1, 8, 8, 6, 3, 1, 7, 7, 6}
    };

    private static readonly int[,] TableTwo = {
        {5, 9, 9, 5, 5, 9, 3, 8, 9, 2},
        {6, 7, 9, 6, 8, 8, 7, 5, 9, 7},
        {8, 1, 3, 1, 8, 1, 9, 1, 7, 2},
        {9, 8, 8, 8, 1, 7, 9, 5, 5, 5},
        {9, 2, 5, 4, 6, 9, 2, 5, 7, 3},
        {9, 2, 5, 7, 6, 2, 2, 5, 7, 3}
    };

    private void Start()
    {
        _moduleID = _moduleIdCounter++;
        Button.OnInteract = ButtonPressed();
        Button.OnInteractEnded += delegate () { Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, Button.transform); };
        Generate();
    }

    private KMSelectable.OnInteractHandler ButtonPressed()
    {
        return delegate
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, Button.transform);
            Button.AddInteractionPunch();
            //Debug.Log("Button Pressed");
            if ((int)Bomb.GetTime() % 10 == answer)
            {
                _moduleSolved = true;
                Module.HandlePass();
                screenTexts[0].text = "G";
                screenTexts[1].text = "G";
                Debug.LogFormat("[Double Digits #{0}] The button was correctly pushed at {1}. Module solved.", _moduleID, answer);
            }
            else
            {
                Module.HandleStrike();
                Debug.LogFormat("[Double Digits #{0}] The button was incorrectly pushed at {1}. Strike.", _moduleID, (int)Bomb.GetTime() % 10);
            }
            return false;
        };
    }

    private void Generate()
    {
        for (int i = 0; i < screenTexts.Length; i++)
        {
            digits[i] = Rnd.Range(0, 10);
            screenTexts[i].text = digits[i].ToString();
        }
        correctDigits[0] = TableOne[Math.Min(Bomb.GetBatteryCount(), 5), digits[0]];
        correctDigits[1] = TableTwo[Math.Min(Bomb.GetBatteryCount(), 5), digits[1]];
        answer = (correctDigits[0] * correctDigits[1]) % 10;
        Debug.LogFormat("[Double Digits #{0}] The button must be pushed when the last digit of the timer is a {1}", _moduleID, answer);
    }

#pragma warning disable 0414
    private readonly string TwitchHelpMessage = "!{0} press 0-9 | Presses the button when the last digit of the timer is 0-9.";
#pragma warning restore 0414
    private IEnumerator ProcessTwitchCommand(string command)
    {
        if (_moduleSolved)
            yield return null;
        var m = Regex.Match(command, @"^\s*(press\s+)?([0-9])\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!m.Success)
            yield return null;
        while ((int)Bomb.GetTime() % 10 != m.Groups[2].Value[0] - '0')
            yield return "trycancel";
        Button.OnInteract();
        Button.OnInteractEnded();
    }

    private IEnumerator TwitchHandleForcedSolve()
    {
        if (_moduleSolved)
            yield break;
        while ((int)Bomb.GetTime() % 10 != answer)
            yield return "trycancel";
        Button.OnInteract();
        Button.OnInteractEnded();
    }
}
