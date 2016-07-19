using System.Collections.Generic;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Android
{
	public class NativeViewWrapper : View, INativeViewBindableController
	{

		public NativeViewWrapper(global::Android.Views.View nativeView, GetDesiredSizeDelegate getDesiredSizeDelegate = null, OnLayoutDelegate onLayoutDelegate = null,
								 OnMeasureDelegate onMeasureDelegate = null)
		{
			GetDesiredSizeDelegate = getDesiredSizeDelegate;
			NativeView = nativeView;
			OnLayoutDelegate = onLayoutDelegate;
			OnMeasureDelegate = onMeasureDelegate;
		}

		public GetDesiredSizeDelegate GetDesiredSizeDelegate { get; }

		public global::Android.Views.View NativeView { get; }

		public OnLayoutDelegate OnLayoutDelegate { get; }

		public OnMeasureDelegate OnMeasureDelegate { get; }


		void INativeViewBindableController.UnApplyNativeBindings()
		{
			foreach (var item in bindableProxies)
			{
				item.Value.Unapply();
				item.Key.RemoveBinding(item.Key.Property);
				item.Key.BindingContext = null;

				if (item.Value.Mode == BindingMode.TwoWay)
				{
					UnSubscribeTwoWay(item);
				}
			}

			bindableProxies = null;
		}

		void INativeViewBindableController.ApplyNativeBindings()
		{
			if (NativeBindingExtensions.NativeBindingPool.ContainsKey(NativeView))
				bindableProxies = NativeBindingExtensions.NativeBindingPool[NativeView];

			foreach (var item in bindableProxies)
			{
				item.Key.SetBinding(item.Key.Property, item.Value);
				item.Key.BindingContext = BindingContext;

				if (item.Value.Mode == BindingMode.TwoWay)
				{
					SubscribeTwoWay(item);
				}
			}
		}

		void INativeViewBindableController.OnNativePropertyChange(string property, object newValue)
		{
			foreach (var item in bindableProxies)
			{
				if (item.Key.TargetPropertyName == property.ToString())
				{
					item.Key.OnTargetPropertyChanged(newValue, item.Value.Converter);
				}
			}
		}

		void SubscribeTwoWay(KeyValuePair<BindableProxy, Binding> item)
		{
			//if (propertyListener == null)
			//	propertyListener = new NativeViewPropertyListener(this);

			//NativeView.AddObserver(propertyListener, new NSString(item.Key.TargetPropertyName), 0, IntPtr.Zero);

			if (!string.IsNullOrEmpty(item.Key.TargetEventName))
			{
				eventListener = new NativeViewEventListener(NativeView, item.Key.TargetEventName, item.Key.TargetPropertyName);
				eventListener.NativeViewEventFired += NativeViewEventFired;
			}
		}

		void UnSubscribeTwoWay(KeyValuePair<BindableProxy, Binding> item)
		{
			//if (propertyListener != null)
			//{
			//	NativeView.RemoveObserver(propertyListener, new NSString(item.Key.TargetPropertyName), IntPtr.Zero);
			//	propertyListener.Dispose();
			//}

			if (eventListener != null)
			{
				eventListener.NativeViewEventFired -= NativeViewEventFired;
				eventListener.Dispose();
			}

			//propertyListener = null;
			eventListener = null;
		}

		void NativeViewEventFired(object sender, NativeViewEventFiredEventArgs e)
		{
			(this as INativeViewBindableController).OnNativePropertyChange(e.PropertyName, null);
		}

		Dictionary<BindableProxy, Binding> bindableProxies = new Dictionary<BindableProxy, Binding>();

		NativeViewEventListener eventListener;
	}
}