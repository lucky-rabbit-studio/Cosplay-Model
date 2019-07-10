// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    public static class ActorExtensions
    {
        public static async Task ChangePositionXAsync (this IActor actor, float posX, float duration, EasingType easingType = default) => await actor.ChangePositionAsync(new Vector3(posX, actor.Position.y, actor.Position.z), duration, easingType);
        public static async Task ChangePositionYAsync (this IActor actor, float posY, float duration, EasingType easingType = default) => await actor.ChangePositionAsync(new Vector3(actor.Position.x, posY, actor.Position.z), duration, easingType);
        public static async Task ChangePositionZAsync (this IActor actor, float posZ, float duration, EasingType easingType = default) => await actor.ChangePositionAsync(new Vector3(actor.Position.x, actor.Position.y, posZ), duration, easingType);

        public static async Task ChangeRotationXAsync (this IActor actor, float rotX, float duration, EasingType easingType = default) => await actor.ChangeRotationAsync(Quaternion.Euler(rotX, actor.Rotation.eulerAngles.y, actor.Rotation.eulerAngles.z), duration, easingType);
        public static async Task ChangeRotationYAsync (this IActor actor, float rotY, float duration, EasingType easingType = default) => await actor.ChangeRotationAsync(Quaternion.Euler(actor.Rotation.eulerAngles.x, rotY, actor.Rotation.eulerAngles.z), duration, easingType);
        public static async Task ChangeRotationZAsync (this IActor actor, float rotZ, float duration, EasingType easingType = default) => await actor.ChangeRotationAsync(Quaternion.Euler(actor.Rotation.eulerAngles.x, actor.Rotation.eulerAngles.y, rotZ), duration, easingType);

        public static async Task ChangeScaleXAsync (this IActor actor, float scaleX, float duration, EasingType easingType = default) => await actor.ChangeScaleAsync(new Vector3(scaleX, actor.Scale.y, actor.Scale.z), duration, easingType);
        public static async Task ChangeScaleYAsync (this IActor actor, float scaleY, float duration, EasingType easingType = default) => await actor.ChangeScaleAsync(new Vector3(actor.Scale.x, scaleY, actor.Scale.z), duration, easingType);
        public static async Task ChangeScaleZAsync (this IActor actor, float scaleZ, float duration, EasingType easingType = default) => await actor.ChangeScaleAsync(new Vector3(actor.Scale.x, actor.Scale.y, scaleZ), duration, easingType);

        public static void ChangePositionX (this IActor actor, float posX) => actor.Position = new Vector3(posX, actor.Position.y, actor.Position.z);
        public static void ChangePositionY (this IActor actor, float posY) => actor.Position = new Vector3(actor.Position.x, posY, actor.Position.z);
        public static void ChangePositionZ (this IActor actor, float posZ) => actor.Position = new Vector3(actor.Position.x, actor.Position.y, posZ);

        public static void ChangeRotationX (this IActor actor, float rotX) => actor.Rotation = Quaternion.Euler(rotX, actor.Rotation.eulerAngles.y, actor.Rotation.eulerAngles.z);
        public static void ChangeRotationY (this IActor actor, float rotY) => actor.Rotation = Quaternion.Euler(actor.Rotation.eulerAngles.x, rotY, actor.Rotation.eulerAngles.z);
        public static void ChangeRotationZ (this IActor actor, float rotZ) => actor.Rotation = Quaternion.Euler(actor.Rotation.eulerAngles.x, actor.Rotation.eulerAngles.y, rotZ);

        public static void ChangeScaleX (this IActor actor, float scaleX) => actor.Scale = new Vector3(scaleX, actor.Scale.y, actor.Scale.z);
        public static void ChangeScaleY (this IActor actor, float scaleY) => actor.Scale = new Vector3(actor.Scale.x, scaleY, actor.Scale.z);
        public static void ChangeScaleZ (this IActor actor, float scaleZ) => actor.Scale = new Vector3(actor.Scale.x, actor.Scale.y, scaleZ);
    } 
}
