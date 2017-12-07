using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class SyncLocationInteraction : MonoBehaviour
{
	int _locationId;

	public Button _syncButton;
	public Text _syncLocationText;
	//public Image _syncLocationImage;

	public event Action<int> OnSyncLocationInteraction = delegate { };

	void Awake()
	{
		_syncButton.onClick.AddListener(SyncLocation);
	}

	public void Register(int location, string label, Action<int> callback, string color = null)
	{
		//_syncButton.onClick.AddListener(SyncLocation);
		_locationId = location;

		OnSyncLocationInteraction += callback;

		_syncLocationText.text = string.IsNullOrEmpty(label) ? location.ToString() : label;
	}

	private void SyncLocation()
	{
		Debug.Log("Button OnClick");
		OnSyncLocationInteraction(_locationId);
	}
}
