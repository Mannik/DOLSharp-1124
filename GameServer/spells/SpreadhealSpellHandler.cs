/*
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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Spells
{
	/// <summary>
	/// 
	/// </summary>
	[SpellHandlerAttribute("SpreadHeal")]
	public class SpreadhealSpellHandler : HealSpellHandler
	{
		// constructor
		public SpreadhealSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}


		/// <summary>
		/// Heals targets group
		/// </summary>
		/// <param name="target"></param>
		/// <param name="amount">amount of hit points to heal</param>
		/// <returns>true if heal was done</returns>
		public override bool HealTarget (GameLiving target, int amount)
		{
			Dictionary<GameLiving, double> injuredTargets = new Dictionary<GameLiving, double>();
			GameLiving mostInjuredLiving = target;
			double mostInjuredPercent = mostInjuredLiving.Health / (float)mostInjuredLiving.MaxHealth;

			int minHealVariance;
			int maxHealVariance;
			int targetHealCap;

			CalculateHealVariance(out minHealVariance, out maxHealVariance);

			if (minHealVariance >= maxHealVariance)
			{
				targetHealCap = maxHealVariance;
			}
			else
			{
				targetHealCap = Util.Random(minHealVariance, maxHealVariance);
			}

			int groupHealCap = targetHealCap;

			Group group = target.Group;
			if (group != null)
			{
				groupHealCap *= group.MemberCount;
				targetHealCap *= 2;

				foreach (GameLiving living in group.GetMembersInTheGroup())
				{
					if (!living.IsAlive) continue;
					//heal only if target is in range
					if (target.IsWithinRadius(living, m_spell.Range))
					{
						double livingHealthPercent = living.Health / (double)living.MaxHealth;
						if (livingHealthPercent < 1)
						{
							injuredTargets.Add(living, livingHealthPercent);
							if (livingHealthPercent < mostInjuredPercent)
							{
								mostInjuredLiving = living;
								mostInjuredPercent = livingHealthPercent;
							}
						}
					}
				}
			}
			else
			{
				// heal caster
				if (mostInjuredPercent < 1)
					injuredTargets.Add(target, mostInjuredPercent);
			}


			if (mostInjuredPercent >= 1)
			{
				//all are healed, 1/2 power
				SendEffectAnimation(target, 0, false, 0);
				MessageToCaster("Your group is already fully healed!", eChatType.CT_SpellResisted);
				return false;
			}

			double bestHealPercent = targetHealCap / (double)mostInjuredLiving.MaxHealth;
			double totalHealed = 0;
			Dictionary<GameLiving, int> healAmount = new Dictionary<GameLiving, int>();


			IDictionaryEnumerator iter = injuredTargets.GetEnumerator();
			//calculate heal for all targets
			while (iter.MoveNext())
			{
				GameLiving healTarget = iter.Key as GameLiving;
				double targetHealthPercent = (double) iter.Value;
				//targets hp percent after heal is same as mostInjuredLiving
				double targetHealPercent = bestHealPercent + mostInjuredPercent - targetHealthPercent;
				int targetHeal = (int) (healTarget.MaxHealth * targetHealPercent);

				if (targetHeal > 0)
				{
					totalHealed += targetHeal;
					healAmount.Add(healTarget, targetHeal);
				}
			}

			iter = healAmount.GetEnumerator();
			//reduce healed hp according to targetHealCap and heal targets
			while (iter.MoveNext())
			{
				GameLiving healTarget = iter.Key as GameLiving;
				double uncappedHeal = (int) iter.Value;
				int reducedHeal = (int) Math.Min(targetHealCap, uncappedHeal * (groupHealCap / totalHealed));

				//heal target
				base.HealTarget(healTarget, reducedHeal);
			}

			return true;
		}
	}
}
