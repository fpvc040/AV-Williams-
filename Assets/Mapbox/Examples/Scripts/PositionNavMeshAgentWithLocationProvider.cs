namespace Mapbox.Examples
{
	using Mapbox.Unity.Location;
	using Mapbox.Unity.Utilities;
	using Mapbox.Unity.Map;
	using UnityEngine;
	using UnityEngine.AI;

	[RequireComponent(typeof(NavMeshAgent))]
	public class PositionNavMeshAgentWithLocationProvider : MonoBehaviour
	{
		[SerializeField]
		private AbstractMap _map;

		/// <summary>
		/// The rate at which the transform's position tries catch up to the provided location.
		/// </summary>
		[SerializeField]
		float _positionFollowFactor;

		bool _isInitialized;
		bool _isPathSet = false;

		float elapsed = 0.0f;

		NavMeshPath path;
		NavMeshAgent _agent;

		/// <summary>
		/// The location provider.
		/// This is public so you change which concrete <see cref="T:Mapbox.Unity.Location.ILocationProvider"/> to use at runtime.
		/// </summary>
		ILocationProvider _locationProvider;
		public ILocationProvider LocationProvider
		{
			private get
			{
				if (_locationProvider == null)
				{
					_locationProvider = LocationProviderFactory.Instance.FixedLocationProvider;
				}

				return _locationProvider;
			}
			set
			{
				if (_locationProvider != null)
				{
					_locationProvider.OnLocationUpdated -= LocationProvider_OnLocationUpdated;

				}
				_locationProvider = value;
				_locationProvider.OnLocationUpdated += LocationProvider_OnLocationUpdated;
			}
		}

		Vector3 _targetPosition;
		private void Awake()
		{
			_agent = GetComponent<NavMeshAgent>();
		}
		void Start()
		{
			LocationProvider.OnLocationUpdated += LocationProvider_OnLocationUpdated;
			DestinationPointLocationProvider.Instance.OnLocationUpdated += DestinationProvider_OnLocationUpdated;
			_map.OnInitialized += () => _isInitialized = true;
			path = new NavMeshPath();
		}

		void OnDestroy()
		{
			if (LocationProvider != null)
			{
				LocationProvider.OnLocationUpdated -= LocationProvider_OnLocationUpdated;
				DestinationPointLocationProvider.Instance.OnLocationUpdated -= DestinationProvider_OnLocationUpdated;
			}
		}

		void LocationProvider_OnLocationUpdated(Location location)
		{
			Debug.Log("Agent location updated " + location.LatitudeLongitude.x + " , " + location.LatitudeLongitude.y);
			if (_isInitialized)
			{
				transform.position = Conversions.GeoToWorldPosition(location.LatitudeLongitude,
																 _map.CenterMercator,
																 _map.WorldRelativeScale).ToVector3xz();
				_agent.Warp(transform.position);
			}
		}
		void DestinationProvider_OnLocationUpdated(IDestinationPoint location)
		{

			if (_isInitialized)
			{
				//NavMesh.CalculatePath(transform.position, location.Location, int areaMask, AI.NavMeshPath path);
				_targetPosition = location.Location;
				_isPathSet = true;
				Debug.Log("Agent Destination updated " + _targetPosition);
				_agent.destination = location.Location;
			}
		}

		void Update()
		{
			//transform.position = Vector3.Lerp(transform.position, _targetPosition, Time.deltaTime * _positionFollowFactor);

			elapsed += Time.deltaTime;
			if (elapsed > 1.0f && _isPathSet)
			{
				elapsed -= 1.0f;
				NavMesh.CalculatePath(transform.position, _targetPosition, NavMesh.AllAreas, path);
			}
			if (_isPathSet)
			{
				for (int i = 0; i < path.corners.Length - 1; i++)
					Debug.DrawLine(path.corners[i], path.corners[i + 1], Color.red);
			}

		}
	}
}
