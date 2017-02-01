using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.DomainModel
{
	public static class FourDiceUtils
	{
		private static System.Random _random;
		public static System.Random Random
		{
			get
			{
				if ( _random == null ) {
					_random = new System.Random();
				}
				return _random;
			}
		}

		public static Vector3 SnapTo( Vector3 current, float interval )
		{
			var retval = new Vector3();

			var minActual = Mathf.Min( current.x, current.y, current.z );

			float minIntervalBound = (int)(minActual / interval) * interval;
			if ( minActual < 0 ) {
				minIntervalBound -= interval;
			}

			bool xDone = false;
			bool yDone = false;
			bool zDone = false;

			float lastInterval = minIntervalBound;

			while ( !xDone || !yDone || !zDone ) {
				float currentBound = lastInterval + interval;
				if ( current.x >= lastInterval && current.x < lastInterval + (interval / 2f)) {
					retval.x = lastInterval;
					xDone = true;
				}
				if ( current.x >= lastInterval + (interval / 2f) && current.x < currentBound ) {
					retval.x = currentBound;
					xDone = true;
				}

				if ( current.y >= lastInterval && current.y < lastInterval + (interval / 2f) ) {
					retval.y = lastInterval;
					yDone = true;
				}
				if ( current.y >= lastInterval + (interval / 2f) && current.y < currentBound ) {
					retval.y = currentBound;
					yDone = true;
				}

				if ( current.z >= lastInterval && current.z < lastInterval + (interval / 2f) ) {
					retval.z = lastInterval;
					zDone = true;
				}
				if ( current.z >= lastInterval + (interval / 2f) && current.z < currentBound ) {
					retval.z = currentBound;
					zDone = true;
				}


				lastInterval = currentBound;
			}

			return retval;

		}
	}
}
