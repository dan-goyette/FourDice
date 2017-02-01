using System;
using Assets.Scripts.DomainModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnityEngine;

namespace FourDiceGame.Test
{
	[TestClass]
	public class UnitTest2
	{

		[TestMethod]
		public void SnapToTest()
		{
			var snapped = FourDiceUtils.SnapTo( new Vector3( 12, -46, 118 ), 25 );
			Assert.AreEqual( 0, snapped.x );
			Assert.AreEqual( -50, snapped.y );
			Assert.AreEqual( 125, snapped.z );
			

			snapped = FourDiceUtils.SnapTo( new Vector3( 12, 13, 14 ), 25 );
			Assert.AreEqual( 0, snapped.x );
			Assert.AreEqual( 25, snapped.y );
			Assert.AreEqual( 25, snapped.z );

			snapped = FourDiceUtils.SnapTo( new Vector3( 12.5f, 12.4f, 12.6f ), 25 );
			Assert.AreEqual( 25, snapped.x );
			Assert.AreEqual( 0, snapped.y );
			Assert.AreEqual( 25, snapped.z );

			snapped = FourDiceUtils.SnapTo( new Vector3( 0,0,0 ), 100 );
			Assert.AreEqual( 0, snapped.x );
			Assert.AreEqual( 0, snapped.y );
			Assert.AreEqual( 0, snapped.z );


			snapped = FourDiceUtils.SnapTo( new Vector3( -271, 0.004f, 91 ), 90 );
			Assert.AreEqual( -270, snapped.x );
			Assert.AreEqual( 0, snapped.y );
			Assert.AreEqual( 90, snapped.z );
		}
	}
}
