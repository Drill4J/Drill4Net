using System;
using System.Reflection;
using System.Threading.Tasks;

//automatic version tagger including Git info
//https://github.com/devlooped/GitInfo
[assembly: AssemblyInformationalVersion(
      ThisAssembly.Git.SemVer.Major + "." +
      ThisAssembly.Git.SemVer.Minor + "." +
      ThisAssembly.Git.SemVer.Patch + "-" +
      ThisAssembly.Git.Branch + "+" +
      ThisAssembly.Git.Commit)]

namespace Drill4Net.Agent.Standard.Tester
{
    class Program
    {
        //private static int _pointRange = 200;
        public static async Task Main()
        {
            try
            {
                TesterConfigurationHelper.SetTitle();
                await TesterStandartAgent.Init();
                OutputInfoHelper.PrintTreeInfo(TesterStandartAgent.TreeInfo);
                OutputInfoHelper.PrintMenu();
                TesterStandartAgent.Polling();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            OutputInfoHelper.WriteMessage("Done.", TesterConstants.COLOR_TEXT);
        }

        ///// <summary>
        ///// Sends to the Admin side the portion of the points.
        ///// </summary>
        ///// <returns></returns>
        //private static bool SendPointBlock()
        //{
        //    if (_points.Count == 0)
        //    {
        //        WriteMessage("No more points!", TesterConstants.COLOR_ERROR);
        //        return false;
        //    }
        //    //
        //    var r = new Random(DateTime.Now.Millisecond);
        //    var end = Math.Min(_pointRange, _points.Count);
        //    for (var i = 0; i < end; i++)
        //    {
        //        var ind = r.Next(0, _points.Count);
        //        var point = _points[ind];
        //        _points.RemoveAt(ind);
        //        StandardAgent.RegisterStatic($"{point.PointUid}");
        //    }
        //    if (_points.Count == 0)
        //        WriteMessage("No more points!", TesterConstants.COLOR_ERROR);
        //    else
        //        WriteMessage($"Remaining points: {_points.Count}", ConsoleColor.Blue);
        //    return true;
        //}
    }
}