using System;
using BoundfoxStudios.FairyTaleDefender.Common;
using BoundfoxStudios.FairyTaleDefender.Systems.InputSystem.ScriptableObjects;
using BoundfoxStudios.FairyTaleDefender.Systems.SettingsSystem.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BoundfoxStudios.FairyTaleDefender.UI.Settings
{
	[AddComponentMenu(Constants.MenuNames.UI + "/" + nameof(KeybindSetting))]
	public class KeybindSetting : SettingBase
	{
		[field: SerializeField]
		public InputManagerSO InputManagerSO { get; private set; } = default!;

		[field: SerializeField]
		public InputActionReference InputActionReference { get; private set; } = default!;

		[field: SerializeField]
		public string BindingID { get; set; } = default!;

		[field: SerializeField]
		public TextMeshProUGUI PrimaryButtonText { get; private set; } = default!;

		[field: SerializeField]
		public InputBinding.DisplayStringOptions DisplayStringOptions { get; private set; }

		[field: SerializeField]
		public bool ExcludeMouse { get; private set; } = true;

		private const string TemporaryRebindText = "...";

		private InputAction? _action;
		private int _bindingIndex;

		private void Awake()
		{
			var actionID = InputActionReference.action.id;
			_action = InputManagerSO.GameInput.asset.FindAction(actionID);

			if (_action == null)
			{
				return;
			}
			_bindingIndex = _action.bindings.IndexOf(x => x.id == new Guid(BindingID));
		}

		public override void ResetSettings(SettingsSO mutableSettings)
		{
			base.ResetSettings(mutableSettings);

			if (_action == null)
			{
				return;
			}

			if (mutableSettings.Input.InputOverrides.TryGetValue(_action.name, out var inputOverride))
			{
				_action.LoadBindingOverridesFromJson(inputOverride);
			}
			else
			{
				_action.RemoveBindingOverride(_bindingIndex);
			}

			UpdateBindingDisplay();
		}

		public void StartRebind()
		{
			if (_action == null)
			{
				return;
			}

			PrimaryButtonText.text = TemporaryRebindText;
			InputManagerSO.StartRebind(_action, _bindingIndex, ExcludeMouse, RebindCompleted,
				RebindCanceled, MutableSettings);
		}

		public void ResetBinding()
		{
			if (_action == null || _action.bindings.Count <= _bindingIndex)
			{
				Debug.LogError("Could not find action or binding");
				return;
			}

			InputManagerSO.ResetBinding(_action, _bindingIndex, MutableSettings);

			UpdateBindingDisplay();
			OnSettingsChange();
		}

		private void RebindCompleted()
		{
			UpdateBindingDisplay();
			OnSettingsChange();
		}

		private void RebindCanceled()
		{
			UpdateBindingDisplay();
		}

		private void UpdateBindingDisplay()
		{
			if (_action == null || _bindingIndex == -1)
			{
				Debug.LogError("Could not find action or binding");
				return;
			}

			PrimaryButtonText.text = _action.GetBindingDisplayString(_bindingIndex, DisplayStringOptions);
		}
	}
}
