/*
 * Punish The Lazy, a really simple KSP reputation nibbler.
 */
using System;
using System.Timers;

using UnityEngine;


namespace PunishTheLazy
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class Punisher : MonoBehaviour
	{
		private const int DEBUG_LEVEL = 0; //Increase this number to progressively reduce log spam.

		private const double UT_MINUTE = 60;
		private const double UT_HOUR = UT_MINUTE * 60;
		private const double UT_DAY = UT_HOUR * 6;
		private const double UT_MUNARMONTH = UT_HOUR * 38.6d;
		private const double UT_YEAR = UT_HOUR * 2665.5d;

		private static Timer pTimer;
		private static float pTimerInterval;

		private static double lazyPeriod; //Let the user stay in time warp for this long.
		private static double punishPeriod; //Punish the user once every this many seconds.
		private static float punishAmount; //Punish the user by this amount over the period.

		private static double utDelta; //universe time passed since last reset.
		private static double lastUT; //Universe time at last timer tick.
		private static double ut;
		private static bool inGame;
		private static bool punishing; //true = we are punishing the player.

		public static Punisher instance{get; private set;}

		public void Start()
		{
			if (instance == null)
			{
				instance = this;
				DontDestroyOnLoad(this);
			}else{
				Destroy(this);
				return;
			}

			lazyPeriod = UT_MUNARMONTH;
			punishPeriod = UT_DAY;
			punishAmount = -0.1f;
			punishing = false;
			try
			{
				lastUT = Planetarium.GetUniversalTime();
				inGame = true;
			}
			catch(Exception ex){
				Debug.Log("[PunishTheLazy] Exception getting universal time: " + ex.Message);
				inGame = false;
			}
			utDelta = 0;
			pTimerInterval = 4000;
			pTimer = new System.Timers.Timer(pTimerInterval);
			pTimer.Elapsed += punishTimer;
			pTimer.Enabled = true;


			GameEvents.onStageActivate.Add(onStageActivate);
			GameEvents.onVesselRecovered.Add(onVesselRecovered);
			GameEvents.onCrewOnEva.Add(onCrewOnEVA);
			GameEvents.onCrewBoardVessel.Add(onCrewBoardVessel);
			GameEvents.OnProgressComplete.Add(onProgressComplete);
			GameEvents.OnProgressAchieved.Add(onProgressAchieved);
			GameEvents.OnTechnologyResearched.Add(onTechnologyResearched);
		}

		private static void punishTimer(System.Object source, ElapsedEventArgs e)
		{
			/*
			 * TODO: Check for career mode.
			 */

			/*
			 * Planetarium.GetUniversalTime() doesn't exist when the plugin starts.
			 * If there's an error grabbing the universe time, skip this tick.            
			 */
			try
			{
				ut = Planetarium.GetUniversalTime();
				utDelta += ut - lastUT;
				inGame = true;
			}
			catch(Exception ex)
			{
				Debug.Log("[PunishTheLazy] Exception getting universal time: " + ex.Message);
				punishing = false;
				inGame = false;
			}

			if (inGame)
			{
				try
				{
					if (ut == 0)
					{
						if (DEBUG_LEVEL < 1)
							Debug.Log("[PunishTheLazy] Got universal time of 0.");
						lastUT = 0;
						utDelta = 0;
					}
					if (punishing)
					{
						if (utDelta >= punishPeriod)
						{
							punish(calcPunishment(utDelta), "Player continues to be lazy.");
							utDelta = 0;
						}
					}
					else{
						if (utDelta > lazyPeriod)
						{
							punishing = true;
							punish(punishAmount, "Player is being lazy.");
							utDelta = 0;
						}
					}
					lastUT = ut;
				}
				catch(Exception ex){
					Debug.Log("[PunishTheLazy] Exception during Punish timer: " + ex.Message);
					Debug.Log("[PunishTheLazy] (Is the game in career mode?)");
					inGame = false;
				}
			}
		}

		private static float calcPunishment(double delta)
		{
			return (float)(punishAmount * (delta / punishPeriod));
		}

		private static void punish(float amt, string reason)
		{
			if (DEBUG_LEVEL < 1)
				Debug.Log("[PunishTheLazy] Punishing player by " + amt + " for reason: " + reason);
			try
			{
				//Reputation.Instance.reputation += amt;
				//We used to be able to give custom reasons. What happened? :(
				Reputation.Instance.AddReputation(amt, TransactionReasons.Cheating);
				showPunishMessage();
				if (DEBUG_LEVEL < 1)
					Debug.Log("[PunishTheLazy] New reputation: " + Reputation.Instance.reputation);
			}
			catch(Exception e){
				Debug.Log("[PunishTheLazy] Exception during punishment: " + e.Message);
			}
		}

		/*
		 * Stop punishing and reset utDelta.
		 */
		private static void stopPunishing()
		{
			if (DEBUG_LEVEL < 1)
				Debug.Log("[PunishTheLazy] Ceasing punishment.");
			punishing = false;
			utDelta = 0;
		}


		/*
		 * 
		 */
		private static void showPunishMessage()
		{
			ScreenMessages.PostScreenMessage("Your reputation has decreased to " + Reputation.Instance.reputation, pTimerInterval / 1000, ScreenMessageStyle.UPPER_CENTER);
		}

		/*!
		 * If the player triggers particular game events, reset the
		 * utDelta variable and stop punishing.
		 */
		private void onStageActivate(Int32 s)
		{
			stopPunishing();
		}
		private void onVesselRecovered(ProtoVessel pv, bool b)
		{
			stopPunishing();
		}
		private void onProgressComplete(ProgressNode p)
		{
			stopPunishing();
		}
		private void onProgressAchieved(ProgressNode p)
		{
			stopPunishing();
		}
		private void onTechnologyResearched(GameEvents.HostTargetAction<RDTech, RDTech.OperationResult> ta) 
		{
			stopPunishing();
		}
		private void onCrewOnEVA(GameEvents.FromToAction<Part, Part> act)
		{
			stopPunishing();
		}
		private void onCrewBoardVessel(GameEvents.FromToAction<Part, Part> act)
		{
			stopPunishing();
		}

		/*!
		 * Unsure if this is too abusable. We'll see. 
		 */
		private void onGameStateLoad(ConfigNode cn)
		{
			stopPunishing();
		}


	}
}


