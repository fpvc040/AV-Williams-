namespace Mapbox.Unity.MeshGeneration.Modifiers
{
	using System;
	using UnityEngine;
	using Mapbox.Utils;
	using Mapbox.Unity.Utilities;
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
		string _destinationLocationIdKey;

		[SerializeField]
		string _destinationLocationNameKey;

		[SerializeField]
		string _destinationLocationPositionKey;

		[SerializeField]
		string _destinationLocationHeadingKey;

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

				var destPosition = met;
				destPosition.y = 0;

				Location.DestinationPointData locationData = new Location.DestinationPointData();
				var feature = ve.Feature;

				int id = 0;
				float heading = 0.0f;
				string locationName = string.Empty;
				string latitudeLongitudeString;
				Vector2d latitudeLongitude = Vector2d.zero;

				if (feature.Properties.ContainsKey(_destinationLocationIdKey))
				{
					if (!int.TryParse(feature.Properties[_destinationLocationIdKey].ToString(), out id))
					{
						Debug.Log("No property with key : " + _destinationLocationIdKey + "found!");
					}
				}

				if (feature.Properties.ContainsKey(_destinationLocationNameKey))
				{
					locationName = feature.Properties[_destinationLocationNameKey].ToString();
					if (string.IsNullOrEmpty(locationName))
					{
						Debug.Log("No property with key : " + _destinationLocationNameKey + "found!");
					}
				}

				if (feature.Properties.ContainsKey(_destinationLocationPositionKey))
				{
					latitudeLongitudeString = feature.Properties[_destinationLocationPositionKey].ToString();
					latitudeLongitude = Conversions.StringToLatLon(latitudeLongitudeString);
				}

				if (feature.Properties.ContainsKey(_destinationLocationHeadingKey))
				{
					if (!float.TryParse(feature.Properties[_destinationLocationHeadingKey].ToString(), out heading))
					{
						Debug.Log("No property with key : " + _destinationLocationHeadingKey + "found!");
					}
				}

				locationData.SetLocation(id, locationName, latitudeLongitude, heading);

				Location.DestinationPointLocationProvider.Instance.Register(locationData);
			}
		}
	}
}
