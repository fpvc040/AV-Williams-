﻿namespace Mapbox.IndoorMappingDemo
{
	using System.Collections;
	using System.Collections.Generic;
	using System;
	using UnityEngine;

	public enum ApplicationState
	{
		AR_Calibration,
		SyncPoint_Calibration,
		Destination_Selection,
		AR_Navigation
	}

	public class ApplicationUIManager : MonoBehaviour
	{

		/// <summary>
		/// The singleton instance of this factory.
		/// </summary>
		private static ApplicationUIManager _instance;
		public static ApplicationUIManager Instance
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

		[SerializeField]
		private GameObject _arCalibrationUI;

		[SerializeField]
		private GameObject _syncPointCalibrationUI;

		[SerializeField]
		private GameObject _destinationSelectionUI;

		private ApplicationState _applicationState;

		[SerializeField]
		private FixedLocationPointUI _destinationSelectionUIManager;

		[SerializeField]
		private FixedLocationPointUI _syncPointCalibrationUIManager;



		// Any actions that need to be triggered on Application State change. 
		public event Action<ApplicationState> StateChanged = delegate { };

		// Use this for initialization
		void Awake()
		{
			if (Instance != null)
			{
				DestroyImmediate(gameObject);
				return;
			}
			Instance = this;
			DontDestroyOnLoad(gameObject);

			_applicationState = ApplicationState.SyncPoint_Calibration;

			if (_arCalibrationUI != null)
			{
				_arCalibrationUI.SetActive(false);
			}

			if (_destinationSelectionUI != null)
			{
				_destinationSelectionUI.SetActive(false);
			}

			if (_syncPointCalibrationUI != null)
			{
				Debug.Log("Sync Point UI");
				_syncPointCalibrationUI.SetActive(true);
			}
		}

		public void OnStateChanged(ApplicationState currentState)
		{
			switch (currentState)
			{
				case ApplicationState.AR_Calibration:
					_applicationState = ApplicationState.SyncPoint_Calibration;
					break;
				case ApplicationState.Destination_Selection:
					_applicationState = ApplicationState.AR_Navigation;
					_destinationSelectionUI.SetActive(false);
					break;
				case ApplicationState.SyncPoint_Calibration:
					_applicationState = ApplicationState.Destination_Selection;
					_syncPointCalibrationUI.SetActive(false);
					_destinationSelectionUI.SetActive(true);
					break;
				default:
					break;
			}

			//Notify subscribers application state changed. 
			StateChanged(_applicationState);
		}

		public void AddToDestinationPointUI(int id, string label, Action<int> callback)
		{
			_destinationSelectionUIManager.RegisterUI(id, label, callback);
		}

		public void AddToSyncPointUI(int id, string label, Action<int> callback)
		{
			_syncPointCalibrationUIManager.RegisterUI(id, label, callback);
		}
		// Update is called once per frame
		void Update()
		{

		}
	}
}