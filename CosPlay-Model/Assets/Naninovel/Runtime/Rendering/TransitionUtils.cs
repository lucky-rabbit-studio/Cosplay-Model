// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections.Generic;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    public static class TransitionUtils
    {
        private const string keywordPrefix = "NANINOVEL_TRANSITION_";

        private static readonly Dictionary<TransitionType, Vector4> typeToDefaultParamsMap = new Dictionary<TransitionType, Vector4> {
            [TransitionType.Crossfade] = Vector4.zero,
            [TransitionType.BandedSwirl] = new Vector4(5, 10),
            [TransitionType.Blinds] = new Vector4(6, 0),
            [TransitionType.CircleReveal] = new Vector4(.25f, 0),
            [TransitionType.CircleStretch] = Vector4.zero,
            [TransitionType.CloudReveal] = Vector4.zero,
            [TransitionType.Crumble] = Vector4.zero,
            [TransitionType.Disolve] = new Vector4(99999, 0),
            [TransitionType.DropFade] = Vector4.zero,
            [TransitionType.LineReveal] = new Vector4(.025f, .5f, .5f),
            [TransitionType.Pixelate] = Vector4.zero,
            [TransitionType.RadialBlur] = Vector4.zero,
            [TransitionType.RadialWiggle] = Vector4.zero,
            [TransitionType.RandomCircleReveal] = Vector4.zero,
            [TransitionType.Ripple] = new Vector4(20f, 10f, .05f),
            [TransitionType.RotateCrumble] = Vector4.zero,
            [TransitionType.Saturate] = Vector4.zero,
            [TransitionType.Shrink] = new Vector4(200, 0),
            [TransitionType.SlideIn] = new Vector4(1, 0),
            [TransitionType.SwirlGrid] = new Vector4(15, 10),
            [TransitionType.Swirl] = new Vector4(15, 0),
            [TransitionType.Water] = Vector4.zero,
            [TransitionType.Waterfall] = Vector4.zero,
            [TransitionType.Wave] = new Vector4(.1f, 14, 20),
            [TransitionType.Custom] = Vector4.zero
        };

        public static string GetShaderKeyword (this TransitionType type)
        {
            return string.Concat(keywordPrefix, type.ToString().ToUpperInvariant());
        }

        public static Vector4 GetDefaultParams (this TransitionType transitionType)
        {
            if (!typeToDefaultParamsMap.ContainsKey(transitionType))
            {
                Debug.LogError($"Default transition params for '{transitionType}' not defined.");
                return Vector4.zero;
            }
            return typeToDefaultParamsMap[transitionType];
        }

        public static TransitionType TypeFromString (string transitionType)
        {
            return (TransitionType)Enum.Parse(typeof(TransitionType), transitionType, true);
        }

        public static TransitionType GetEnabled (Material material)
        {
            for (int i = 0; i < material.shaderKeywords.Length; i++)
                if (material.shaderKeywords[i].StartsWith(keywordPrefix) && material.IsKeywordEnabled(material.shaderKeywords[i]))
                    return TypeFromString(material.shaderKeywords[i].GetAfter(keywordPrefix));
            return TransitionType.Crossfade; // Crossfade is executed by default when no keywords enabled.
        }

        public static void EnableKeyword (Material material, TransitionType transitionType)
        {
            for (int i = 0; i < material.shaderKeywords.Length; i++)
                if (material.shaderKeywords[i].StartsWith(keywordPrefix) && material.IsKeywordEnabled(material.shaderKeywords[i]))
                    material.DisableKeyword(material.shaderKeywords[i]);

            // Crossfade is executed when no transition keywords enabled.
            if (transitionType == TransitionType.Crossfade) return;

            material.EnableKeyword(transitionType.GetShaderKeyword());
        }
    }
}
