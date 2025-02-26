// VRSYS plugin of Virtual Reality and Visualization Group (Bauhaus-University Weimar)
//  _    ______  _______  _______
// | |  / / __ \/ ___/\ \/ / ___/
// | | / / /_/ /\__ \  \  /\__ \ 
// | |/ / _, _/___/ /  / /___/ / 
// |___/_/ |_|/____/  /_//____/  
//
//  __                            __                       __   __   __    ___ .  . ___
// |__)  /\  |  | |__|  /\  |  | /__`    |  | |\ | | \  / |__  |__) /__` |  |   /\   |  
// |__) /~~\ \__/ |  | /~~\ \__/ .__/    \__/ | \| |  \/  |___ |  \ .__/ |  |  /~~\  |  
//
//       ___               __                                                           
// |  | |__  |  |\/|  /\  |__)                                                          
// |/\| |___ |  |  | /~~\ |  \                                                                                                                                                                                     
//
// Copyright (c) 2023 Virtual Reality and Visualization Group
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//-----------------------------------------------------------------
//   Authors:        Sebastian Muehlhaus
//   Date:           2023
//-----------------------------------------------------------------

using System;
using UnityEngine;
using VRSYS.Core.Utility;

namespace VRSYS.Core.Navigation
{
    public class ProgressVisualization : MonoBehaviour
    {
        public float progress = 0f;

        public bool scaleX = true;

        public bool scaleY = false;

        public bool scaleZ = true;

        private float lastProgress;

        private Vector3 originalScale;

        private Vector3 currentScale
        {
            get
            {
                return new Vector3(
                    scaleX ? originalScale.x * progress : originalScale.x,
                    scaleY ? originalScale.y * progress : originalScale.y,
                    scaleZ ? originalScale.z * progress : originalScale.z
                );
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            originalScale = transform.localScale;
            lastProgress = progress;
            ApplyCurrentProgress();
        }

        // Update is called once per frame
        void Update()
        {
            if (!lastProgress.Equals(progress, epsilon: 0.001f))
                ApplyCurrentProgress();
        }

        public void SetProgress(float value)
        {
            progress = value;
        }

        private void ApplyCurrentProgress()
        {
            transform.localScale = currentScale;
            lastProgress = progress;
        }
    }
}
