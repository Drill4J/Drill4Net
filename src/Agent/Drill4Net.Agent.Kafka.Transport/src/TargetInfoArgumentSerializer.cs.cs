using System;
using System.Text;
using Drill4Net.Agent.Kafka.Common;
using Drill4Net.Common;

namespace Drill4Net.Agent.Kafka.Transport
{
    //public static class TargetInfoArgumentSerializer
    //{
    //    public static string Serialize(TargetInfo target)
    //    {
    //        if (target == null)
    //            throw new ArgumentNullException(nameof(target));
    //        //
    //        var ar = Serializer.ToArray(target);
    //        var compressed = Compressor.Compress(ar);
    //        var str = Encoding.UTF8.GetString(compressed);
    //        var base64 = CommonUtils.ToBase64String(str);
    //        return base64;
    //    }

    //    public static TargetInfo Deserialize(string base64)
    //    {
    //        if (string.IsNullOrWhiteSpace(base64))
    //            throw new ArgumentNullException(nameof(base64));
    //        //
    //        var str = CommonUtils.FromBase64String(base64);
    //        var compressed = Encoding.UTF8.GetBytes(str);
    //        var ar = Compressor.Decompress(compressed);
    //        var target = Serializer.FromArray<TargetInfo>(ar);
    //        return target;
    //    }
    //}
}
