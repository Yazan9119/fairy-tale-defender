using BoundfoxStudios.FairyTaleDefender.Common;
using BoundfoxStudios.FairyTaleDefender.Systems.SaveGameSystem.ScriptableObjects;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BoundfoxStudios.FairyTaleDefender.UI.Settings
{
	[AddComponentMenu(Constants.MenuNames.UI + "/" + nameof(ResetGameProgress))]
	public class ResetGameProgress : MonoBehaviour
	{
		[field: Header("References")]
		[field: SerializeField]
		private SaveGameManagerSO SaveGameManager { get; set; } = default!;

		public void ResetProgress()
		{
			SaveGameManager.DeleteSaveGameAsync(Constants.SaveGames.DefaultSaveGameName).Forget();
		}
	}
}
