// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel
{
    /// <summary>
    /// Represents a position in a <see cref="Script"/> played by a <see cref="ScriptPlayer"/>.
    /// </summary>
    [System.Serializable]
    public class PlaybackSpot
    {
        public string ScriptName;
        public int LineIndex, InlineIndex;
    }
}
