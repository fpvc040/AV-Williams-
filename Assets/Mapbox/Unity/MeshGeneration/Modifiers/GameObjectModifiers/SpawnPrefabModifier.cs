namespace Mapbox.Unity.MeshGeneration.Modifiers
{
	using System;
	using UnityEngine;
	using Mapbox.Unity.MeshGeneration.Data;
	using Mapbox.Unity.MeshGeneration.Components;
	using Mapbox.Unity.MeshGeneration.Interfaces;

	[CreateAssetMenu(menuName = "Mapbox/Modifiers/Spawn Prefab Modifier")]
	public class SpawnPrefabModifier : GameObjectModifier
	{
		enum SpawnLocation
		{
			Top,
			Front,
			Center
		};

		[SerializeField]
		private SpawnLocation _spawnLocation;

		[SerializeField]
		private string _prefabLocation;

		//[SerializeField]
		//private GameObject _prefab;

		[SerializeField]
		private bool _scaleDownWithWorld = false;

		public override void Run(VectorEntity ve, UnityTile tile)
		{
			if (ve.Feature.Properties["destination-type"].ToString() == "conference-room")
			{
				string prefabName = "Prefabs/" + ve.Feature.Properties["name"].ToString() + "Model";
				//Debug.Log("PrefabName : " + prefabName);

				var scale = tile.TileScale;
				int selpos = ve.Feature.Points[0].Count / 2;
				var met = ve.Feature.Points[0][selpos];
				var prefabGO = (GameObject)Instantiate(Resources.Load(prefabName));
				prefabGO.name = prefabName;
				met.y = 7 * scale;
				prefabGO.transform.position = met;
				prefabGO.transform.SetParent(ve.GameObject.transform, false);

				var destPosition = prefabGO.transform.position;
				destPosition.y = 0;

				Location.DestinationPointData locationData = new Location.DestinationPointData();

				locationData.SetDestinationPoint(Convert.ToInt32(ve.Feature.Properties["id"]), ve.Feature.Properties["name"].ToString(), destPosition);

				Location.DestinationPointLocationProvider.Instance.Register(locationData);
			}
		}
	}
}
