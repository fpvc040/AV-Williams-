using UnityARInterface;
namespace Mapbox.Unity.Ar
{
	using Mapbox.Unity.Map;
	using Mapbox.Unity.Location;
	using UnityEngine;
	using Mapbox.Unity.Utilities;
	using UnityEngine.XR.iOS;
	using System;

	public class FixedLocationSynchronizationContextBehaviour : MonoBehaviour, ISynchronizationContext
	{
		[SerializeField]
		AbstractMap _map;

		[SerializeField]
		Transform _mapCamera;

		[SerializeField]
		SyncronizationPointsLocationProvider _locationProvider;

		[SerializeField]
		AbstractAlignmentStrategy _alignmentStrategy;

		float _lastHeight;
		float _lastHeading;

		public event Action<Alignment> OnAlignmentAvailable = delegate { };

		void Awake()
		{
			_alignmentStrategy.Register(this);
			_map.OnInitialized += Map_OnInitialized;
			ARInterface.planeAdded += ARInterface_PlaneAdded;
		}

		void OnDestroy()
		{
			_alignmentStrategy.Unregister(this);
			ARInterface.planeAdded -= ARInterface_PlaneAdded;
		}

		void Map_OnInitialized()
		{
			_map.OnInitialized -= Map_OnInitialized;
			_locationProvider.OnLocationUpdated += _locationProvider_OnLocationUpdated;
		}

		void _locationProvider_OnLocationUpdated(Unity.Location.Location location)
		{
			var heading = location.Heading;

			var alignment = new Alignment();
			var originalPosition = _map.Root.position;
			alignment.Rotation = -heading + _map.Root.localEulerAngles.y;

			// Rotate our offset by the last heading.
			var rotation = Quaternion.Euler(0, -heading, 0);
			alignment.Position = rotation * (-Conversions.GeoToWorldPosition(location.LatitudeLongitude,
																			 _map.CenterMercator,
																			 _map.WorldRelativeScale).ToVector3xz() + originalPosition);
			alignment.Position.y = _lastHeight;
			Debug.Log("Alignment");
			OnAlignmentAvailable(alignment);

			// Reset camera to avoid confusion.
			var mapCameraPosition = Vector3.zero;
			mapCameraPosition.y = _mapCamera.localPosition.y;
			var mapCameraRotation = Vector3.zero;
			mapCameraRotation.x = _mapCamera.localEulerAngles.x;
			_mapCamera.localPosition = mapCameraPosition;
			_mapCamera.eulerAngles = mapCameraRotation;
		}

		void ARInterface_PlaneAdded(UnityARInterface.BoundedPlane obj)
		{
			_lastHeight = obj.center.y;
		}
	}
}