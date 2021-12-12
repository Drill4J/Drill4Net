using System;
using System.Threading.Tasks;

namespace AsyncTest
{
    /// <summary>
    ///Async samples.
    /// </summary>
    public class AsyncSample
    {
        public async Task Async_Task(bool cond)
        {
            if (cond)
            {
                await Task.Delay(50);
            }
            else
                await Delay100();
            Console.WriteLine(string.Format("{0}: {1}", "Async_Task", cond));
        }

        public async Task<int> Async_Task2(int cond)
        {
            try
            {
                var t = 1444;
                switch (cond)
                {
                    default:
                        var i = await Delay10();
                        try
                        {
                            await Task.Delay(i);
                        }
                        catch
                        {
                           t = 100;
                        }
                        break;


                    case 1:
                        t = 1;
                        break;
                    case 2:
                        t = 2;
                        break;
                    case 3:
                        goto default;
                }
                Console.WriteLine(string.Format("{0}: {1}", "Async_Task", cond));
            }
            catch
            {
            }
            return 0;
        }

        private Task Delay100()
        {
            return Task.Delay(100);
        }

        private async Task<int> Delay10()
        {
            await Task.Delay(10);
            return 10;
        }
    }
}
