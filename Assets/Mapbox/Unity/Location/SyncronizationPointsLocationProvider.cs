namespace Mapbox.Unity.Location
{
	using UnityEngine;
	using System.Collections.Generic;
	using IndoorMappingDemo;

	public class SyncronizationPointsLocationProvider : AbstractLocationProvider
	{

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
				//Debug.Log("Enqueue " + _syncronizationPointQueue.Count);
			}
		}

		protected IFixedLocation Dequeue()
		{
			lock (_syncLock)
			{
				//Debug.Log("Dequeue " + _syncronizationPointQueue.Count);
				return _syncronizationPointQueue.Dequeue();
			}
		}

		public void Register(IFixedLocation locationProvider)
		{
			Enqueue(locationProvider);
		}

		private void Update()
		{
			//HACK : To add buttons in increasing order. 

			if (Count < 8)
			{
				return;
			}
			else
			{
				while (Count > 0)
				{
					var locationProvider = Dequeue();
					if (!_syncronizationPoints.ContainsKey(locationProvider.LocationId))
					{
						//Debug.Log("Registering id : " + locationProvider.LocationId);
						_syncronizationPoints.Add(locationProvider.LocationId, locationProvider);
					}
				}

				for (int i = 0; i < 8; i++)
				{
					ApplicationUIManager.Instance.AddToSyncPointUI(i, _syncronizationPoints[i].LocationName, OnSyncRequested);
				}
			}
		}

		public void OnSyncRequested(int id)
		{
			Debug.Log("Pressed button");
			SendLocation(_syncronizationPoints[id].CurrentLocation);
			ApplicationUIManager.Instance.OnStateChanged(ApplicationState.SyncPoint_Calibration);
		}
	}
}