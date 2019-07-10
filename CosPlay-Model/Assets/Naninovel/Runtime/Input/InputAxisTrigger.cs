// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Naninovel
{
    [Serializable]
    public class InputAxisTrigger : IEquatable<InputAxisTrigger>
    {
        [Tooltip("Name of the axis.")]
        public string AxisName = string.Empty;
        [Tooltip("Whether trigger should happen when axis value is positive, negative or both.")]
        public InputAxisTriggerMode TriggerMode = InputAxisTriggerMode.Both;
        [Tooltip("When axis value is below or equal to this value, the trigger won't be activated."), Range(0, .999f)]
        public float TriggerTolerance = .001f;

        private bool wasActiveLastSample;

        /// <summary>
        /// Returns true when changed state from inactive to active since the last sample.
        /// Returns false when changed state from active to inactive since the last sample.
        /// Returns null when activation state hadn't changed since the last sample.
        /// </summary>
        public bool? Sample ()
        {
            var active = IsActive();
            var result = active != wasActiveLastSample ? (bool?)active : null;
            wasActiveLastSample = active;
            return result;
        }

        public override bool Equals (object obj)
        {
            return Equals(obj as InputAxisTrigger);
        }

        public bool Equals (InputAxisTrigger other)
        {
            return other != null &&
                   AxisName == other.AxisName &&
                   TriggerMode == other.TriggerMode;
        }

        public override int GetHashCode ()
        {
            var hashCode = 1471448403;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(AxisName);
            hashCode = hashCode * -1521134295 + TriggerMode.GetHashCode();
            return hashCode;
        }

        public static bool operator == (InputAxisTrigger trigger1, InputAxisTrigger trigger2)
        {
            return EqualityComparer<InputAxisTrigger>.Default.Equals(trigger1, trigger2);
        }

        public static bool operator != (InputAxisTrigger trigger1, InputAxisTrigger trigger2)
        {
            return !(trigger1 == trigger2);
        }

        private bool IsActive ()
        {
            if (string.IsNullOrWhiteSpace(AxisName)) return false;

            var value = Input.GetAxis(AxisName);
            if (TriggerMode == InputAxisTriggerMode.Positive && value <= 0) return false;
            if (TriggerMode == InputAxisTriggerMode.Negative && value >= 0) return false;
            return Mathf.Abs(value) > TriggerTolerance;
        }
    }
}
