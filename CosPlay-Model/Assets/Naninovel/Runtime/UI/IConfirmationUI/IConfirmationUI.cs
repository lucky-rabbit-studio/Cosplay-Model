// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System.Threading.Tasks;

namespace Naninovel.UI
{
    /// <summary>
    /// Represents a set of UI elements used to block user input until it confirms or cancels.
    /// </summary>
    public interface IConfirmationUI : IManagedUI
    {
        /// <summary>
        /// Blocks user input and shows the confirmation dialogue with the provided message.
        /// The async should return when user confirms or cancels.
        /// </summary>
        /// <param name="message">The confirmation message.</param>
        /// <returns>Whether the user confirmed.</returns>
        Task<bool> ConfirmAsync (string message);
    }
}
