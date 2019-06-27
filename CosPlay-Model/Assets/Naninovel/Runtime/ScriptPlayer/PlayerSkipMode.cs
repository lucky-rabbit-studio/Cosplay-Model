// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel
{
    /// <summary>
    /// The mode in which <see cref="ScriptPlayer"/> should handle commands skipping.
    /// </summary>
    public enum PlayerSkipMode
    {
        /// <summary>
        /// Skip only the commands that has already been executed.
        /// </summary>
        ReadOnly,
        /// <summary>
        /// Skip all commands.
        /// </summary>
        Everything
    }
}
