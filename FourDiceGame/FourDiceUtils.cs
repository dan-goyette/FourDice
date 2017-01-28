﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FourDiceGame
{
	public static class FourDiceUtils
	{
		private static Random _random;
		public static Random Random
		{
			get
			{
				if ( _random == null ) {
					_random = new Random();
				}
				return _random;
			}
		}
	}
}