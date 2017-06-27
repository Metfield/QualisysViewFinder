using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Arqus.CameraApplication;

namespace Arqus.Visualization
{

    class CameraScreenLayoutManager
    {
       
        public CameraScreenLayout currentLayout { get; private set; }
        private Dictionary<ScreenLayoutType, CameraScreenLayout> layouts;

        private int selection;

        public int Selection
        {
            get { return currentLayout.Selection; }
        }


        public CameraScreenLayoutManager(Dictionary<ScreenLayoutType, CameraScreenLayout> layouts)
        {
            this.layouts = layouts;
        }
        
        public void Set(ScreenLayoutType type)
        {
            currentLayout = layouts[type];
        }

        public void Select(int id)
        {
            layouts
                .Values
                .ToList()
                .ForEach(layout => layout.Select(id));
        }
        
    }
}
