// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Collections;
using UnityEngine;

namespace Naninovel
{
    public interface IEngineBehaviour
    {
        event Action OnBehaviourUpdate;
        event Action OnBehaviourLateUpdate;
        event Action OnBehaviourDestroy;

        GameObject GetRootObject ();
        void AddChildObject (GameObject obj);
        void Destroy ();
        Coroutine StartCoroutine (IEnumerator routine);
        void StopCoroutine (IEnumerator routine);
    }
}
