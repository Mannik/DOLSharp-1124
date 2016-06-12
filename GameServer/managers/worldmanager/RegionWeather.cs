﻿/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Linq;

namespace DOL.GS
{
	/// <summary>
	/// RegionWeather keeps track of ongoing Weather in a Region
	/// </summary>
	public sealed class RegionWeather
	{
		/// <summary>
		/// Region Reference for this Weather 
		/// </summary>
		public Region Region { get; private set; }
		
		/// <summary>
		/// Weather Position on X axis (from West to East)
		/// </summary>
		public uint Position { get; set; }
		
		/// <summary>
		/// Weather Width, happening on the Whole Y Axis
		/// Width and Speed can change Weather Duration
		/// </summary>
		public uint Width { get; set; }
		
		/// <summary>
		/// Game Unit by Second the Weather travels
		/// </summary>
		public ushort Speed { get; set; }
		
		/// <summary>
		/// Weather Fog and Rain Intensity
		/// Value above 120 give weird results
		/// </summary>
		public ushort Intensity { get; set; }
		
		/// <summary>
		/// Fog Diffusion Value
		/// </summary>
		public ushort FogDiffusion { get; set; }
		
		/// <summary>
		/// Time this Weather Started
		/// </summary>
		public long StartTime { get; set; }
		
		/// <summary>
		/// Time this Weather should be checked again
		/// </summary>
		public long DueTime { get; set; }
		
		/// <summary>
		/// Weather Start Position in this Region
		/// </summary>
		private uint WeatherStartPosition { get; set; }
		
		/// <summary>
		/// Weather Stop Position in this Region
		/// </summary>
		private uint WeatherStopPosition { get; set; }
		
		/// <summary>
		/// Create a new Instance of <see cref="RegionWeather"/>
		/// </summary>
		public RegionWeather(Region Region)
		{
			this.Region = Region;
			WeatherStartPosition = (uint)Math.Max(0, Region.Zones.Min(z => z.XOffset) - Width);
			WeatherStopPosition = (uint)Math.Max(0, Region.Zones.Max(z => z.XOffset + z.Width) + Width);
		}
		
		/// <summary>
		/// Reset Weather Values
		/// </summary>
		public void Clear()
		{
			Position = 0;
			Width = 0;
			Speed = 0;
			Intensity = 0;
			FogDiffusion = 0;
			StartTime = 0;
			DueTime = 0;
		}
		
		/// <summary>
		/// Create Weather for this Region
		/// </summary>
		/// <param name="Width">Width of the Weather (Game Unit, Min 15000)</param>
		/// <param name="Speed">Speed of the Weather (Unit/s, Min 100)</param>
		/// <param name="Intensity">Intensity of the Weather (Max 120)</param>
		/// <param name="FogDiffusion">Fog Diffusion of the Weather (Min 16000)</param>
		/// <param name="StartTime">StartTime of the Weather (StopWatch ms Timestamp)</param>
		public void CreateWeather(uint Width, ushort Speed, ushort Intensity, ushort FogDiffusion, long StartTime)
		{
			
			Position = WeatherStartPosition;
			this.Width = Math.Max(15000, Width);
			this.Speed = Math.Max((ushort)100, Speed);
			this.Intensity = Math.Min((ushort)120, Intensity);
			this.FogDiffusion = Math.Max((ushort)16000, FogDiffusion);
			this.StartTime = StartTime;
			DueTime = StartTime + Convert.ToInt64(Math.Ceiling((WeatherStopPosition - WeatherStartPosition / (double)Speed) * 1000));
		}
		
		/// <summary>
		/// Create Random Weather for this Region
		/// </summary>
		/// <param name="StartTime">StartTime of the Weather</param>
		public void CreateWeather(long StartTime)
		{
			CreateWeather(
				(uint)Util.Random(25000, 90000),
				(ushort)Util.Random(100, 700),
				(ushort)Util.Random(30, 110),
				(ushort)Util.Random(16000, 32000),
				StartTime
			);
		}
		
		/// <summary>
		/// Get Current Position of this Storm
		/// </summary>
		/// <param name="CurrentTime"></param>
		/// <returns></returns>
		public uint CurrentPosition(long CurrentTime)
		{
			return Position + Convert.ToUInt32(Math.Ceiling((CurrentTime - StartTime / 1000.0) * Speed));
		}
		
		/// <summary>
		/// Display Region Weather Info
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("[RegionWeather Region={0}, Position={1}, Width={2}, Speed={3}, Intensity={4}, FogDiffusion={5}, StartTime={6}, DueTime={7}, WeatherStartPosition={8}, WeatherStopPosition={9}]", Region.ID, Position, Width, Speed, Intensity, FogDiffusion, StartTime, DueTime, WeatherStartPosition, WeatherStopPosition);
		}

	}
}
