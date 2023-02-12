using KModkit;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class DoubleDigitsScript : MonoBehaviour
{

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private bool _moduleSolved;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable Button;
    public TextMesh[] screenTexts;
    public GameObject ButtonObj;

    private int[] digits = new int[2];
    private readonly int[] correctDigits = new int[2];
    private int answer;

    bool allowFunction = true;


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
        _moduleId = _moduleIdCounter++;
        Button.OnInteract += ButtonPress;
        Button.OnInteractEnded += ButtonRelease;
        for (int i = 0; i < screenTexts.Length; i++)
        {
            digits[i] = Rnd.Range(0, 10);
            screenTexts[i].text = digits[i].ToString();
        }
        Debug.LogFormat("[Double Digits #{0}] The digit on screens are {1} and {2}.", _moduleId, digits[0], digits[1]);
        answer = Generate();
    }

    private bool ButtonPress()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, Button.transform);
        Button.AddInteractionPunch();
        StartCoroutine(MoveButton(true));
        if (!_moduleSolved && allowFunction)
        {
            if ((int)Bomb.GetTime() % 10 == answer)
            {
                _moduleSolved = true;
                Module.HandlePass();
                screenTexts[0].text = "G";
                screenTexts[1].text = "G";
                Debug.LogFormat("[Double Digits #{0}] The button was correctly pushed at {1}. Module solved.", _moduleId, answer);
            }
            else
            {
                Module.HandleStrike();
                Debug.LogFormat("[Double Digits #{0}] The button was incorrectly pushed at {1}. Strike. Regerating.", _moduleId, (int)Bomb.GetTime() % 10);
                StartCoroutine(IncorrectAnim());

            }
        }
        return false;
    }

    private void ButtonRelease()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, Button.transform);
        StartCoroutine(MoveButton(false));
    }

    private IEnumerator IncorrectAnim()
    {
        allowFunction = false;
        foreach (TextMesh screen in screenTexts)
        {
            screen.color = Color.red;
            screen.text = "✘";
        }
        yield return new WaitForSeconds(0.6f);

        digits[0] = Rnd.Range(0, 10);
        digits[1] = Rnd.Range(0, 10);
        answer = Generate();

        var changes = 20;

        for (int i = 0; i < changes; i++)
        {
            screenTexts[0].text = Rnd.Range(0, 10).ToString();
            yield return new WaitForSeconds(0.03f);
        }
        screenTexts[0].color = Color.white;
        Audio.PlaySoundAtTransform("click", screenTexts[0].transform);
        screenTexts[0].text = digits[0].ToString();

        for (int i = 0; i < changes; i++)
        {
            screenTexts[1].text = Rnd.Range(0, 10).ToString();
            yield return new WaitForSeconds(0.03f);
        }
        screenTexts[1].color = Color.white;
        Audio.PlaySoundAtTransform("click", screenTexts[1].transform);
        screenTexts[1].text = digits[1].ToString();

        Debug.LogFormat("[Double Digits #{0}] The digit on screens are {1} and {2}.", _moduleId, digits[0], digits[1]);
        allowFunction = true;
    }


    private IEnumerator MoveButton(bool pushIn)
    {
        var duration = 0.1f;
        var elapsed = 0f;
        while (elapsed < duration)
        {
            ButtonObj.transform.localPosition = new Vector3(0f, Easing.InOutQuad(elapsed, pushIn ? 0.025f : 0.017f, pushIn ? 0.017f : 0.025f, duration), -0.025f);
            yield return null;
            elapsed += Time.deltaTime;
        }
    }

    private int Generate()
    {
        correctDigits[0] = TableOne[Math.Min(Bomb.GetBatteryCount(), 5), digits[0]];
        correctDigits[1] = TableTwo[Math.Min(Bomb.GetBatteryCount(), 5), digits[1]];
        int timeWhenPress = (correctDigits[0] * correctDigits[1]) % 10;
        Debug.LogFormat("[Double Digits #{0}] The button must be pushed when the last digit of the timer is a {1}", _moduleId, timeWhenPress);
        return timeWhenPress;
    }

#pragma warning disable 0414
    private readonly string TwitchHelpMessage = "!{0} press 0-9 | Presses the button when the last digit of the timer is 0-9.";
#pragma warning restore 0414
    private IEnumerator ProcessTwitchCommand(string command)
    {
        if (_moduleSolved)
            yield break;
        var m = Regex.Match(command, @"^\s*(press\s+|submit\s+|push\s+)?([0-9])\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        if (!m.Success)
            yield break;
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
            yield return true;
        Button.OnInteract();
        Button.OnInteractEnded();
    }
}
