using BoundfoxStudios.FairyTaleDefender.Common;
using BoundfoxStudios.FairyTaleDefender.Infrastructure.Events.ScriptableObjects;
using UnityEngine;

namespace BoundfoxStudios.FairyTaleDefender.Systems.ObjectiveSystem.ScriptableObjects
{
	[CreateAssetMenu(menuName = Constants.MenuNames.Objectives + "/Survive All Waves Objectives")]
	public class SurviveAllWavesObjectiveSO : ObjectiveSO
	{
		[field: Header("Listening Channels")]
		[field: SerializeField]
		public VoidEventChannelSO AllEnemiesDefeatedEventChannel { get; private set; } = default!;

		private void OnEnable()
		{
			AllEnemiesDefeatedEventChannel.Raised += Complete;
		}

		private void OnDisable()
		{
			AllEnemiesDefeatedEventChannel.Raised -= Complete;
		}
	}
}
