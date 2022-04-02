using System.Collections;
using Toggle.Core;
using Toggle.Core.Function;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Toggle.Game.Common
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class BaseButtonView : MonoBehaviour
    {
        public BaseButton button { get; private set; }

        internal SpriteRenderer functionShape;

        private SpriteRenderer background;
        private new BoxCollider2D collider;

        private GameManager gameManager;
        private CommandManager commandManager;
        private AudioManager audioManager;

        private Camera cam;
        private bool inGame = false;

        void Awake()
        {
            cam = Camera.main;
            background = GetComponent<SpriteRenderer>();
            collider = GetComponent<BoxCollider2D>();

            functionShape = transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
        }

        public virtual void Start()
        {
            gameManager = GameManager.Instance;
            commandManager = CommandManager.Instance;
            audioManager = AudioManager.Instance;

            inGame = (gameManager != null);
        }

        public void SetData(BaseButton button)
        {
            this.button = button;
        }

        protected bool isHover;



        private float lastPointerOnUITime = 0;
        private bool CheckHover(Vector3 touchPosition)
        {
            if (TouchUtils.IsPointerOverGameObject())
            {
                lastPointerOnUITime = Time.time;
                return false;
            }

            if (Time.time - lastPointerOnUITime < 0.1f)
            {
                return false;
            }

            Vector3 pos = cam.ScreenToWorldPoint(touchPosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);

            return (hit.collider == collider);
        }

        public ButtonSkin skin;
        private Color bgColor, fgColor;
#if !UNITY_EDITOR && UNITY_ANDROID
        private bool hasTouchBegan = false;
#endif

        private Vector3 colorVelocity = new Vector3();

        public virtual void Update()
        {
            bool canInput = !(inGame && (gameManager.isGamePaused || gameManager.isInputFreeze));

            isHover = false;
            if (canInput)
            {
#if !UNITY_EDITOR && UNITY_ANDROID
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            isHover = CheckHover(touch.position);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (!hasTouchBegan) {
                        hasTouchBegan = true;
                    }
                    break;
                case TouchPhase.Ended:
                    if (isHover && hasTouchBegan)
                    {
                        OnClicked();
                        hasTouchBegan = false;
                    }
                    break;
                case TouchPhase.Canceled:
                    if (isHover) isHover = false;
                    break;
            }
        }
#else
                isHover = CheckHover(Input.mousePosition);
                if (Input.GetMouseButtonUp(0))
                {
                    if (isHover) OnClicked();
                }
#endif
            }

            if (button.isOn)
            {
                fgColor = skin.highlightFunctionColor;
                bgColor = isHover ? skin.highlightHoverColor : skin.highlightDefaultColor;
            }
            else
            {
                fgColor = skin.normalFunctionColor;
                bgColor = isHover ? skin.normalHoverColor : skin.normalDefaultColor;
            }


            Color currentBgColor = background.color;
            float r = Mathf.SmoothDamp(currentBgColor.r, bgColor.r, ref colorVelocity.x, Time.deltaTime);
            float g = Mathf.SmoothDamp(currentBgColor.g, bgColor.g, ref colorVelocity.y, Time.deltaTime);
            float b = Mathf.SmoothDamp(currentBgColor.b, bgColor.b, ref colorVelocity.z, Time.deltaTime);

            background.color = new Color(r, g, b, 1);
            functionShape.color = fgColor;
        }

        public virtual void OnClicked()
        {
            if (button.functionSubType != FunctionSubType.NOP && button.function.IsClickable)
            {
                var clickCommand = new ButtonClickCommand(button);
                clickCommand.Execute();
                commandManager.Register(clickCommand);

                audioManager.PlaySFX("Click", 0.4f, Random.Range(0.9f, 1.2f));
            }
        }

        float easeOutQuad(float x)
        {
            return 1 - (1 - x) * (1 - x);
        }

        public IEnumerator StartFuncShapeAnimation(Sprite targetSprite, Quaternion newRotation)
        {
            if (functionShape.sprite != targetSprite && targetSprite != null)
            {
                functionShape.sprite = targetSprite;
            }

            Vector3 previousLocalScale = functionShape.transform.localScale;
            Vector3 newLocalScale = Vector3.one;

            if (targetSprite == null)
            {
                newLocalScale = new Vector3(0, 0, 1);
            }

            Quaternion previousRotation = functionShape.transform.rotation;
            float t = 0;
            while (t < 1)
            {
                float et = easeOutQuad(t);
                functionShape.transform.rotation = Quaternion.Lerp(previousRotation, newRotation, et);
                functionShape.transform.localScale = Vector3.Lerp(previousLocalScale, newLocalScale, et);
                t += Time.deltaTime / 0.15f;
                yield return null;
            }
            functionShape.transform.rotation = newRotation;
            functionShape.transform.localScale = newLocalScale;

        }


    }
}