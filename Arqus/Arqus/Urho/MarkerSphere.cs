﻿using System;
using System.Globalization;
using Urho;
using Urho.Actions;
using Urho.Gui;
using Urho.Shapes;
using QTMRealTimeSDK.Data;

namespace Arqus.Visualization
{
    /// <summary>
    /// Marker node with an attached sphere component
    /// </summary>
    public class MarkerSphere : Node
    {
        public Color SphereColor { get; set; }

        public MarkerSphere()
        {
            // Set default color to white
            SphereColor = Color.White;

            // Initialize disabled
            Enabled = false;

            // HACK: Arbitrary number of 0.1 makes sense right now
            SetScale(0.01f);

            // Create sphere component and attach it 
            Sphere sphereComponent = CreateComponent<Sphere>();
            sphereComponent.SetMaterial(Material.FromColor(SphereColor, true));
        }
    }
}
