using System;
using System.Linq;
using BoundfoxStudios.FairyTaleDefender.Common;
using BoundfoxStudios.FairyTaleDefender.Systems.SettingsSystem.ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BoundfoxStudios.FairyTaleDefender.Systems.InputSystem.ScriptableObjects
{
	// We only need one.
	//[CreateAssetMenu(fileName = "InputManager", menuName = Constants.MenuNames.Input + "/InputManager", order = 0)]
	public class InputManagerSO : ScriptableObject
	{
		[field: SerializeField]
		public InputReaderSO InputReaderSO { get; private set; } = default!;

		public GameInput GameInput => InputReaderSO.GameInput;

		private const string MousePath = "Mouse";
		private const string MouseRightButtonBindingPath = "<Mouse>/rightButton";
		private const string KeyboardAnyKey = "<Keyboard>/anyKey";
		private InputActionRebindingExtensions.RebindingOperation? _ongoingRebind;


		public void StartRebind(InputAction action, int bindingIndex, bool excludeMouse, Action completeCallback,
			Action cancelCallback, SettingsSO mutableSettings)
		{
			_ongoingRebind?.Cancel();

			if (action.bindings[bindingIndex].isComposite)
			{
				var firstPartIndex = bindingIndex + 1;
				if (action.bindings.Count > firstPartIndex && action.bindings[firstPartIndex].isPartOfComposite)
				{
					PerformRebind(action, firstPartIndex, excludeMouse, completeCallback, cancelCallback,
						mutableSettings, allCompositeParts: true);
				}
			}
			else
			{
				PerformRebind(action, bindingIndex, excludeMouse, completeCallback, cancelCallback,
					mutableSettings);
			}
		}

		private void PerformRebind(InputAction action, int index, bool excludeMouse, Action completeCallback,
			Action cancelCallback, SettingsSO mutableSettings, bool allCompositeParts = false)
		{
			action.Disable();

			_ongoingRebind = action.PerformInteractiveRebinding(index)
				.OnCancel(_ =>
				{
					CleanUp();
					action.Enable();
					cancelCallback();
				})
				.OnComplete(_ =>
				{
					CleanUp();
					action.Enable();

					if (allCompositeParts)
					{
						var nextIndex = index + 1;
						if (action.bindings.Count > nextIndex && action.bindings[nextIndex].isPartOfComposite)
						{
							PerformRebind(action, nextIndex, excludeMouse, completeCallback, cancelCallback,
								mutableSettings, true);
						}
					}

					SetValue(action, mutableSettings);
					completeCallback();
				})
				.WithCancelingThrough(MouseRightButtonBindingPath);

			ExcludeBindings(action, index, excludeMouse);

			_ongoingRebind.Start();
		}

		private void ExcludeBindings(InputAction action, int index, bool excludeMouse)
		{
			if (_ongoingRebind == null)
			{
				return;
			}

			if (excludeMouse)
			{
				_ongoingRebind.WithControlsExcluding(MousePath);
			}

			var otherControls =
				GameInput.bindings.Where(binding => binding.effectivePath != action.bindings[index].effectivePath);

			foreach (var control in otherControls)
			{
				_ongoingRebind.WithControlsExcluding(control.effectivePath);
			}

			_ongoingRebind.WithControlsExcluding(KeyboardAnyKey);
		}

		public void ResetBinding(InputAction action, int bindingIndex, SettingsSO mutableSettings)
		{
			if (action.bindings[bindingIndex].isComposite)
			{
				for (var i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++)
					action.RemoveBindingOverride(i);
			}
			else
			{
				action.RemoveBindingOverride(bindingIndex);
			}

			SetValue(action, mutableSettings);
		}

		private void SetValue(InputAction action, SettingsSO mutableSettings)
		{
			var newOverride = action.SaveBindingOverridesAsJson();

			mutableSettings.Input.InputOverrides[action.name] = newOverride;
		}

		private void CleanUp()
		{
			_ongoingRebind?.Dispose();
			_ongoingRebind = null;
		}

		private void OnDisable()
		{
			CleanUp();
		}
	}
}
