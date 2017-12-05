namespace Mapbox.Examples
{
	using Mapbox.Unity.Location;
	using Mapbox.Unity.Utilities;
	using Mapbox.Unity.Map;
	using UnityEngine;
	using UnityEngine.AI;
	using Mapbox.Unity.Ar;

	[RequireComponent(typeof(NavMeshAgent), typeof(LineRenderer))]
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
		[SerializeField]
		private GameObject directionPrefab;

		[SerializeField]
		private float tileSpacing=2;

		float elapsed = 0.0f;

		NavMeshPath path;
		NavMeshAgent _agent;
		LineRenderer _line;

		Location _agentSourceLocation;

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

		[SerializeField]
		FixedLocationSynchronizationContextBehaviour _syncContext;
		public FixedLocationSynchronizationContextBehaviour SyncContext
		{
			private get
			{
				return _syncContext;
			}
			set
			{
				if (_syncContext != null)
				{
					_syncContext.OnAlignmentAvailable -= LocationProvider_OnAlignmentAvailable;

				}
				_syncContext = value;
				_syncContext.OnAlignmentAvailable += LocationProvider_OnAlignmentAvailable;
			}
		}

		Vector3 _targetPosition;
		private void Awake()
		{
			_agent = GetComponent<NavMeshAgent>();
			_line = GetComponent< LineRenderer > ();
			_map.OnInitialized += Map_OnInitialized;
		}
		void Start()
		{
			LocationProvider.OnLocationUpdated += LocationProvider_OnLocationUpdated;
			DestinationPointLocationProvider.Instance.OnLocationUpdated += DestinationProvider_OnLocationUpdated;
			SyncContext.OnAlignmentAvailable += LocationProvider_OnAlignmentAvailable;
			path = new NavMeshPath();
		}

		void Map_OnInitialized()
		{
			_isInitialized = true;
		}

		void OnDestroy()
		{
			if (LocationProvider != null)
			{
				LocationProvider.OnLocationUpdated -= LocationProvider_OnLocationUpdated;
				DestinationPointLocationProvider.Instance.OnLocationUpdated -= DestinationProvider_OnLocationUpdated;
			}
		}

		void LocationProvider_OnAlignmentAvailable(Alignment alignment)
		{
			Debug.Log("Alignment complete");

			// Need this to place the agent correctly. Otherwise NavMesh complains of 
			_agent.Warp(transform.position);
		}

		void LocationProvider_OnLocationUpdated(Location location)
		{
			Debug.Log("Agent location updated " + location.LatitudeLongitude.x + " , " + location.LatitudeLongitude.y);
			if (_isInitialized)
			{
				_agentSourceLocation = location;
				transform.position = _map.Root.TransformPoint(Conversions.GeoToWorldPosition(
					_agentSourceLocation.LatitudeLongitude,
					_map.CenterMercator,
					_map.WorldRelativeScale).ToVector3xz());
				Debug.Log("Agent location position updated " + transform.position.ToString());
			}
		}

		void DestinationProvider_OnLocationUpdated(Location location)
		{
			if (_isInitialized)
			{
				_targetPosition = _map.Root.TransformPoint(
					Conversions.GeoToWorldPosition(location.LatitudeLongitude,
												   _map.CenterMercator,
												   _map.WorldRelativeScale).ToVector3xz());

				////Debug sphere to check the position. 
				//var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				//go.transform.localPosition = _targetPosition;
				//_targetPosition = go.transform.position;

				Debug.Log("Agent Destination updated " + _targetPosition);
				_agent.destination = _targetPosition;
				NavMesh.CalculatePath(transform.position, _targetPosition, NavMesh.AllAreas, path);
				DrawPath(path);
				_isPathSet = true;
			}
		}

		void DrawPath(NavMeshPath navPath)
		{

			//If the path has 1 or no corners, there is no need to draw the line
			if (navPath.corners.Length < 2)
			{
				return;
			}

			// Set the array of positions to the amount of corners...
			_line.positionCount = navPath.corners.Length;
			Quaternion planerot = Quaternion.identity;
			for (int i = 0; i < navPath.corners.Length; i++)
			{
				// Go through each corner and set that to the line renderer's position...
				_line.SetPosition(i, navPath.corners[i]);
				float distance = 0;
				Vector3 offsetVector = Vector3.zero;
				if (i < navPath.corners.Length - 1)
				{
					//plane rotation calculation
					offsetVector = navPath.corners[i + 1] - navPath.corners[i];
					planerot = Quaternion.LookRotation(offsetVector);
					distance = Vector3.Distance(navPath.corners[i + 1], navPath.corners[i]);
					if (distance < tileSpacing)
						continue;

					planerot = Quaternion.Euler(90, planerot.eulerAngles.y, planerot.eulerAngles.z);

					//plane position calculation
					float newSpacing = 0;
					for (int j = 0; j < distance / tileSpacing; j++)
					{
						newSpacing += tileSpacing;
						var normalizedVector = offsetVector.normalized;
						var position = navPath.corners[i] + newSpacing * normalizedVector;
						GameObject go = Instantiate(directionPrefab, position, planerot);
					}
				}
				else
				{
					GameObject go = Instantiate(directionPrefab, navPath.corners[i], planerot);
				}
			}
		}

		void Update()
		{
			if (_isPathSet)
				Debug.DrawLine(transform.position, _targetPosition, Color.red);
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
