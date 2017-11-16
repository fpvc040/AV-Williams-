using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace Mapbox.Unity.Location
{
	using UnityEngine;
	using System.Collections.Generic;
	using IndoorMappingDemo;
	using System;

	public interface IDestinationPoint
	{
		int LocationId { get; }
		string LocationName { get; }
		Vector3 Location { get; }
	}

	public class DestinationPointData : IDestinationPoint
	{
		private int _locationId;
		public int LocationId
		{
			get
			{
				return _locationId;
			}
		}

		private Vector3 _location;
		public Vector3 Location
		{
			get
			{
				return _location;
			}
		}

		private string _locationName;
		public string LocationName
		{
			get
			{
				return _locationName;
			}
		}

		public void SetDestinationPoint(int id, string name, Vector3 location)
		{
			_locationId = id;
			_locationName = name;
			_location = location;
		}
	}


	public class DestinationPointLocationProvider : MonoBehaviour
	{
		public event Action<IDestinationPoint> OnLocationUpdated = delegate { };

		protected void SendLocation(IDestinationPoint location)
		{
			OnLocationUpdated(location);
		}
		/// <summary>
		/// The singleton instance of this factory.
		/// </summary>
		private static DestinationPointLocationProvider _instance;
		public static DestinationPointLocationProvider Instance
		{
			get
			{
				return _instance;
			}

			private set
			{
				_instance = value;
			}
		}

		private object _syncLock = new object();
		protected Dictionary<int, IDestinationPoint> _syncronizationPoints = new Dictionary<int, IDestinationPoint>();
		protected Queue<IDestinationPoint> _syncronizationPointQueue = new Queue<IDestinationPoint>();

		private void Awake()
		{
			if (Instance != null)
			{
				DestroyImmediate(gameObject);
				return;
			}
			Instance = this;
			DontDestroyOnLoad(gameObject);

			if (_syncronizationPoints != null)
			{
				_syncronizationPoints = new Dictionary<int, IDestinationPoint>();
			}
			if (_syncronizationPointQueue != null)
			{
				_syncronizationPointQueue = new Queue<IDestinationPoint>();
			}
		}

		protected int Count
		{
			get
			{
				lock (_syncLock)
				{
					return _syncronizationPointQueue.Count;
				}
			}
		}

		protected void Enqueue(IDestinationPoint locationProvider)
		{
			lock (_syncLock)
			{
				_syncronizationPointQueue.Enqueue(locationProvider);
				//Debug.Log("Enqueue " + _syncronizationPointQueue.Count);
			}
		}

		protected IDestinationPoint Dequeue()
		{
			lock (_syncLock)
			{
				//Debug.Log("Dequeue " + _syncronizationPointQueue.Count);
				return _syncronizationPointQueue.Dequeue();
			}
		}

		public void Register(IDestinationPoint locationProvider)
		{
			Enqueue(locationProvider);
		}

		private void Update()
		{
			while (Count > 0)
			{
				var locationProvider = Dequeue();
				if (!_syncronizationPoints.ContainsKey(locationProvider.LocationId))
				{
					_syncronizationPoints.Add(locationProvider.LocationId, locationProvider);

					ApplicationUIManager.Instance.AddToDestinationPointUI(locationProvider.LocationId, locationProvider.LocationName, OnSyncRequested);
				}
			}
		}

		public void OnSyncRequested(int id)
		{
			Debug.Log("Pressed button");
			SendLocation(_syncronizationPoints[id]);
			ApplicationUIManager.Instance.OnStateChanged(ApplicationState.Destination_Selection);

		}
	}
}

