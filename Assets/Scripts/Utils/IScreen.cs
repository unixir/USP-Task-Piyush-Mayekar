using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScreen
{
    /// <summary>
    /// Activate screen
    /// </summary>
    void Activate(string message = null);

    /// <summary>
    /// Deactivate screen
    /// </summary>
    void Deactivate();

    /// <summary>
    /// What this screen should do when Back button is pressed
    /// </summary>
    void OnBackButtonPressed();
}
