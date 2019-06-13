// Copyright 2017-2019 Elringus (Artyom Sovetnikov). All Rights Reserved.

using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Naninovel.UI
{
    /// <summary>
    /// Generic interface for all the UI objects managable by <see cref="UIManager"/> service.
    /// </summary>
    public interface IManagedUI
    {
        /// <summary>
        /// Event invoked when <see cref="IsVisible"/> of the UI object is changed.
        /// </summary>
        event Action<bool> OnVisibilityChanged;

        /// <summary>
        /// Whether the UI element is currently visible to the user.
        /// </summary>
        /// <remarks>
        /// The visibility should change instantly when set.
        /// </remarks>
        bool IsVisible { get; set; }
        /// <summary>
        /// The order in which the element is sorted among other elements.
        /// </summary>
        int SortingOrder { get; set; }
        /// <summary>
        /// Rendering mode of the UI element.
        /// </summary>
        RenderMode RenderMode { get; set; }
        /// <summary>
        /// Camera the UI lement uses for reference when scaling and handling user input.
        /// </summary>
        Camera RenderCamera { get; set; }

        /// <summary>
        /// Allows to execute any async initialization logic.
        /// Invoked once by <see cref="UIManager"/> on service initialization.
        /// </summary>
        Task InitializeAsync ();
        /// <summary>
        /// Shows the UI element to the user.
        /// </summary>
        /// <remarks>
        /// <see cref="IsVisible"/> should be set to true at the time this method is invoked.
        /// The actual visibility of the UI element could change gradually, including any animations.
        /// </remarks>
        void Show ();
        /// <summary>
        /// Hides the UI element from the user.
        /// </summary>
        /// <remarks>
        /// <see cref="IsVisible"/> should be set to false at the time this method is invoked.
        /// The actual visibility of the UI element could change gradually, including any animations.
        /// </remarks>
        void Hide ();
        /// <summary>
        /// Changes the visibility over the specified <paramref name="fadeTime"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="IsVisible"/> should be set to <paramref name="isVisible"/> at the time this method is invoked.
        /// Should not return until visibility has been completely changed.
        /// </remarks>
        Task SetIsVisibleAsync (bool isVisible, float? fadeTime = null);
    }
}
