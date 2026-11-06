using System;
using System.Linq.Expressions;
using System.Reflection;
using IPA.Loader;
using UnityEngine;

namespace SmoothedController {
	internal static class NalulunaAvatarsCompatibility {
		private static Func<MonoBehaviour> getView;

		internal static void Initialize() {
			Reset();
			var metadata = PluginManager.GetPluginFromId("NalulunaAvatars");
			if(metadata?.Assembly == null || !PluginManager.IsEnabled(metadata))
				return;

			try {
				var type = metadata.Assembly.GetType("NalulunaAvatars.ListViewController", true);
				var instanceField = type.GetField("instance", BindingFlags.Public | BindingFlags.Static);
				var viewProperty = type.GetProperty("viewController", BindingFlags.Public | BindingFlags.Instance);
				if(instanceField?.FieldType != type || viewProperty?.GetGetMethod() == null ||
					!typeof(MonoBehaviour).IsAssignableFrom(viewProperty.PropertyType))
					throw new NotSupportedException("Missing NalulunaAvatars menu visibility API.");

				var instance = Expression.Variable(type, "instance");
				var body = Expression.Block(new[] { instance },
					Expression.Assign(instance, Expression.Field(null, instanceField)),
					Expression.Condition(Expression.ReferenceEqual(instance, Expression.Constant(null, type)),
						Expression.Constant(null, typeof(MonoBehaviour)),
						Expression.Convert(Expression.Property(instance, viewProperty), typeof(MonoBehaviour))));
				getView = Expression.Lambda<Func<MonoBehaviour>>(body).Compile();
			} catch(Exception error) {
				Reset();
				Plugin.Log.Warn("Naluluna Avatars menu compatibility is unavailable. " + error.GetBaseException().Message);
			}
		}

		internal static bool IsMenuOpen {
			get {
				var view = getView?.Invoke();
				return view != null && view.isActiveAndEnabled;
			}
		}

		internal static void Reset() {
			getView = null;
		}
	}
}
