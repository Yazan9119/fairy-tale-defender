using System;
using System.Collections.Generic;
using System.Linq;
using BoundfoxStudios.FairyTaleDefender.Common;
using BoundfoxStudios.FairyTaleDefender.Extensions;
using BoundfoxStudios.FairyTaleDefender.Infrastructure.FileManagement;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;

namespace BoundfoxStudios.FairyTaleDefender.Systems.SettingsSystem.ScriptableObjects
{
	/// <summary>
	/// Holding information about game settings.
	/// </summary>
	[CreateAssetMenu(fileName = "GameSettings", menuName = Constants.MenuNames.MenuName + "/GameSettings")]
	public class SettingsSO : ScriptableObject
	{
		private GameSettings? _gameSettings;

		public AudioConfig Audio => _gameSettings.EnsureOrThrow().Audio;
		public GraphicConfig Graphic => _gameSettings.EnsureOrThrow().Graphic;
		public LocalizationConfig Localization => _gameSettings.EnsureOrThrow().Localization;
		public CameraConfig Camera => _gameSettings.EnsureOrThrow().Camera;
		public InputConfig Input => _gameSettings.EnsureOrThrow().Input;

		private JsonFileManager _jsonFileManager = default!;
		private readonly string _jsonFileName = "config.json";

		private void OnEnable()
		{
			_gameSettings = new();
			_jsonFileManager = new();
		}

		public async UniTask SaveAsync()
		{
			await _jsonFileManager.WriteAsync(_jsonFileName, _gameSettings.EnsureOrThrow());
		}

		public async UniTask LoadAsync()
		{
			var fileExists = await _jsonFileManager.ExistsAsync(_jsonFileName);

			if (!fileExists)
				return;

			_gameSettings = await _jsonFileManager.ReadAsync<GameSettings>(_jsonFileName);
		}

		[Serializable]
		public class GameSettings
		{
			public AudioConfig Audio = new();
			public GraphicConfig Graphic = new();
			public LocalizationConfig Localization = new();
			public CameraConfig Camera = new();
			public InputConfig Input = new();
		}

		[Serializable]
		public class AudioConfig
		{
			[Range(0f, 1f)]
			public float MasterVolume = 1f;
			[Range(0f, 1f)]
			public float EffectsVolume = 1f;
			[Range(0f, 1f)]
			public float MusicVolume = 1f;
			[Range(0f, 1f)]
			public float UIVolume = 1f;
		}

		[Serializable]
		public class GraphicConfig
		{
			public int ScreenWidth;
			public int ScreenHeight;
			public bool IsFullscreen = true;
			public bool EnableCursorEffects = true;
			public GraphicLevels GraphicLevel = GraphicLevels.HighFidelity;
		}

		[Serializable]
		public class LocalizationConfig
		{
			[NonSerialized]
			public bool LocaleSetViaSteam = false;
			public LocaleIdentifier Locale;
		}

		[Serializable]
		public class CameraConfig
		{
			public bool EnableEdgePanning = true;
			public bool EnableKeyboardPanning = true;

			[Range(Constants.Settings.Panning.Start, Constants.Settings.Panning.Start)]
			public float PanSpeed = 7.5f;
		}

		[Serializable]
		public class InputConfig : ISerializationCallbackReceiver
		{
			[Serializable]
			private class InputOverride
			{
				public string ActionName = string.Empty;
				public string Overrides = string.Empty;
			}

			public Dictionary<string, string> InputOverrides = new();

			[SerializeField]
			// ReSharper disable once InconsistentNaming
			private List<InputOverride> _inputOverrides = new();

			public void OnBeforeSerialize()
			{
				var newOverrides = new List<InputOverride>();

				foreach (var kvp in InputOverrides)
				{
					InputOverride newOverride = new() { ActionName = kvp.Key, Overrides = kvp.Value };
					newOverrides.Add(newOverride);
				}
				_inputOverrides = newOverrides;
			}

			public void OnAfterDeserialize()
			{
				InputOverrides = _inputOverrides.ToDictionary(
					x => x.ActionName, x => x.Overrides);
			}
		}
	}
}
