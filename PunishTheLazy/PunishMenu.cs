using System;
using UnityEngine;
using System.Collections.Generic;

namespace PunishTheLazy
{
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class PunishMenu : MonoBehaviour
	{
		/*
		Texture2D appButton;
		private List<string> historyList;
		private int historyMaxLength = 30;

		private int windowX;
		private int windowY;
		private string windowName;
		*/

		public static PunishMenu instance{get; private set;}

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
		}

		private void drawMenu()
		{
		}

		private void drawRepCount()
		{
		}

		private void drawEnableCheckbox()
		{
		}

		private void drawSettingsButton()
		{
		}

		private void drawHistoryBox()
		{
		}

		private void checkAppButton()
		{
		}
	}
}
