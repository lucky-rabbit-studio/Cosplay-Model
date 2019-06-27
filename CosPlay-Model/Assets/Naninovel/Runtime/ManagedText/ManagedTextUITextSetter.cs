// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;
using UnityEngine.UI;

namespace Naninovel
{
    /// <summary>
    /// Allows setting <see cref="ManagedText"/> to a <see cref="UnityEngine.UI.Text"/> 
    /// by associating <see cref="GameObject"/> name and defined managed text fields.
    /// Underscore ("_") chars and casing in the defined field names are ignored.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public abstract class ManagedTextUITextSetter : ManagedTextSetter
    {
        protected Text Text { get; private set; }

        protected virtual void Awake () => Text = GetComponent<Text>();

        protected override void SetManagedTextValue (string value) => Text.text = value;
    }
}
