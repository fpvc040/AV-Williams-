using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class SyncLocationInteraction : MonoBehaviour
{
	int _locationId;

	public Button _syncButton;
	public Text _syncLocationText;
	public Image _syncLocationImage;


	public event Action<int> OnSyncLocationInteraction = delegate { };

	void Start()
	{
		_syncButton.onClick.AddListener(SyncLocation);
	}

	public void Register(int location, Action<int> callback)
	{

		_locationId = location;
		OnSyncLocationInteraction += callback;

		_syncLocationText.text = location.ToString();
	}

	private void SyncLocation()
	{
		OnSyncLocationInteraction(_locationId);
	}
}
