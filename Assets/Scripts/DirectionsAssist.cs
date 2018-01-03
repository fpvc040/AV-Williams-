using UnityEngine;
namespace Mapbox.Examples
{
	public class DirectionsAssist : MonoBehaviour
	{
		//the camera the directons assist arrow should follow
		[SerializeField]
		Transform _positionTarget;

		[SerializeField]
		SpriteRenderer _spriteRenderer;

		[SerializeField]
		float _height;

		Color _originalColor;

		void Start()
		{
			_originalColor = _spriteRenderer.color;
			_originalColor.a = .05f;

			// FIXME: This was for resetting the height of the sprite, so that it never went below the ground or too high.
			//UnityARSessionNativeInterface.ARAnchorAddedEvent += AnchorAdded;
		}

		//void AnchorAdded(ARPlaneAnchor anchorData)
		//{
		//	_lastHeight = UnityARMatrixOps.GetPosition(anchorData.transform).y;
		//}

		// TODO: if we're not doing world scale navigation, then this should just 
		// point in the direction of the next manuever. In this case, we would use
		// the player avatar transform, rather than the camera (same coordinate system).
		void Update()
		{
			var applicationState = IndoorMappingDemo.ApplicationUIManager.Instance.CurrentState;

			// Display directions assist only during navigation.
			if (applicationState == IndoorMappingDemo.ApplicationState.AR_Navigation)
			{
				_spriteRenderer.gameObject.SetActive(true);
				var targetForward = _positionTarget.forward;
				targetForward.y = 0f;
				targetForward.Normalize();

				var position = _positionTarget.position + (targetForward * 2f);
				position.y = _height;
				transform.position = position;

				var forward = RoutingManager.Instance.CurrentTarget - transform.position;
				forward.y = 0f;
				transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

				//Debug.Log("Arrow Forward : " + forward);

				var red = Color.red;
				red.a = .5f;
				var color = Color.Lerp(red, _originalColor, Mathf.Clamp01(Vector3.Dot(targetForward, transform.forward)));
				_spriteRenderer.color = color;
			}
			else
			{
				_spriteRenderer.gameObject.SetActive(false);
			}

		}
	}
}