using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public enum CursorType
    {
        AimReady = 0,
        AimMoving = 1,
        AimReloading = 2,
        Pointer = 3
    }

    [SerializeField] public List<CustomCursor> CustomCursors;
    [SerializeField] RectTransform tankIndicatorContainer, tankIndicatorArrow, baseIndicatorContainer, baseIndicatorArrow;
    [SerializeField] Transform tankTransform, baseTransform;
    public CustomCursor currentCursor;
    float frameTimer = 0f;

    public Slider tankSlider, humanSlider;
    public Color lowHealthColor;
    public Color highHealthColor;

    PlayerController playerCTRL;

    void Start()
    {
        SetCustomCursor(CursorType.AimReady);
        playerCTRL = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    void Update()
    {

        SetHealthBar(playerCTRL.TankHealth, playerCTRL.TankTotalHealth, tankSlider);

        SetHealthBar(playerCTRL.HumanHealth, playerCTRL.HumanTotalHealth, humanSlider);


        AnimateCursor();

        SetArrowIndicator(tankIndicatorContainer, tankIndicatorArrow, tankTransform);
        SetArrowIndicator(baseIndicatorContainer, baseIndicatorArrow, baseTransform);
    }

    void AnimateCursor()
    {
        //Lógica de animação do cursor
        Texture2D currentTexture = currentCursor.cursorTextures[currentCursor.currentIndex];
        if (currentCursor.hasAnimation)
        {
            frameTimer -= Time.deltaTime;
            if (frameTimer <= 0f)
            {
                frameTimer = currentCursor.durationInSeconds / currentCursor.cursorTextures.Length;
                Cursor.SetCursor(currentTexture, GetCursorCenter(currentTexture), CursorMode.ForceSoftware);
                currentCursor.NextIndex();
            }
        }
        else
        {
            Cursor.SetCursor(currentCursor.cursorTextures[0], GetCursorCenter(currentTexture), CursorMode.ForceSoftware);
        }
    }

    void SetArrowIndicator(RectTransform sourcePos, RectTransform sourceRot, Transform target)
    {
        //Posição do alvo em relação a tela
        Vector3 targetPos = Camera.main.WorldToScreenPoint(target.position);

        //Calcula a direção entre o alvo a seta
        Vector3 direction = targetPos - sourcePos.position;
        direction.Normalize();

        //Calcula o angulo necessário para girar
        Quaternion angle = Quaternion.LookRotation(forward: Vector3.forward, upwards: direction);

        //Rotaciona a seta até esse ângulo
        sourceRot.rotation = Quaternion.RotateTowards(sourceRot.rotation, angle, 1);

        float indicatorWidth = sourcePos.rect.width;
        float indicatorHeight = sourcePos.rect.height;
        sourcePos.Translate(direction * Time.deltaTime * 1000);
        sourcePos.position = new Vector3(Mathf.Clamp(sourcePos.position.x, indicatorWidth / 1.5f, Screen.width - indicatorWidth / 1.5f),
                                                  Mathf.Clamp(sourcePos.position.y, indicatorHeight, Screen.height - indicatorHeight / 1.5f), 0);
        
        sourcePos.gameObject.SetActive(!isTargetVisible(target));
    }

    bool isTargetVisible(Transform target)
    {
        Vector3 targetPos = Camera.main.WorldToScreenPoint(target.position);
        return targetPos.x > 0 && targetPos.x < Screen.width && targetPos.y > 0 && targetPos.y < Screen.height;
    }

    public void SetCustomCursor(CursorType cursorType)
    {
        CustomCursor cursor = CustomCursors[cursorType.GetHashCode()];
        currentCursor = cursor;
        currentCursor.currentIndex = 0;

        if (cursorType == CursorType.AimReloading)
        {
            cursor.durationInSeconds = playerCTRL.TankFireDelay;
        }

        frameTimer = currentCursor.durationInSeconds / currentCursor.cursorTextures.Length;
    }

    Vector2 GetCursorCenter(Texture2D sprite)
    {
        return new Vector2(sprite.width / 2, sprite.height / 2);
    }

    public void SetHealthBar(float health, float maxHealth, Slider slider)
    {
        var fillBar = (health * 100 / maxHealth) / 100;
        slider.value = fillBar;
        slider.maxValue = 1;

        slider.fillRect.GetComponentInChildren<Image>().color = Color.Lerp(lowHealthColor, highHealthColor, slider.normalizedValue);
    }

    [System.Serializable]
    public class CustomCursor
    {
        public CursorType cursorType;
        public Texture2D[] cursorTextures;
        public float durationInSeconds;
        public bool hasAnimation { get { return cursorTextures != null && cursorTextures.Length > 1; } }
        public int currentIndex = 0;
        public void NextIndex()
        {
            currentIndex = currentIndex == cursorTextures.Length - 1 ? 0 : currentIndex + 1;
        }
    }
}
