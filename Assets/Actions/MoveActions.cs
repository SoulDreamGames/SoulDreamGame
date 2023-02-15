//------------------------------------------------------------------------------
// <auto-generated>
//     This code was auto-generated by com.unity.inputsystem:InputActionCodeGenerator
//     version 1.3.0
//     from Assets/Actions/MoveActions.inputactions
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public partial class @MoveInput : IInputActionCollection2, IDisposable
{
    public InputActionAsset asset { get; }
    public @MoveInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""MoveActions"",
    ""maps"": [
        {
            ""name"": ""Ground"",
            ""id"": ""2e0165db-9825-4ea6-8461-67e03042d54d"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""105dd881-dd4d-4445-a2d5-aef618442c18"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""9003b26e-da4b-4376-b747-a2ae16261c69"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""0d246786-630f-4747-93c5-8cd7b5f193e8"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""64b1cf8c-e9a0-43f6-bad7-0e57e20086a7"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""5749c087-bc9d-4b3c-bf21-52616e2765af"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""c75e4f37-ecc8-4f5f-b056-a8b1a4b2f241"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        },
        {
            ""name"": ""Fly"",
            ""id"": ""561648c7-8d25-4171-a63e-5ca0a08f0abc"",
            ""actions"": [
                {
                    ""name"": ""Movement"",
                    ""type"": ""Value"",
                    ""id"": ""884c60a4-5f61-42a3-99da-fb076b4a639e"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Hover"",
                    ""type"": ""Value"",
                    ""id"": ""908bf114-020e-4107-bad1-a26a80e7fa29"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""8de42a75-82b0-4377-ba19-53b9ce956e16"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""6732c347-517f-4d11-b5ad-0018d19fba7c"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""60e8a136-02f8-47ee-9272-1efa72cf4a92"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""c75d9ffc-2dc7-457a-a0ae-ef7ad8cc3904"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""f9e87c30-6bab-4147-9c50-160450e747e6"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Movement"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""HoverAxis"",
                    ""id"": ""aac20b30-f883-4367-b2c4-c52b7063c26e"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hover"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""046abb45-e39d-4e6c-8a87-5040233befac"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""278b28dd-5c9e-4417-810c-01468e9d501d"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Hover"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Ground
        m_Ground = asset.FindActionMap("Ground", throwIfNotFound: true);
        m_Ground_Move = m_Ground.FindAction("Move", throwIfNotFound: true);
        // Fly
        m_Fly = asset.FindActionMap("Fly", throwIfNotFound: true);
        m_Fly_Movement = m_Fly.FindAction("Movement", throwIfNotFound: true);
        m_Fly_Hover = m_Fly.FindAction("Hover", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }
    public IEnumerable<InputBinding> bindings => asset.bindings;

    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    // Ground
    private readonly InputActionMap m_Ground;
    private IGroundActions m_GroundActionsCallbackInterface;
    private readonly InputAction m_Ground_Move;
    public struct GroundActions
    {
        private @MoveInput m_Wrapper;
        public GroundActions(@MoveInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Ground_Move;
        public InputActionMap Get() { return m_Wrapper.m_Ground; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GroundActions set) { return set.Get(); }
        public void SetCallbacks(IGroundActions instance)
        {
            if (m_Wrapper.m_GroundActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_GroundActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_GroundActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_GroundActionsCallbackInterface.OnMove;
            }
            m_Wrapper.m_GroundActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
            }
        }
    }
    public GroundActions @Ground => new GroundActions(this);

    // Fly
    private readonly InputActionMap m_Fly;
    private IFlyActions m_FlyActionsCallbackInterface;
    private readonly InputAction m_Fly_Movement;
    private readonly InputAction m_Fly_Hover;
    public struct FlyActions
    {
        private @MoveInput m_Wrapper;
        public FlyActions(@MoveInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Movement => m_Wrapper.m_Fly_Movement;
        public InputAction @Hover => m_Wrapper.m_Fly_Hover;
        public InputActionMap Get() { return m_Wrapper.m_Fly; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(FlyActions set) { return set.Get(); }
        public void SetCallbacks(IFlyActions instance)
        {
            if (m_Wrapper.m_FlyActionsCallbackInterface != null)
            {
                @Movement.started -= m_Wrapper.m_FlyActionsCallbackInterface.OnMovement;
                @Movement.performed -= m_Wrapper.m_FlyActionsCallbackInterface.OnMovement;
                @Movement.canceled -= m_Wrapper.m_FlyActionsCallbackInterface.OnMovement;
                @Hover.started -= m_Wrapper.m_FlyActionsCallbackInterface.OnHover;
                @Hover.performed -= m_Wrapper.m_FlyActionsCallbackInterface.OnHover;
                @Hover.canceled -= m_Wrapper.m_FlyActionsCallbackInterface.OnHover;
            }
            m_Wrapper.m_FlyActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Movement.started += instance.OnMovement;
                @Movement.performed += instance.OnMovement;
                @Movement.canceled += instance.OnMovement;
                @Hover.started += instance.OnHover;
                @Hover.performed += instance.OnHover;
                @Hover.canceled += instance.OnHover;
            }
        }
    }
    public FlyActions @Fly => new FlyActions(this);
    public interface IGroundActions
    {
        void OnMove(InputAction.CallbackContext context);
    }
    public interface IFlyActions
    {
        void OnMovement(InputAction.CallbackContext context);
        void OnHover(InputAction.CallbackContext context);
    }
}