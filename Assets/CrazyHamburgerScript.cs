using KModkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class CrazyHamburgerScript : MonoBehaviour
{

    static int _moduleIdCounter = 1;
    int _moduleID = 0;

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] Buttons;
    public SpriteRenderer[] Sprites;
    public Sprite EatenHamburger;

    private Coroutine[] ButtonAnimCoroutines = new Coroutine[2];
    private KMAudio.KMAudioRef Sound;
    private int BreadIndex = -1;
    private int CheeseCount;
    private string Ingredients;
    private bool BurgerIsCrazy, HorseTrigger, Solved;
    private Settings _Settings;

    class Settings
    {
        public bool DisableSounds = false;
    }

    void GetSettings()
    {
        var SettingsConfig = new ModConfig<Settings>("CrazyHamburger");
        _Settings = SettingsConfig.Settings; // This reads the settings from the file, or creates a new file if it does not exist
        SettingsConfig.Settings = _Settings; // This writes any updates or fixes if there's an issue with the file
    }

    void Awake()
    {
        _moduleID = _moduleIdCounter++;
        GetSettings();
        Module.OnActivate += delegate { if (Sound != null) Sound.StopSound(); if (!_Settings.DisableSounds) Sound = Audio.HandlePlaySoundAtTransformWithRef("activate", Sprites[0].transform, false); };
        Buttons[0].OnInteract += delegate { ButtonPress(0); return false; };
        Buttons[1].OnInteract += delegate { ButtonPress(1); return false; };
        AddIngredient(false);
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private IEnumerator Solve()
    {
        Module.HandlePass();
        Debug.LogFormat("[Crazy Hamburger #{0}] You ate the hamburger, which was correct. Module solved!", _moduleID);
        for (int i = 0; i < 6; i++)
            Sprites[i].gameObject.SetActive(false);
        Sprites[6].gameObject.SetActive(true);
        if (Sound != null) Sound.StopSound();
        if (!_Settings.DisableSounds)
            Sound = Audio.HandlePlaySoundAtTransformWithRef("solve", Sprites[0].transform, false);
        Solved = true;
        float timer = 0;
        while (timer < 3.5f)
        {
            yield return null;
            timer += Time.deltaTime;
        }
        if (Sound != null) Sound.StopSound();
        if (!_Settings.DisableSounds)
            Sound = Audio.HandlePlaySoundAtTransformWithRef("nom", Sprites[0].transform, false);
        Sprites[6].sprite = EatenHamburger;
    }

    void Strike(bool strike1)
    {
        Module.HandleStrike();
        if (!strike1)
            Debug.LogFormat("[Crazy Hamburger #{0}] You added the current ingredient, which was incorrect. Strike!", _moduleID);
        else
            Debug.LogFormat("[Crazy Hamburger #{0}] You ate the hamburger, which was incorrect. Strike!", _moduleID);
        if (Sound != null) Sound.StopSound();
        if (strike1 && !_Settings.DisableSounds)
            Sound = Audio.HandlePlaySoundAtTransformWithRef("strike 1", Sprites[0].transform, false);
        else if (!_Settings.DisableSounds)
            Sound = Audio.HandlePlaySoundAtTransformWithRef("strike 2", Sprites[0].transform, false);
        Ingredients = "";
        BurgerIsCrazy = false;
        BreadIndex = -1;
        CheeseCount = 0;
        HorseTrigger = false;
        AddIngredient(false);
    }

    void AddIngredient(bool PlaySound)
    {
        if (PlaySound)
            Debug.LogFormat("[Crazy Hamburger #{0}] You added the current ingredient, which was correct.", _moduleID);
        for (int i = 0; i < 6; i++)
            Sprites[i].gameObject.SetActive(false);
        switch (Rnd.Range(0, 6))
        {
            case 0:
                Ingredients += "B";
                int Current = Ingredients.Length - 1;
                Sprites[0].gameObject.SetActive(true);
                if (PlaySound)
                {
                    if (Sound != null) Sound.StopSound();
                    if (!_Settings.DisableSounds)
                        Sound = Audio.HandlePlaySoundAtTransformWithRef("bread made in turkey", Sprites[0].transform, false);
                }
                if (BreadIndex != -1 && Current - BreadIndex > 1 && Ingredients[Current - 1] == Ingredients[BreadIndex + 1])
                    BurgerIsCrazy = true;
                BreadIndex = Current;
                Debug.LogFormat("[Crazy Hamburger #{0}] Bread Made in Turkey has been added. This {1} made the hamburger crazy.", _moduleID, BurgerIsCrazy ? "has" : "has not");
                break;
            case 1:
                Ingredients += "C";
                Sprites[1].gameObject.SetActive(true);
                if (PlaySound)
                {
                    if (Sound != null) Sound.StopSound();
                    if (!_Settings.DisableSounds)
                        Sound = Audio.HandlePlaySoundAtTransformWithRef("cheese from são paolo from brazil", Sprites[0].transform, false);
                }
                CheeseCount++;
                if (CheeseCount >= 4)
                {
                    BurgerIsCrazy = true;
                    break;
                }
                Debug.LogFormat("[Crazy Hamburger #{0}] Cheese from São Paolo From Brazil has been added. This {1} made the hamburger crazy.", _moduleID, BurgerIsCrazy ? "has" : "has not");
                break;
            case 2:
                Ingredients += "G";
                Sprites[2].gameObject.SetActive(true);
                if (PlaySound)
                {
                    if (Sound != null) Sound.StopSound();
                    if (!_Settings.DisableSounds)
                        Sound = Audio.HandlePlaySoundAtTransformWithRef("grass of death", Sprites[0].transform, false);
                }
                if (Ingredients.Length >= 10)
                    BurgerIsCrazy = true;
                Debug.LogFormat("[Crazy Hamburger #{0}] Grass of Death has been added. This {1} made the hamburger crazy.", _moduleID, BurgerIsCrazy ? "has" : "has not");
                break;
            case 3:
                Ingredients += "H";
                Sprites[3].gameObject.SetActive(true);
                if (PlaySound)
                {
                    if (Sound != null) Sound.StopSound();
                    if (!_Settings.DisableSounds)
                        Sound = Audio.HandlePlaySoundAtTransformWithRef("horse meat", Sprites[0].transform, false);
                }
                if (HorseTrigger)
                    BurgerIsCrazy = true;
                if (Ingredients.Length > 1)
                    if (!HorseTrigger && (Ingredients[Ingredients.Length - 2] == 'C' || Ingredients[Ingredients.Length - 2] == 'G'))
                        HorseTrigger = true;
                Debug.LogFormat("[Crazy Hamburger #{0}] Horse Meat From Brazil has been added. This {1} made the hamburger crazy.", _moduleID, BurgerIsCrazy ? "has" : "has not");
                break;
            case 4:
                Ingredients += "O";
                Sprites[4].gameObject.SetActive(true);
                if (PlaySound)
                {
                    if (Sound != null) Sound.StopSound();
                    if (!_Settings.DisableSounds)
                        Sound = Audio.HandlePlaySoundAtTransformWithRef("oil from iraq", Sprites[0].transform, false);
                }
                if (Ingredients.Length > 3)
                    if (Ingredients[Ingredients.Length - 3] == Ingredients[Ingredients.Length - 4])
                        BurgerIsCrazy = true;
                Debug.LogFormat("[Crazy Hamburger #{0}] Oil From Iraq has been added. This {1} made the hamburger crazy.", _moduleID, BurgerIsCrazy ? "has" : "has not");
                break;
            default:
                Ingredients += "R";
                Sprites[5].gameObject.SetActive(true);
                if (PlaySound)
                {
                    if (Sound != null) Sound.StopSound();
                    if (!_Settings.DisableSounds)
                        Sound = Audio.HandlePlaySoundAtTransformWithRef("red hot chili peppers", Sprites[0].transform, false);
                }
                if (Ingredients.Contains('B') && Ingredients.Contains('C') && Ingredients.Contains('G') && Ingredients.Contains('H') && Ingredients.Contains('O'))
                    BurgerIsCrazy = true;
                Debug.LogFormat("[Crazy Hamburger #{0}] Red Hot Chili Peppers has been added. This {1} made the hamburger crazy.", _moduleID, BurgerIsCrazy ? "has" : "has not");
                break;
        }
    }

    void ButtonPress(int pos)
    {
        if (ButtonAnimCoroutines[pos] != null)
            StopCoroutine(ButtonAnimCoroutines[pos]);
        ButtonAnimCoroutines[pos] = StartCoroutine(ButtonAnim(pos));
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, Buttons[pos].transform);
        Buttons[pos].AddInteractionPunch();
        if (!Solved)
        {
            if (pos == 0)
            {
                if (!BurgerIsCrazy)
                    AddIngredient(true);
                else
                    Strike(false);
            }
            else
            {
                if (BurgerIsCrazy)
                    StartCoroutine(Solve());
                else
                    Strike(true);
            }
        }
    }

    private IEnumerator ButtonAnim(int pos, float duration = 0.1f)
    {
        float timer = 0;
        while (timer < duration / 2)
        {
            yield return null;
            timer += Time.deltaTime;
            Buttons[pos].transform.localPosition = Vector3.Lerp(new Vector3(Buttons[pos].transform.localPosition.x, 0.0212f, Buttons[pos].transform.localPosition.z), new Vector3(Buttons[pos].transform.localPosition.x, 0.0107f, Buttons[pos].transform.localPosition.z), timer / (duration / 2));
        }
        Buttons[pos].transform.localPosition = new Vector3(Buttons[pos].transform.localPosition.x, 0.0107f, Buttons[pos].transform.localPosition.z);
        timer = 0;
        while (timer < duration / 2)
        {
            yield return null;
            timer += Time.deltaTime;
            Buttons[pos].transform.localPosition = Vector3.Lerp(new Vector3(Buttons[pos].transform.localPosition.x, 0.0107f, Buttons[pos].transform.localPosition.z), new Vector3(Buttons[pos].transform.localPosition.x, 0.0212f, Buttons[pos].transform.localPosition.z), timer / (duration / 2));
        }
        Buttons[pos].transform.localPosition = new Vector3(Buttons[pos].transform.localPosition.x, 0.0212f, Buttons[pos].transform.localPosition.z);
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "Use '!{0} add' to add the current ingredient and use '!{0} eat' to eat the hamburger.";
#pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        yield return null;
        if (command == "add")
            Buttons[0].OnInteract();
        else if (command == "eat")
            Buttons[1].OnInteract();
        else
        {
            yield return "sendtochaterror Invalid command.";
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (!Solved)
        {
            if (BurgerIsCrazy)
                Buttons[1].OnInteract();
            else
                Buttons[0].OnInteract();
            yield return true;
        }
    }
}
