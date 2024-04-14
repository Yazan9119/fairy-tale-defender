using BoundfoxStudios.FairyTaleDefender.Common;
using BoundfoxStudios.FairyTaleDefender.Infrastructure.Events.ScriptableObjects;
using BoundfoxStudios.FairyTaleDefender.Infrastructure.RuntimeSets;
using UnityEngine;

namespace BoundfoxStudios.FairyTaleDefender.Entities.Characters.Enemies
{
	[AddComponentMenu(Constants.MenuNames.Characters + "/" + nameof(LivingEnemyManager))]
	public class LivingEnemyManager : MonoBehaviour
	{
		[field: Header("References")]
		[field: SerializeField]
		public EnemyRuntimeSetSO LivingEnemies { get; private set; } = default!;

		[field: Header("Listening Channels")]
		[field: SerializeField]
		public EnemyEventChannelSO EnemySpawnedEventChannel { get; private set; } = default!;

		[field: SerializeField]
		public EnemyEventChannelSO EnemyDestroyedEventChannel { get; private set; } = default!;

		[field: SerializeField]
		public WaveSpawnedEventChannelSO WaveSpawnedEventChannel { get; private set; } = default!;

		[field: SerializeField]
		public VoidEventChannelSO GamePlayStartedEventChannel { get; private set; } = default!;

		[field: Header("Broadcasting Channels")]
		[field: SerializeField]
		private VoidEventChannelSO AllEnemiesDefeatedEventChannel { get; set; } = default!;

		private bool _levelHasMoreWaves = true;

		private void OnEnable()
		{
			EnemySpawnedEventChannel.Raised += EnemySpawned;
			EnemyDestroyedEventChannel.Raised += EnemyDestroyed;
			GamePlayStartedEventChannel.Raised += GamePlayStarted;
			WaveSpawnedEventChannel.Raised += WaveSpawned;
		}

		private void OnDisable()
		{
			EnemySpawnedEventChannel.Raised -= EnemySpawned;
			EnemyDestroyedEventChannel.Raised -= EnemyDestroyed;
			GamePlayStartedEventChannel.Raised -= GamePlayStarted;
			WaveSpawnedEventChannel.Raised -= WaveSpawned;

			LivingEnemies.Clear();
		}

		private void EnemySpawned(Enemy spawnedEnemy)
		{
			LivingEnemies.Add(spawnedEnemy);
		}

		private void EnemyDestroyed(Enemy destroyedEnemy)
		{
			LivingEnemies.Remove(destroyedEnemy);

			if (!LevelHasMoreEnemies())
			{
				AllEnemiesDefeatedEventChannel.Raise();
			}
		}

		private void WaveSpawned(WaveSpawnedEventChannelSO.EventArgs args)
		{
			_levelHasMoreWaves = args.LevelHasMoreWaves;
		}

		private void GamePlayStarted()
		{
			_levelHasMoreWaves = true;
			LivingEnemies.Clear();
		}

		private bool LevelHasMoreEnemies()
		{
			return LivingEnemies.Items.Count > 0 || _levelHasMoreWaves;
		}
	}
}
