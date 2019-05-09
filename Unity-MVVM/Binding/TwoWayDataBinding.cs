﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace UnityMVVM.Binding
{
    public class TwoWayDataBinding
        : OneWayDataBinding
    {
        [HideInInspector]
        public string _dstChangedEventName = null;

        [HideInInspector]
        public List<string> DstChangedEvents = new List<string>();

        UnityEventBinder _binder = new UnityEventBinder();
        Delegate changeDelegate;
        public override void RegisterDataBinding()
        {
            base.RegisterDataBinding();

            var propInfo = _dstView.GetType().GetProperty(_dstChangedEventName);

            var type = propInfo.PropertyType.BaseType;
            var args = type.GetGenericArguments();

            var evn = propInfo.GetValue(_dstView);

            var addListenerMethod = UnityEventBinder.GetAddListener(propInfo.GetValue(_dstView));

            changeDelegate = UnityEventBinder.GetDelegate(_binder, args);

            var p = new object[] { changeDelegate };

            _binder.OnChange += _connection.DstUpdated;

            addListenerMethod.Invoke(propInfo.GetValue(_dstView), p);
        }

        public override void UnregisterDataBinding()
        {
            base.UnregisterDataBinding();

            var propInfo = _dstView.GetType().GetProperty(_dstChangedEventName);
            var removeListenerMethod = UnityEventBinder.GetRemoveListener(propInfo.GetValue(_dstView));


            var p = new object[] { changeDelegate };

            _binder.OnChange -= _connection.DstUpdated;

            removeListenerMethod.Invoke(propInfo.GetValue(_dstView), p);
        }


        protected override void OnValidate()
        {
            UpdateBindings();
        }

        protected override IEnumerable<BindablePropertyInfo> GetExtraViewModelProperties(FieldInfo[] fields)
        {
            return new BindablePropertyInfo[0];
        }

        public override void UpdateBindings()
        {
            base.UpdateBindings();
            if (_dstView != null)
            {
                var props = _dstView.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                DstChangedEvents = props.Where(p => p.PropertyType.IsSubclassOf(typeof(UnityEventBase))
                                               && !p.GetCustomAttributes(typeof(ObsoleteAttribute), true).Any())
                                        .Select(p => p.Name).ToList();
            }
        }
    }
}
