using System;
using System.Linq;
using BoundfoxStudios.FairyTaleDefender.Editor.Extensions;
using BoundfoxStudios.FairyTaleDefender.UI.Settings;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BoundfoxStudios.FairyTaleDefender.Editor.Editors.UI
{
	// Provides a more comfortable way of selecting the actual binding on an action in editor.
	// Source: Input system rebind example
	[CustomEditor(typeof(KeybindSetting))]
	public class KeybindSettingEditor : UnityEditor.Editor
	{
		private SerializedProperty _inputManagerProperty = default!;
		private SerializedProperty _actionProperty = default!;
		private SerializedProperty _bindingIdProperty = default!;
		private SerializedProperty _primaryButtonTextProperty = default!;
		private SerializedProperty _displayStringOptionsProperty = default!;
		private SerializedProperty _excludeMouseProperty = default!;

		private readonly GUIContent _bindingLabel = new("Binding");
		private readonly GUIContent _displayOptionsLabel = new("Display Options");
		private GUIContent[] _bindingOptions = default!;
		private string[] _bindingOptionValues = default!;
		private int _selectedBindingOption;

        protected void OnEnable()
        {
	        _inputManagerProperty = serializedObject.FindRealProperty(nameof(KeybindSetting.InputManagerSO));
	        _actionProperty = serializedObject.FindRealProperty(nameof(KeybindSetting.InputActionReference));
	        _bindingIdProperty = serializedObject.FindRealProperty(nameof(KeybindSetting.BindingID));
            _primaryButtonTextProperty = serializedObject.FindRealProperty(nameof(KeybindSetting.PrimaryButtonText));
            _displayStringOptionsProperty = serializedObject.FindRealProperty(nameof(KeybindSetting.DisplayStringOptions));
            _excludeMouseProperty = serializedObject.FindRealProperty(nameof(KeybindSetting.ExcludeMouse));

            RefreshBindingOptions();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_inputManagerProperty);
            EditorGUILayout.PropertyField(_actionProperty);

            var newSelectedBinding = EditorGUILayout.Popup(_bindingLabel, _selectedBindingOption, _bindingOptions);
            if (newSelectedBinding != _selectedBindingOption)
            {
	            var bindingId = _bindingOptionValues[newSelectedBinding];
	            _bindingIdProperty.stringValue = bindingId;
	            _selectedBindingOption = newSelectedBinding;
            }

            var optionsOld = (InputBinding.DisplayStringOptions)_displayStringOptionsProperty.intValue;
            var optionsNew = (InputBinding.DisplayStringOptions)EditorGUILayout.EnumFlagsField(_displayOptionsLabel, optionsOld);
            if (optionsOld != optionsNew)
	            _displayStringOptionsProperty.intValue = (int)optionsNew;

            EditorGUILayout.PropertyField(_primaryButtonTextProperty);
            EditorGUILayout.PropertyField(_excludeMouseProperty);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                RefreshBindingOptions();
            }
        }

        private void RefreshBindingOptions()
        {
            var actionReference = (InputActionReference)_actionProperty.objectReferenceValue;
            var action = actionReference != null ? actionReference.action : null;
            _selectedBindingOption = -1;

            if (action == null)
            {
                _bindingOptions = Array.Empty<GUIContent>();
                _bindingOptionValues = Array.Empty<string>();
                return;
            }

            var bindings = action.bindings;
            var bindingCount = bindings.Count;

            _bindingOptions = new GUIContent[bindingCount];
            _bindingOptionValues = new string[bindingCount];
            var currentBindingId = _bindingIdProperty.stringValue;
            for (var i = 0; i < bindingCount; ++i)
            {
                var binding = bindings[i];
                var bindingId = binding.id.ToString();
                var haveBindingGroups = !string.IsNullOrEmpty(binding.groups);

                // If we don't have a binding groups (control schemes), show the device that if there are, for example,
                // there are two bindings with the display string "A", the user can see that one is for the keyboard
                // and the other for the gamepad.
                var displayOptions =
                    InputBinding.DisplayStringOptions.DontUseShortDisplayNames | InputBinding.DisplayStringOptions.IgnoreBindingOverrides;
                if (!haveBindingGroups)
                    displayOptions |= InputBinding.DisplayStringOptions.DontOmitDevice;

                // Create display string.
                var displayString = action.GetBindingDisplayString(i, displayOptions);

                // If binding is part of a composite, include the part name.
                if (binding.isPartOfComposite)
                    displayString = $"{ObjectNames.NicifyVariableName(binding.name)}: {displayString}";

                // Some composites use '/' as a separator. When used in popup, this will lead to to submenus. Prevent
                // by instead using a backlash.
                displayString = displayString.Replace('/', '\\');

                // If the binding is part of control schemes, mention them.
                if (haveBindingGroups)
                {
                    var asset = action.actionMap?.asset;
                    if (asset != null)
                    {
                        var controlSchemes = string.Join(", ",
                            binding.groups.Split(InputBinding.Separator)
                                .Select(x => asset.controlSchemes.FirstOrDefault(c => c.bindingGroup == x).name));

                        displayString = $"{displayString} ({controlSchemes})";
                    }
                }

                _bindingOptions[i] = new (displayString);
                _bindingOptionValues[i] = bindingId;

                if (currentBindingId == bindingId)
                    _selectedBindingOption = i;
            }
        }
	}
}
