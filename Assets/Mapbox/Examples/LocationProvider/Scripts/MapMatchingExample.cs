﻿namespace Mapbox.Examples
{
	using Mapbox.Unity;
	using Mapbox.Utils;
	using Mapbox.Unity.Utilities;
	using Mapbox.Unity.Map;
	using System.Collections.Generic;
	using Mapbox.MapMatching;
	using UnityEngine;
	using System.Linq;

	public class MapMatchingExample : MonoBehaviour
	{
		[SerializeField]
		AbstractMap _map;

		[SerializeField]
		LineRenderer _lineRenderer;

		[SerializeField]
		PlotRoute _originalRoute;

		[SerializeField]
		Profile _profile;

		[SerializeField]
		float _height;

		MapMatcher _mapMatcher;

		void Awake()
		{
			_mapMatcher = MapboxAccess.Instance.MapMatcher;
		}

		[ContextMenu("Test")]
		public void Match()
		{
			var resource = new MapMatchingResource();

			var coordinates = new List<Vector2d>();
			foreach (var position in _originalRoute.Positions)
			{
				var coord = position.GetGeoPosition(_map.CenterMercator, _map.WorldRelativeScale);
				coordinates.Add(coord);
				Debug.Log("MapMatchingExample: " + coord);
			}

			if (coordinates.Count < 2)
			{
				Debug.Log("Need at least two coordinates for map matching.");
			}
			else
			{
				//API allows for max 100 coordinates, take newest
				resource.Coordinates = coordinates.Skip(System.Math.Max(0, coordinates.Count - 100)).ToArray();
				resource.Profile = _profile;
				_mapMatcher.Match(resource, HandleMapMatchResponse);
			}
		}

		void HandleMapMatchResponse(MapMatching.MapMatchingResponse response)
		{
			if (response.HasMatchingError)
			{
				Debug.LogError("MapMatchingExample: " + response.MatchingError);
				return;
			}

			if (response.HasRequestError)
			{
				foreach (var exception in response.RequestExceptions)
				{
					Debug.LogError("MapMatchingExample: " + exception);
				}
				return;
			}

			_lineRenderer.positionCount = 0;
			for (int i = 0, responseTracepointsLength = response.Tracepoints.Length; i < responseTracepointsLength; i++)
			{
				var point = response.Tracepoints[i];

				// Tracepoints can be null, so let's avoid trying to process those outliers.
				// see https://www.mapbox.com/api-documentation/#match-response-object
				if (point == null)
				{
					continue;
				}

				_lineRenderer.positionCount++;
				Debug.Log("MapMatchingExample: " + point.Name);
				Debug.Log("MapMatchingExample: " + point.Location);
				Debug.Log("MapMatchingExample: " + point.MatchingsIndex);
				Debug.Log("MapMatchingExample: " + point.WaypointIndex);
				var position = Conversions.GeoToWorldPosition(point.Location, _map.CenterMercator, _map.WorldRelativeScale).ToVector3xz();
				position.y = _height;
				_lineRenderer.SetPosition(_lineRenderer.positionCount - 1, position);
			}
		}
	}
}