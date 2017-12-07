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

	public Sprite _syncPointIcon;
	public Sprite _conferenceRoomIcon;
	public Sprite _phoneRoomIcon;

	public event Action<int> OnSyncLocationInteraction = delegate { };

	void Awake()
	{
		_syncButton.onClick.AddListener(SyncLocation);
	}

	public void Register(int location, string label, Action<int> callback, string type = null)
	{
		//_syncButton.onClick.AddListener(SyncLocation);
		_locationId = location;

		OnSyncLocationInteraction += callback;

		_syncLocationText.text = string.IsNullOrEmpty(label) ? location.ToString() : label;

		if (type != null)
		{
			Debug.Log("Setting types");
			if (type == "conference-room")
			{
				_syncLocationImage.sprite = _conferenceRoomIcon;
			}
			if (type == "phone-room")
			{
				_syncLocationImage.sprite = _phoneRoomIcon;
			}
		}
	}

	private void SyncLocation()
	{
		Debug.Log("Button OnClick");
		OnSyncLocationInteraction(_locationId);
	}
}
