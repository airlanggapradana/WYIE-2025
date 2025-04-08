using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MobileControllerUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool mobileControlsEnabled = true;

    [Header("Movement Buttons")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button jumpButton;

    [Header("Combat Buttons")]
    [SerializeField] private Button attackButton;
    [SerializeField] private Button specialAttackButton;
    [SerializeField] private Button healthItemButton;

    // Input simulation variables
    private float horizontalInput = 0f;
    private bool jumpPressed = false;

    // Create a custom input class to allow the rest of the game to read our simulated input
    public static class CustomInput
    {
        public static float HorizontalInput { get; set; } = 0f;
        public static bool JumpButtonDown { get; private set; } = false;
        public static bool JumpButton { get; set; } = false;

        // Combat inputs
        public static bool AttackButtonDown { get; private set; } = false;
        public static bool SpecialAttackButtonDown { get; private set; } = false;
        public static bool HealthItemButtonDown { get; private set; } = false;

        // Track if buttons were just pressed this frame
        private static bool jumpPressedThisFrame = false;
        private static bool attackPressedThisFrame = false;
        private static bool specialAttackPressedThisFrame = false;
        private static bool healthItemPressedThisFrame = false;

        public static void SetJumpPressed(bool value)
        {
            // If newly pressed, set JumpButtonDown to true
            if (value && !JumpButton)
            {
                jumpPressedThisFrame = true;
            }

            // Always update the held state
            JumpButton = value;
        }

        public static void SetAttackPressed()
        {
            attackPressedThisFrame = true;
        }

        public static void SetSpecialAttackPressed()
        {
            specialAttackPressedThisFrame = true;
        }

        public static void SetHealthItemPressed()
        {
            healthItemPressedThisFrame = true;
        }

        public static void Update()
        {
            // Set ButtonDown flags based on if they were pressed this frame
            JumpButtonDown = jumpPressedThisFrame;
            AttackButtonDown = attackPressedThisFrame;
            SpecialAttackButtonDown = specialAttackPressedThisFrame;
            HealthItemButtonDown = healthItemPressedThisFrame;

            // Reset for next frame
            jumpPressedThisFrame = false;
            attackPressedThisFrame = false;
            specialAttackPressedThisFrame = false;
            healthItemPressedThisFrame = false;
        }
    }

    private void Start()
    {
        Debug.Log("MobileControllerUI Start - Setting up controls");

        // Check if buttons are assigned
        if (jumpButton == null)
            Debug.LogError("Jump button is not assigned in the Inspector!");
        if (leftButton == null)
            Debug.LogError("Left button is not assigned in the Inspector!");
        if (rightButton == null)
            Debug.LogError("Right button is not assigned in the Inspector!");

        // Set up button event listeners
        SetupButtonEvents();

        // Only show mobile controls if enabled
        SetControlsActive(mobileControlsEnabled);
    }

    private void SetupButtonEvents()
    {
        if (leftButton != null)
        {
            // Add pointer down/up events for continuous movement while pressed
            EventTrigger leftTrigger = SetupEventTrigger(leftButton.gameObject);
            AddEventTriggerListener(leftTrigger, EventTriggerType.PointerDown, (data) => { SetHorizontalInput(-1f); });
            AddEventTriggerListener(leftTrigger, EventTriggerType.PointerUp, (data) => { ResetHorizontalInput(); });
            AddEventTriggerListener(leftTrigger, EventTriggerType.PointerExit, (data) => { ResetHorizontalInput(); });
        }

        if (rightButton != null)
        {
            // Add pointer down/up events for continuous movement while pressed
            EventTrigger rightTrigger = SetupEventTrigger(rightButton.gameObject);
            AddEventTriggerListener(rightTrigger, EventTriggerType.PointerDown, (data) => { SetHorizontalInput(1f); });
            AddEventTriggerListener(rightTrigger, EventTriggerType.PointerUp, (data) => { ResetHorizontalInput(); });
            AddEventTriggerListener(rightTrigger, EventTriggerType.PointerExit, (data) => { ResetHorizontalInput(); });
        }

        if (jumpButton != null)
        {
            Debug.Log("Setting up jump button events");

            // First try using standard button click event
            jumpButton.onClick.AddListener(() =>
            {
                Debug.Log("Jump button clicked (onClick)");
                SetJumpPressed(true);
                // Automatically release after a short delay
                Invoke("ReleaseJumpButton", 0.1f);
            });

            // Also set up the event trigger as backup
            EventTrigger jumpTrigger = SetupEventTrigger(jumpButton.gameObject);
            AddEventTriggerListener(jumpTrigger, EventTriggerType.PointerDown, (data) =>
            {
                Debug.Log("Jump button pointer down");
                SetJumpPressed(true);
            });
            AddEventTriggerListener(jumpTrigger, EventTriggerType.PointerUp, (data) =>
            {
                Debug.Log("Jump button pointer up");
                SetJumpPressed(false);
            });
            AddEventTriggerListener(jumpTrigger, EventTriggerType.PointerExit, (data) =>
            {
                Debug.Log("Jump button pointer exit");
                SetJumpPressed(false);
            });
        }

        // Combat buttons
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(() =>
            {
                Debug.Log("Attack button clicked");
                CustomInput.SetAttackPressed();
            });
        }

        if (specialAttackButton != null)
        {
            specialAttackButton.onClick.AddListener(() =>
            {
                Debug.Log("Special attack button clicked");
                CustomInput.SetSpecialAttackPressed();
            });
        }

        if (healthItemButton != null)
        {
            healthItemButton.onClick.AddListener(() =>
            {
                Debug.Log("Health item button clicked");
                CustomInput.SetHealthItemPressed();
            });
        }
    }

    private void ReleaseJumpButton()
    {
        Debug.Log("Auto-releasing jump button");
        SetJumpPressed(false);
    }

    private void SetHorizontalInput(float value)
    {
        horizontalInput = value;
        CustomInput.HorizontalInput = value;
    }

    private void ResetHorizontalInput()
    {
        horizontalInput = 0f;
        CustomInput.HorizontalInput = 0f;
    }

    private void SetJumpPressed(bool pressed)
    {
        Debug.Log("SetJumpPressed called with value: " + pressed);
        jumpPressed = pressed;

        // Use our improved method to track jump press states
        CustomInput.SetJumpPressed(pressed);
    }

    private EventTrigger SetupEventTrigger(GameObject obj)
    {
        // Get existing or add new EventTrigger component
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = obj.AddComponent<EventTrigger>();
        }

        if (trigger.triggers == null)
        {
            trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();
        }

        return trigger;
    }

    private void AddEventTriggerListener(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventType;
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    public void SetControlsActive(bool active)
    {
        if (this.gameObject.activeSelf != active)
        {
            this.gameObject.SetActive(active);
        }
    }

    // Reset inputs when application loses focus
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            ResetHorizontalInput();
            SetJumpPressed(false);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Update our input system
        CustomInput.Update();

        // Debug logging to see if jump is being detected
        if (CustomInput.JumpButtonDown)
        {
            Debug.Log("Jump button pressed detected!");
        }

        if (CustomInput.AttackButtonDown)
        {
            Debug.Log("Attack button pressed detected!");
        }

        if (CustomInput.SpecialAttackButtonDown)
        {
            Debug.Log("Special attack button pressed detected!");
        }

        if (CustomInput.HealthItemButtonDown)
        {
            Debug.Log("Health item button pressed detected!");
        }
    }

    // Public methods that can be called directly from button OnClick() in inspector
    public void OnJumpButtonPressed()
    {
        Debug.Log("OnJumpButtonPressed called directly from button");
        SetJumpPressed(true);
        // Auto-release after a short delay
        Invoke("ReleaseJumpButton", 0.1f);
    }

    public void OnAttackButtonPressed()
    {
        Debug.Log("OnAttackButtonPressed called directly from button");
        CustomInput.SetAttackPressed();
    }

    public void OnSpecialAttackButtonPressed()
    {
        Debug.Log("OnSpecialAttackButtonPressed called directly from button");
        CustomInput.SetSpecialAttackPressed();
    }

    public void OnHealthItemButtonPressed()
    {
        Debug.Log("OnHealthItemButtonPressed called directly from button");
        CustomInput.SetHealthItemPressed();
    }

    public void OnLeftButtonPressed()
    {
        Debug.Log("OnLeftButtonPressed called directly from button");
        SetHorizontalInput(-1f);
    }

    public void OnLeftButtonReleased()
    {
        Debug.Log("OnLeftButtonReleased called directly from button");
        ResetHorizontalInput();
    }

    public void OnRightButtonPressed()
    {
        Debug.Log("OnRightButtonPressed called directly from button");
        SetHorizontalInput(1f);
    }

    public void OnRightButtonReleased()
    {
        Debug.Log("OnRightButtonReleased called directly from button");
        ResetHorizontalInput();
    }
}