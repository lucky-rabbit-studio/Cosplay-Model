// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace Naninovel.UI
{
    /// <summary>
    /// Implementation is able to gradually reveal text.
    /// </summary>
    public interface IRevealableText
    {
        /// <summary>
        /// Actual text stored by the implementation.
        /// </summary>
        string Text { get; set; }
        /// <summary>
        /// Current text color.
        /// </summary>
        Color TextColor { get; set; }
        /// <summary>
        /// Object that hosts the implementation.
        /// </summary>
        GameObject GameObject { get; }
        /// <summary>
        /// Whether the <see cref="Text"/> is entirely revealed and visible to the user.
        /// </summary>
        bool IsFullyRevealed { get; }
        /// <summary>
        /// Progress (in 0.0 to 1.0 range) of the <see cref="Text"/> reveal process.
        /// </summary>
        float RevealProgress { get; set; }

        /// <summary>
        /// Implementation should attempt to reveal next visible (not-a-tag) <see cref="Text"/> character.
        /// </summary>
        /// <param name="revealDelay">Current reveal (print) delay.</param>
        /// <returns>Whether reveal was a success. Should returns false when no characters are left reveal.</returns>
        bool RevealNextChar (float revealDelay);
        /// <summary>
        /// Implementation should immediately reveal all the <see cref="Text"/> characters.
        /// </summary>
        void RevealAll ();
        /// <summary>
        /// Implementation should immediately hide all the <see cref="Text"/> characters.
        /// </summary>
        void HideAll ();
        /// <summary>
        /// Implementation should return position (in world space) of the last revealed <see cref="Text"/> character.
        /// </summary>
        Vector2 GetLastRevealedCharPosition ();
        /// <summary>
        /// Implementation should return the last revealed <see cref="Text"/> character.
        /// </summary>
        char GetLastRevealedChar ();
    }
}
