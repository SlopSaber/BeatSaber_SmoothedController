using System;
using System.Reflection;
using IPA.Loader;

namespace SmoothedController {
	internal static class EasyOffsetCompatibility {
		private static Func<bool> panelVisible;
		private static Func<bool> inMainMenu;

		internal static void Initialize() {
			Reset();
			var metadata = PluginManager.GetPluginFromId("EasyOffset");
			if(metadata?.Assembly == null)
				return;

			try {
				var config = metadata.Assembly.GetType("EasyOffset.PluginConfig", true);
				panelVisible = Bind(config, "IsModPanelVisible");
				inMainMenu = Bind(config, "IsInMainMenu");
			} catch(Exception error) {
				Reset();
				Plugin.Log.Warn("Easy Offset calibration compatibility is unavailable; update Easy Offset to expose panel visibility. " + error.GetBaseException().Message);
			}
		}

		internal static bool IsPanelOpen => panelVisible != null && inMainMenu != null && panelVisible() && inMainMenu();

		private static Func<bool> Bind(Type config, string property) {
			var getter = config.GetProperty(property, BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
			if(getter == null || getter.ReturnType != typeof(bool) || getter.GetParameters().Length != 0)
				throw new NotSupportedException("Missing EasyOffset.PluginConfig." + property);
			return (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), getter);
		}

		internal static void Reset() {
			panelVisible = null;
			inMainMenu = null;
		}
	}
}
