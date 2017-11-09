namespace Mapbox.Unity.Location
{
	using UnityEngine;
	using System.Collections.Generic;

	public class SyncronizationPointsLocationProvider : AbstractLocationProvider
	{
		// TODO : Make this an abstract class and create concrete implementations for manual/iBeacon stuff. 
		[SerializeField]
		public GameObject buttonPrefab;

		[SerializeField]
		public Transform contentPanel;

		[SerializeField]
		bool _sendEvent;

		[SerializeField]
		bool _registerEvent;

		private object _syncLock = new object();
		Dictionary<int, IFixedLocation> _syncronizationPoints = new Dictionary<int, IFixedLocation>();
		Queue<IFixedLocation> _syncronizationPointQueue = new Queue<IFixedLocation>();


		private void Awake()
		{
			if (_syncronizationPoints != null)
			{
				_syncronizationPoints = new Dictionary<int, IFixedLocation>();
			}
			if (_syncronizationPointQueue != null)
			{
				_syncronizationPointQueue = new Queue<IFixedLocation>();
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

		protected void Enqueue(IFixedLocation locationProvider)
		{
			lock (_syncLock)
			{
				_syncronizationPointQueue.Enqueue(locationProvider);
				Debug.Log("Enqueue " + _syncronizationPointQueue.Count);
			}
		}

		protected IFixedLocation Dequeue()
		{
			lock (_syncLock)
			{
				Debug.Log("Dequeue " + _syncronizationPointQueue.Count);
				return _syncronizationPointQueue.Dequeue();
			}
		}

		public void Register(IFixedLocation locationProvider)
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
					Debug.Log("Registering id : " + locationProvider.LocationId);
					_syncronizationPoints.Add(locationProvider.LocationId, locationProvider);

					if (buttonPrefab != null)
					{
						var syncButtonGO = Instantiate(buttonPrefab);
						syncButtonGO.transform.SetParent(contentPanel);

						var syncButton = syncButtonGO.GetComponent<SyncLocationInteraction>();
						syncButton.Register(locationProvider.LocationId, OnSyncRequested);
					}
				}
			}
		}

		public void OnSyncRequested(int id)
		{
			Debug.Log("Pressed button");
			SendLocation(_syncronizationPoints[id].CurrentLocation);
		}

		private void OnValidate()
		{
			// This is for testing purposes. 
			if (_sendEvent)
			{
				//var syncButtonGO = Instantiate(buttonPrefab);
				//syncButtonGO.transform.SetParent(contentPanel);

				//var syncButton = syncButtonGO.GetComponent<SyncLocationInteraction>();
				//syncButton.Register(0, OnSyncRequested);

				while (Count > 0)
				{
					var locationProvider = Dequeue();
					if (!_syncronizationPoints.ContainsKey(locationProvider.LocationId))
					{
						Debug.Log("Registering id : " + locationProvider.LocationId);
						_syncronizationPoints.Add(locationProvider.LocationId, locationProvider);

						if (buttonPrefab != null)
						{
							var syncButtonGO = Instantiate(buttonPrefab);
							syncButtonGO.transform.SetParent(contentPanel);

							var syncButton = syncButtonGO.GetComponent<SyncLocationInteraction>();
							syncButton.Register(locationProvider.LocationId, OnSyncRequested);
						}
					}
				}
				_sendEvent = false;
			}

			if (_registerEvent)
			{

				_registerEvent = false;
			}
		}
	}
}