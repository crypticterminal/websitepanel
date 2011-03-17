﻿// Copyright (c) 2011, SMB SAAS Systems Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// - Redistributions of source code must  retain  the  above copyright notice, this
//   list of conditions and the following disclaimer.
//
// - Redistributions in binary form  must  reproduce the  above  copyright  notice,
//   this list of conditions  and  the  following  disclaimer in  the documentation
//   and/or other materials provided with the distribution.
//
// - Neither  the  name of  the  SMB SAAS Systems Inc.  nor   the   names  of  its
//   contributors may be used to endorse or  promote  products  derived  from  this
//   software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,  BUT  NOT  LIMITED TO, THE IMPLIED
// WARRANTIES  OF  MERCHANTABILITY   AND  FITNESS  FOR  A  PARTICULAR  PURPOSE  ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
// ANY DIRECT, INDIRECT, INCIDENTAL,  SPECIAL,  EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO,  PROCUREMENT  OF  SUBSTITUTE  GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)  HOWEVER  CAUSED AND ON
// ANY  THEORY  OF  LIABILITY,  WHETHER  IN  CONTRACT,  STRICT  LIABILITY,  OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE)  ARISING  IN  ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace WebsitePanel.Setup.Actions
{
	public class SetWebPortalWebSettingsAction : Action, IPrepareDefaultsAction
	{
		public const string LogStartMessage = "Retrieving default IP address of the component...";

		void IPrepareDefaultsAction.Run(SetupVariables vars)
		{
			//
			if (String.IsNullOrEmpty(vars.WebSitePort))
				vars.WebSitePort = Global.WebPortal.DefaultPort;
			//
			if (String.IsNullOrEmpty(vars.UserAccount))
				vars.UserAccount = Global.WebPortal.ServiceAccount;

			// By default we use public ip for the component
			if (String.IsNullOrEmpty(vars.WebSiteIP))
			{
				var serverIPs = WebUtils.GetIPv4Addresses();
				//
				if (serverIPs != null && serverIPs.Length > 0)
				{
					vars.WebSiteIP = serverIPs[0];
				}
				else
				{
					vars.WebSiteIP = Global.LoopbackIPv4;
				}
			}
		}
	}

	public class UpdateEnterpriseServerUrlAction : Action, IInstallAction
	{
		public const string LogStartInstallMessage = "Updating site settings...";

		void IInstallAction.Run(SetupVariables vars)
		{
			try
			{
				Begin(LogStartInstallMessage);
				//
				Log.WriteStart(LogStartInstallMessage);
				//
				var path = Path.Combine(vars.InstallationFolder, @"App_Data\SiteSettings.config");
				//
				if (!File.Exists(path))
				{
					Log.WriteInfo(String.Format("File {0} not found", path));
					//
					return;
				}
				//
				var doc = new XmlDocument();
				doc.Load(path);
				//
				var urlNode = doc.SelectSingleNode("SiteSettings/EnterpriseServer") as XmlElement;
				if (urlNode == null)
				{
					Log.WriteInfo("EnterpriseServer setting not found");
					return;
				}

				urlNode.InnerText = vars.EnterpriseServerURL;
				doc.Save(path);
				//
				Log.WriteEnd("Updated site settings");
				//
				InstallLog.AppendLine("- Updated site settings");
			}
			catch (Exception ex)
			{
				if (Utils.IsThreadAbortException(ex))
					return;
				//
				Log.WriteError("Site settigs error", ex);
				//
				throw;
			}
		}
	}

	public class CreateDesktopShortcutsAction : Action, IInstallAction
	{
		public const string LogStartInstallMessage = "Creating shortcut...";
		public const string ApplicationUrlNotFoundMessage = "Application url not found";
		public const string Path2 = "WebsitePanel Software";

		void IInstallAction.Run(SetupVariables vars)
		{
			//
			try
			{
				Begin(LogStartInstallMessage);
				//
				Log.WriteStart(LogStartInstallMessage);
				//
				var urls = Utils.GetApplicationUrls(vars.WebSiteIP, vars.WebSiteDomain, vars.WebSitePort, null);
				string url = null;

				if (urls.Length == 0)
				{
					Log.WriteInfo(ApplicationUrlNotFoundMessage);
					//
					return;
				}
				// Retrieve top-most url from the list
				url = "http://" + urls[0];
				//
				Log.WriteStart("Creating menu shortcut");
				//
				string programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
				string fileName = "Login to WebsitePanel.url";
				string path = Path.Combine(programs, Path2);
				//
				if (!Directory.Exists(path))
				{
					Directory.CreateDirectory(path);
				}
				//
				WriteShortcutData(Path.Combine(path, fileName), url);
				//
				Log.WriteEnd("Created menu shortcut");
				//
				Log.WriteStart("Creating desktop shortcut");
				//
				string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
				WriteShortcutData(Path.Combine(desktop, fileName), url);
				//
				Log.WriteEnd("Created desktop shortcut");
				//
				InstallLog.AppendLine("- Created application shortcuts");
			}
			catch (Exception ex)
			{
				if (Utils.IsThreadAbortException(ex))
					return;
				//
				Log.WriteError("Create shortcut error", ex);
			}
		}

		private static void WriteShortcutData(string filePath, string url)
		{
			string iconFile = Path.Combine(Environment.SystemDirectory, "url.dll");
			//
			using (StreamWriter sw = File.CreateText(filePath))
			{
				sw.WriteLine("[InternetShortcut]");
				sw.WriteLine("URL=" + url);
				sw.WriteLine("IconFile=" + iconFile);
				sw.WriteLine("IconIndex=0");
				sw.WriteLine("HotKey=0");
				//
				Log.WriteInfo(String.Format("Shortcut url: {0}", url));
			}
		}
	}

	public class CopyWebConfigAction : Action, IInstallAction
	{
		void IInstallAction.Run(SetupVariables vars)
		{
			try
			{
				Log.WriteStart("Copying web.config");
				string configPath = Path.Combine(vars.InstallationFolder, "web.config");
				string config6Path = Path.Combine(vars.InstallationFolder, "web6.config");

				bool iis6 = (vars.IISVersion.Major == 6);
				if (!File.Exists(config6Path))
				{
					Log.WriteInfo(string.Format("File {0} not found", config6Path));
					return;
				}

				if (iis6)
				{
					if (!File.Exists(configPath))
					{
						Log.WriteInfo(string.Format("File {0} not found", configPath));
						return;
					}

					FileUtils.DeleteFile(configPath);
					File.Move(config6Path, configPath);
				}
				else
				{
					FileUtils.DeleteFile(config6Path);
				}
				Log.WriteEnd("Copied web.config");
			}
			catch (Exception ex)
			{
				if (Utils.IsThreadAbortException(ex))
					return;
				//
				Log.WriteError("Copy web.config error", ex);
				//
				throw;
			}
		}
	}

	public class WebPortalActionManager : BaseActionManager
	{
		public static readonly List<Action> InstallScenario = new List<Action>
		{
			new SetCommonDistributiveParamsAction(),
			new SetWebPortalWebSettingsAction(),
			new EnsureServiceAccntSecured(),
			new CopyFilesAction(),
			new CopyWebConfigAction(),
			new CreateWindowsAccountAction(),
			new ConfigureAspNetTempFolderPermissionsAction(),
			new SetNtfsPermissionsAction(),
			new CreateWebApplicationPoolAction(),
			new CreateWebSiteAction(),
			new SwitchAppPoolAspNetVersion(),
			new UpdateEnterpriseServerUrlAction(),
			new SaveComponentConfigSettingsAction(),
			new CreateDesktopShortcutsAction()
		};

		public WebPortalActionManager(SetupVariables sessionVars)
			: base(sessionVars)
		{
			Initialize += new EventHandler(WebPortalActionManager_Initialize);
		}

		void WebPortalActionManager_Initialize(object sender, EventArgs e)
		{
			//
			switch (SessionVariables.SetupAction)
			{
				case SetupActions.Install: // Install
					LoadInstallationScenario();
					break;
			}
		}

		private void LoadInstallationScenario()
		{
			CurrentScenario.AddRange(InstallScenario);
		}
	}
}
