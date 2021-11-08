using System;
using System.Threading.Tasks;

namespace AsyncTest
{
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

        //public async Task Async_Task3(int cond)
        //{
        //    try
        //    {
        //        switch (cond)
        //        {
        //            default:
        //                var i = await Delay10();
        //                await Task.Delay(i);
        //                break;


        //            case 1:
        //                await Delay100();
        //                break;
        //            case 2:
        //                await Task.Delay(15);
        //                break;

        //            case 3:
        //                goto default;
        //        }
        //        Console.WriteLine(string.Format("{0}: {1}", "Async_Task", cond));

        //    }
        //    catch
        //    {

        //    }
        //}

        //public async Task Async_Task2(bool cond)
        //{
        //    try
        //    {
        //        int y = 10;
        //        if (cond)
        //        {

        //            var i = await Delay10();
        //            try
        //            {
        //                await Task.Delay(i);
        //            }
        //            catch
        //            {

        //            }
        //        }
        //        else
        //            await Delay100();
        //        Console.WriteLine(string.Format("{0}: {1}", "Async_Task", cond));
        //    }
        //    catch
        //    {
        //        Console.WriteLine("Error");
        //    }
        //}

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
