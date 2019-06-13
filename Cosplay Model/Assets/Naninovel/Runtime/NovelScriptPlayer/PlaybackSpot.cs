// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.


namespace Naninovel
{
    /// <summary>
    /// Represents a position in a <see cref="NovelScript"/> played by a <see cref="NovelScriptPlayer"/>.
    /// </summary>
    [System.Serializable]
    public class PlaybackSpot
    {
        public string ScriptName;
        public int LineIndex, InlineIndex;
    }
}
