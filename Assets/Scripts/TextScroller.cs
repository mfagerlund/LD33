using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class TextScroller : MyMonoBehaviour
{
    [TextArea(50, 60)]
    public string text;

    public int currentLine = 0;
    public float fadeInTime = 0.2f;
    public float rowShowTime = 5f;
    public float fadeOutTime = 0.2f;
    public float timeBetweenRows = 0.2f;

    private Text _text;
    private CanvasGroup _canvasGroup;

    public IEnumerator Start()
    {
        _text = GetComponent<Text>();
        _canvasGroup = GetComponent<CanvasGroup>();

        while (true)
        {
            List<string> lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
            currentLine %= lines.Count;
            string line = lines[currentLine];
            _text.text = line;

            yield return StartCoroutine(TimeEase(f =>_canvasGroup.alpha = f, 0, 1, fadeInTime, Ease.FromType(EaseType.CubeIn)));            
            yield return new WaitForSeconds(rowShowTime);
            yield return StartCoroutine(TimeEase(f =>_canvasGroup.alpha = f, 1, 0, fadeOutTime, Ease.FromType(EaseType.CubeOut)));
            yield return new WaitForSeconds(timeBetweenRows);
            currentLine++;
        }
    }
}