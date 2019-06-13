// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel
{
    /// <summary>
    /// The mode in which <see cref="NovelScriptPlayer"/> should handle actions skipping.
    /// </summary>
    public enum PlayerSkipMode
    {
        /// <summary>
        /// Skip only the actions that has already been executed.
        /// </summary>
        ReadOnly,
        /// <summary>
        /// Skip all actions.
        /// </summary>
        Everything
    }
}
