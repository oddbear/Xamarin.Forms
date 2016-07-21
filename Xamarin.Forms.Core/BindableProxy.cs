using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Xamarin.Forms
{
	internal class BindableProxy : BindableObject
	{
		readonly object targetObject;
		readonly string targetProperty;
		readonly string targetEvent;
		readonly PropertyInfo propInfo;
		readonly MethodInfo[] setMethodsInfo;
		readonly MethodInfo[] getMethodsInfo;
		readonly Type[] parameterPossibleTypes;

		Action<object, object> callbackSetValue;
		Func<object> callbackGetValue;


		public BindableProperty Property;

		public Type TargetPropertyType;

		public string TargetPropertyName => targetProperty;
		public string TargetEventName => targetEvent;

		public List<Type> ParameterTypes => parameterPossibleTypes?.ToList();

		public BindableProxy(object target, PropertyInfo targetPropInfo, string eventName = null)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (targetPropInfo == null)
				throw new ArgumentException("targetProperty should not be null or empty", nameof(targetPropInfo));
			targetProperty = targetPropInfo.Name;
			targetObject = target;
			targetEvent = eventName;

			propInfo = targetPropInfo;

			Init();
		}

		public BindableProxy(object target, string targetProp, Action<object, object> setValue = null, Func<object> getValue = null)
		{
			if (target == null)
				throw new ArgumentNullException(nameof(target));
			if (string.IsNullOrEmpty(targetProp))
				throw new ArgumentException("targetProperty should not be null or empty", nameof(targetProp));
			targetProperty = targetProp;
			targetObject = target;

			callbackSetValue = setValue;
			callbackGetValue = getValue;

			var targetObjectType = targetObject.GetType();

			propInfo = targetObjectType.GetProperty(targetProperty);

			if (propInfo == null)
			{
				var setMethodName = $"Set{targetProp}";
				var getMethodName = $"Get{targetProp}";
				var gets = new List<MethodInfo>();
				var sets = new List<MethodInfo>();
				var parameterTypes = new List<Type>();
				foreach (var method in targetObjectType.GetRuntimeMethods())
				{
					System.Diagnostics.Debug.WriteLine(method);
					if (method.DeclaringType != targetObjectType)
						continue;

					if (method.Name == setMethodName)
					{
						sets.Add(method);
						foreach (var parameter in method.GetParameters())
						{
							parameterTypes.Add(parameter.ParameterType);
						}
					}

					if (method.Name == getMethodName)
					{
						gets.Add(method);
						parameterTypes.Add(method.ReturnType);
					}

				}

				getMethodsInfo = gets.ToArray();
				setMethodsInfo = sets.ToArray();
				parameterPossibleTypes = parameterTypes.ToArray();
			}


			Init();
		}

		void OnPropertyChanged(object oldValue, object newValue)
		{
			if (callbackSetValue != null)
				callbackSetValue(oldValue, newValue);
			else
				SetTargetValue(newValue);
		}

		void Init()
		{
			Property = BindableProperty.Create(targetProperty, typeof(object), typeof(BindableProxy), propertyChanged: (bo, o, n) => ((BindableProxy)bo).OnPropertyChanged(o, n));

			if (propInfo != null)
				TargetPropertyType = propInfo.PropertyType;
		}

		internal void OnTargetPropertyChanged(object valueFromNative = null, IValueConverter converter = null)
		{
			//this comes converted
			var currentValue = GetValue(Property);

			var nativeValue = GetTargetValue();

			if (valueFromNative == null)
				valueFromNative = nativeValue;


			if (valueFromNative.Equals(currentValue))
				return;

			SetValueCore(Property, valueFromNative);
		}

		void SetTargetValue(object value)
		{
			if (value == null)
				return;

			bool wasSet = TrySetValueOnTarget(value);

			if (!wasSet)
				throw new InvalidCastException($"Can't bind properties of different types target property {TargetPropertyType}, and the value is {value.GetType()}");

		}

		bool TrySetValueOnTarget(object value)
		{
			bool wasSet = false;

			if (propInfo != null)
				wasSet = SetPropertyInfo(value);

			if (setMethodsInfo != null && !wasSet)
				wasSet = SetSetMethodInfo(value);

			return wasSet;
		}

		object GetTargetValue()
		{
			if (callbackGetValue != null)
				return callbackGetValue();

			if (propInfo != null)
				return ReadPropertyInfo();

			if (getMethodsInfo != null)
				return ReadGetMethodInfo();

			return null;

		}

		bool SetSetMethodInfo(object value)
		{
			bool wasSet = false;

			foreach (var setMethod in setMethodsInfo)
			{
				try
				{
					setMethod.Invoke(targetObject, new object[] { value });
					wasSet = true;
					break;
				}
				catch (ArgumentException)
				{
					System.Diagnostics.Debug.WriteLine("Failed to convert");
				}
			}

			return wasSet;
		}

		object ReadGetMethodInfo()
		{
			foreach (var getMethod in getMethodsInfo)
			{
				try
				{
					var possibleValue = getMethod.Invoke(targetObject, new object[] { });
					if (possibleValue != null)
						return possibleValue;
					break;
				}
				catch (Exception ex)
				{
					throw (ex);
				}
			}
			return null;
		}

		object ReadPropertyInfo()
		{
			if (!propInfo.CanRead)
			{
				System.Diagnostics.Debug.WriteLine($"No GetMethod found for {TargetPropertyName}");
				return null;
			}

			var obj = propInfo.GetValue(targetObject);
			return obj;
		}

		bool SetPropertyInfo(object value)
		{
			if (TargetPropertyType != value.GetType())
				return false;

			if (!propInfo.CanWrite)
			{
				System.Diagnostics.Debug.WriteLine($"No SetMethod found for {TargetPropertyName}");
				return false;
			}

			propInfo.SetValue(targetObject, value);
			return true;
		}
	}
}

