﻿using System;
using Drill4Net.Common;

namespace Drill4Net.Agent.Standard.Tester
{
    /// <summary>
    /// Functions for setting Tester app up
    /// </summary>
    internal class TesterInformer
    {
        private readonly TesterOutputHelper _helper;

        /*******************************************************************/

        public TesterInformer(TesterOutputHelper helper)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        /*******************************************************************/

        internal void SetTitle()
        {
            var version = CommonUtils.GetAppVersion();
            var appName = CommonUtils.GetAppName();
            var title = $"{appName} {version}";
            Console.Title = title;
            _helper.WriteMessage(title, ConsoleColor.Cyan);
        }
    }
}
