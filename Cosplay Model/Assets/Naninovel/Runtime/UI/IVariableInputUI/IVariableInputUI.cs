// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel.UI
{
    /// <summary>
    /// Implementation is able to present an input UI to set a custom state variable
    /// when requested by <see cref="Actions.InputCustomVariable"/> action.
    /// </summary>
    public interface IVariableInputUI : IManagedUI
    {
        /// <summary>
        /// Shows the UI to input a custom variable.
        /// </summary>
        /// <param name="variableName">Name of custom variable to assign the input value to.</param>
        /// <param name="summary">Summary text to show in the UI.</param>
        /// <param name="playOnSubmit">Whether to invoke <see cref="NovelScriptPlayer.Play()"/> on submit.</param>
        void Show (string variableName, string summary, bool playOnSubmit);
    }
}
