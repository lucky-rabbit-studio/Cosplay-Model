// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;
using UnityCommon;
using UnityEngine;

namespace Naninovel
{
    public static class NovelActorExtensions
    {
        public static async Task ChangePositionXAsync (this INovelActor actor, float posX, float duration, EasingType easingType = default) => await actor.ChangePositionAsync(new Vector3(posX, actor.Position.y, actor.Position.z), duration, easingType);
        public static async Task ChangePositionYAsync (this INovelActor actor, float posY, float duration, EasingType easingType = default) => await actor.ChangePositionAsync(new Vector3(actor.Position.x, posY, actor.Position.z), duration, easingType);
        public static async Task ChangePositionZAsync (this INovelActor actor, float posZ, float duration, EasingType easingType = default) => await actor.ChangePositionAsync(new Vector3(actor.Position.x, actor.Position.y, posZ), duration, easingType);

        public static async Task ChangeRotationXAsync (this INovelActor actor, float rotX, float duration, EasingType easingType = default) => await actor.ChangeRotationAsync(Quaternion.Euler(rotX, actor.Rotation.eulerAngles.y, actor.Rotation.eulerAngles.z), duration, easingType);
        public static async Task ChangeRotationYAsync (this INovelActor actor, float rotY, float duration, EasingType easingType = default) => await actor.ChangeRotationAsync(Quaternion.Euler(actor.Rotation.eulerAngles.x, rotY, actor.Rotation.eulerAngles.z), duration, easingType);
        public static async Task ChangeRotationZAsync (this INovelActor actor, float rotZ, float duration, EasingType easingType = default) => await actor.ChangeRotationAsync(Quaternion.Euler(actor.Rotation.eulerAngles.x, actor.Rotation.eulerAngles.y, rotZ), duration, easingType);

        public static async Task ChangeScaleXAsync (this INovelActor actor, float scaleX, float duration, EasingType easingType = default) => await actor.ChangeScaleAsync(new Vector3(scaleX, actor.Scale.y, actor.Scale.z), duration, easingType);
        public static async Task ChangeScaleYAsync (this INovelActor actor, float scaleY, float duration, EasingType easingType = default) => await actor.ChangeScaleAsync(new Vector3(actor.Scale.x, scaleY, actor.Scale.z), duration, easingType);
        public static async Task ChangeScaleZAsync (this INovelActor actor, float scaleZ, float duration, EasingType easingType = default) => await actor.ChangeScaleAsync(new Vector3(actor.Scale.x, actor.Scale.y, scaleZ), duration, easingType);

        public static void ChangePositionX (this INovelActor actor, float posX) => actor.Position = new Vector3(posX, actor.Position.y, actor.Position.z);
        public static void ChangePositionY (this INovelActor actor, float posY) => actor.Position = new Vector3(actor.Position.x, posY, actor.Position.z);
        public static void ChangePositionZ (this INovelActor actor, float posZ) => actor.Position = new Vector3(actor.Position.x, actor.Position.y, posZ);

        public static void ChangeRotationX (this INovelActor actor, float rotX) => actor.Rotation = Quaternion.Euler(rotX, actor.Rotation.eulerAngles.y, actor.Rotation.eulerAngles.z);
        public static void ChangeRotationY (this INovelActor actor, float rotY) => actor.Rotation = Quaternion.Euler(actor.Rotation.eulerAngles.x, rotY, actor.Rotation.eulerAngles.z);
        public static void ChangeRotationZ (this INovelActor actor, float rotZ) => actor.Rotation = Quaternion.Euler(actor.Rotation.eulerAngles.x, actor.Rotation.eulerAngles.y, rotZ);

        public static void ChangeScaleX (this INovelActor actor, float scaleX) => actor.Scale = new Vector3(scaleX, actor.Scale.y, actor.Scale.z);
        public static void ChangeScaleY (this INovelActor actor, float scaleY) => actor.Scale = new Vector3(actor.Scale.x, scaleY, actor.Scale.z);
        public static void ChangeScaleZ (this INovelActor actor, float scaleZ) => actor.Scale = new Vector3(actor.Scale.x, actor.Scale.y, scaleZ);
    } 
}
