using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;

[System.Serializable]
public class LocalMessagePreset
{
    public Vector3 Alpha = new Vector3(0, 1, 1), FadeSpeed = new Vector2(1, 1);
    public bool HasBlinkAnimation = false, IsTopMost = true, hasFadeInOut = false;
    public List<Word> Words = new List<Word>() { new Word() };
    public bool showWords = false;
    public bool colapsed = true;
}

public class LocalMessage : MonoBehaviour
{
    [SerializeField]
    List<LocalMessagePreset> Presets = new List<LocalMessagePreset>() { new LocalMessagePreset() };
    private int _activePreset;
    public int ActivePreset
    {
        get { return _activePreset; }
        set
        {
            _activePreset = value;
            SetText();
        }
    }

    string fullText;
    GameObject canvas, iconPrefab;
    CanvasGroup group;
    Transform Icons;
    Text textComponent;
    float currentAlpha;
    bool isActive = false, isFading;
    int fadeDir = 0;


    // Start is called before the first frame update
    void Start()
    {
        SetText();
        Hide();
    }

    public void SetText()
    {
        canvas = transform.GetChild(0).gameObject;
        canvas.GetComponent<Canvas>().sortingOrder = Presets[ActivePreset].IsTopMost ? 20 : -4;
        Icons = canvas.transform.Find("Icons");
        iconPrefab = canvas.transform.Find("Icon").gameObject;
        textComponent = canvas.transform.GetChild(0).GetComponent<Text>();
        group = canvas.GetComponent<CanvasGroup>();

        fullText = "";
        for (int p = 0; p < Presets.Count; p++)
        {
            if (Presets[p].Words != null && Presets[p].Words.Count > 0)
            {
                foreach (Word word in Presets[p].Words)
                {
                    if (!word.isIcon)
                    {
                        DestroyIcon(p, Presets[p].Words.Where(i => i.isIcon).ToList().IndexOf(word));
                        word.spriteRendered = false;

                        if (p == ActivePreset)
                        {
                            string text = word.text;
                            if (word.isItalic) text = $"<i>{text}</i>";
                            if (word.isBold) text = $"<b>{text}</b>";
                            if (word.color != null) text = $"<color=#{ColorUtility.ToHtmlStringRGBA(word.color)}>{text}</color>";
                            fullText += text;
                        }
                    }
                    else
                    {
                        if (p == ActivePreset)
                            fullText += new string(' ', 5);
                        if (!word.spriteRendered)
                        {
                            word.spriteRendered = true;
                            var icon = Instantiate(iconPrefab, Icons);
                            icon.name = $"P{p}_Icon {Presets[p].Words.Count(i => i.isIcon) - 1}";
                        }
                    }
                }
                SetIcons();
            }
        }
        textComponent.text = fullText;
    }

    private void Update()
    {
        if (isFading)
        {
            if (fadeDir == 1)
            {
                bool finished = FadeIn(0, 1, Presets[ActivePreset].FadeSpeed.x);
                if (finished)
                {
                    isFading = false;
                    fadeDir = 0;
                }
            }
            else if (fadeDir == -1)
            {
                bool finished = FadeOut(0, 1, Presets[ActivePreset].FadeSpeed.y);
                if (finished)
                {
                    isFading = false;
                    fadeDir = 0;
                    isActive = false;
                    canvas.SetActive(false);
                }
            }
        }
        else if (Presets[ActivePreset].HasBlinkAnimation && isActive && !isFading)
        {
            if (fadeDir == 0) fadeDir = -1;

            if (fadeDir == 1)
            {
                bool finished = FadeIn(Presets[ActivePreset].Alpha.x, Presets[ActivePreset].Alpha.y, Presets[ActivePreset].Alpha.z);
                if (finished) fadeDir = -1;
            }
            else if (fadeDir == -1)
            {
                bool finished = FadeOut(Presets[ActivePreset].Alpha.x, Presets[ActivePreset].Alpha.y, Presets[ActivePreset].Alpha.z);
                if (finished) fadeDir = 1;
            }
        }
    }

    bool FadeIn(float min, float max, float speed)
    {
        currentAlpha = Mathf.Clamp(currentAlpha + (speed * Time.deltaTime), min, max);
        group.alpha = currentAlpha;

        return currentAlpha >= max;
    }

    bool FadeOut(float min, float max, float speed)
    {
        currentAlpha = Mathf.Clamp(currentAlpha + (-speed * Time.deltaTime), min, max);
        group.alpha = currentAlpha;
        return currentAlpha <= min;
    }

    public void Hide()
    {
        if (!isActive) Show();
        if (Presets[ActivePreset].hasFadeInOut)
        {
            isFading = true;
            fadeDir = -1;
        }
        else
        {
            fadeDir = 0;
            canvas.SetActive(false);
            isActive = false;
        }
    }
    public void Show()
    {
        if (isActive) Hide();
        if (Presets[ActivePreset].hasFadeInOut)
        {
            currentAlpha = 0;
            isActive = true;
            isFading = true;
            fadeDir = 1;
        }
        else
        {
            isActive = true;
            fadeDir = 0;
        }
        canvas.SetActive(true);
    }

    void DestroyIcon(int presetIndex, int iconIndex)
    {
        var name = $"P{presetIndex}_Icon {iconIndex}";
        var icon = Icons.Find(name);
        if (icon != null) DestroyImmediate(icon.gameObject);
    }

    void DestroyLastPresetIcons()
    {
        List<Transform> lastPresetIcons = new List<Transform>();
        foreach (Transform icon in Icons)
        {
            if (int.Parse(icon.name.ElementAt(1).ToString()) == (Presets.Count - 1))
            {
                lastPresetIcons.Add(icon);
            }
        }
        foreach (Transform icon in lastPresetIcons)
        {
            DestroyIcon(Presets.Count - 1, int.Parse(icon.name.Remove(0, icon.name.IndexOf('_') + 6)));
        }
    }

    void SetIcons()
    {
        //if (!canvas.activeSelf) return;
        foreach (Transform icon in Icons)
        {
            if (int.Parse(icon.name.ElementAt(1).ToString()) != ActivePreset)
            {
                icon.GetComponent<Image>().enabled = false;
                continue;
            }

            List<Word> allIcons = Presets[ActivePreset].Words.Where(i => i.isIcon).ToList();
            Word word = allIcons[int.Parse(icon.name.Remove(0, icon.name.IndexOf('_') + 6))];
            int index = Presets[ActivePreset].Words.IndexOf(word);

            icon.GetComponent<Image>().sprite = Presets[ActivePreset].Words[index].sprite;
            icon.GetComponent<Image>().enabled = true;
            icon.GetComponent<RectTransform>().localPosition = Presets[ActivePreset].Words[index].Offset * 10;
            icon.GetComponent<RectTransform>().localScale = Vector3.one * Presets[ActivePreset].Words[index].scale;
        }
    }


    #region Editor

    [CustomEditor(typeof(LocalMessage))]
    public class LocalMessageEditor : Editor
    {
        GUIStyle horizontalLine;
        public override void OnInspectorGUI()
        {
            LocalMessage localMessage = (LocalMessage)target;
            localMessage.SetText();

            horizontalLine = new GUIStyle();
            horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
            horizontalLine.margin = new RectOffset(0, 0, 4, 4);
            horizontalLine.fixedHeight = 1;

            EditorGUI.BeginChangeCheck();
            localMessage.ActivePreset = EditorGUILayout.Popup("Active Preset", localMessage.ActivePreset, localMessage.Presets.Select(p => localMessage.Presets.IndexOf(p).ToString()).ToArray());
            EditorGUI.EndChangeCheck();

            for (int p = 0; p < localMessage.Presets.Count; p++)
            {
                localMessage.Presets[p].colapsed = EditorGUILayout.Foldout(localMessage.Presets[p].colapsed, "Preset " + p, true);
                if (localMessage.Presets[p].colapsed)
                {
                    EditorGUI.indentLevel++;
                    var Alpha = localMessage.Presets[p].Alpha;
                    var FadeSpeed = localMessage.Presets[p].FadeSpeed;
                    var HasBlinkAnimation = localMessage.Presets[p].HasBlinkAnimation;
                    var IsTopMost = localMessage.Presets[p].IsTopMost;
                    var hasFadeInOut = localMessage.Presets[p].hasFadeInOut;
                    var Words = localMessage.Presets[p].Words;
                    var showWords = localMessage.Presets[p].showWords;

                    IsTopMost = EditorGUILayout.Toggle("TopMost", IsTopMost);
                    HasBlinkAnimation = EditorGUILayout.Toggle("Blink Animation", HasBlinkAnimation);
                    if (HasBlinkAnimation)
                    {
                        EditorGUI.indentLevel++;
                        Alpha = EditorGUILayout.Vector3Field("Alpha Min / Max / Speed", Alpha, GUILayout.MinWidth(350));
                        EditorGUI.indentLevel--;
                        HorizontalLine(Color.black);
                    }
                    hasFadeInOut = EditorGUILayout.Toggle("Fade In/Out", hasFadeInOut);
                    if (hasFadeInOut)
                    {
                        EditorGUI.indentLevel++;
                        FadeSpeed = EditorGUILayout.Vector2Field("Fade In / Out Speed", FadeSpeed);
                        EditorGUI.indentLevel--;
                        HorizontalLine(Color.black);
                    }

                    showWords = EditorGUILayout.Foldout(showWords, "Itens", true);

                    if (showWords)
                    {
                        EditorGUI.indentLevel++;

                        for (int i = 0; i < Words.Count; i++)
                        {
                            string label = "Item " + i + (localMessage.Presets[p].Words[i].isIcon ? " (Icon)" : " (Word)");
                            Words[i].colapsed = EditorGUILayout.Foldout(Words[i].colapsed, label, true);
                            if (Words[i].colapsed)
                            {
                                EditorGUI.indentLevel++;

                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(EditorGUI.indentLevel * 13);
                                Words[i].isIcon = GUILayout.SelectionGrid(Words[i].isIcon == true ? 1 : 0, new string[] { " Text", " Icon" }, 2, EditorStyles.radioButton, GUILayout.Width(90)) == 1;
                                EditorGUILayout.EndHorizontal();


                                if (Words[i].isIcon)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("Sprite", GUILayout.MaxWidth(80));
                                    Words[i].sprite = EditorGUILayout.ObjectField(Words[i].sprite, typeof(Sprite), true) as Sprite;
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("Scale", GUILayout.MaxWidth(80));
                                    Words[i].scale = EditorGUILayout.FloatField(Words[i].scale);
                                    EditorGUILayout.LabelField("Offset", GUILayout.MaxWidth(80));
                                    Words[i].Offset = EditorGUILayout.Vector2Field("", Words[i].Offset);
                                    EditorGUILayout.EndHorizontal();
                                }
                                else
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("Text", GUILayout.MaxWidth(80));
                                    Words[i].text = EditorGUILayout.TextField(Words[i].text);
                                    EditorGUILayout.EndHorizontal();

                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("Color", GUILayout.MaxWidth(80));
                                    Words[i].color = EditorGUILayout.ColorField(Words[i].color);

                                    EditorGUILayout.LabelField("Bold", GUILayout.MaxWidth(75));
                                    Words[i].isBold = EditorGUILayout.Toggle(Words[i].isBold);

                                    EditorGUILayout.LabelField("Italic", GUILayout.MaxWidth(75));
                                    Words[i].isItalic = EditorGUILayout.Toggle(Words[i].isItalic);
                                    GUILayout.FlexibleSpace();
                                    EditorGUILayout.EndHorizontal();
                                }
                                EditorGUI.indentLevel--;
                            }
                        }

                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("+")) Words.Add(new Word());
                        if (Words.Count == 1) GUI.enabled = false;
                        if (GUILayout.Button("-"))
                        {
                            if (Words[Words.Count - 1].isIcon)
                            {
                                var iconIndex = Words.Where(w => w.isIcon).ToList().Count - 1;
                                localMessage.DestroyIcon(p, iconIndex);
                            }
                            Words.RemoveAt(Words.Count - 1);
                        }
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();

                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;

                    localMessage.Presets[p].Alpha = Alpha;
                    localMessage.Presets[p].FadeSpeed = FadeSpeed;
                    localMessage.Presets[p].HasBlinkAnimation = HasBlinkAnimation;
                    localMessage.Presets[p].IsTopMost = IsTopMost;
                    localMessage.Presets[p].hasFadeInOut = hasFadeInOut;
                    localMessage.Presets[p].Words = Words;
                    localMessage.Presets[p].showWords = showWords;
                }
                HorizontalLine(Color.grey);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add Preset")) localMessage.Presets.Add(new LocalMessagePreset());
            if (localMessage.Presets.Count == 1) GUI.enabled = false;
            if (GUILayout.Button("Remove Preset"))
            {
                if (localMessage.ActivePreset == localMessage.Presets.Count - 1) localMessage.ActivePreset--;
                localMessage.DestroyLastPresetIcons();
                localMessage.Presets.RemoveAt(localMessage.Presets.Count - 1);
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(localMessage);
                if (!EditorApplication.isPlaying)
                    EditorSceneManager.MarkSceneDirty(localMessage.gameObject.scene);
            }
        }

        void HorizontalLine(Color color)
        {
            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, horizontalLine);
            GUI.color = c;
        }
    }
    #endregion
}

[System.Serializable]
public class Word
{
    [SerializeField]
    public string text;
    [SerializeField]
    public Color color = Color.white;
    [SerializeField]
    public bool isBold, isItalic;

    [SerializeField]
    public Sprite sprite;
    [SerializeField]
    public float scale = 1;
    [SerializeField]
    public Vector2 Offset = new Vector2(0, 0);
    [SerializeField]
    public bool spriteRendered, isIcon = false, colapsed = true;

    public Word(string text)
    {
        this.text = text;
    }

    public Word() { }
}
