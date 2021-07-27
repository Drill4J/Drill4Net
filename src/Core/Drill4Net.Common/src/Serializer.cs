using MBrace.FsPickler;

namespace Drill4Net.Common
{
    public static class Serializer
    {
        public static byte[] StringToArray(string str)
        {
            return ToArray<string>(str);
        }

        public static string ArrayToString(byte[] ar)
        {
            return FromArray<string>(ar);
        }

        public static byte[] ToArray<T>(T obj)
        {
            var binarySerializer = FsPickler.CreateBinarySerializer();
            var data = binarySerializer.Pickle<T>(obj);
            return data;
        }

        public static T FromArray<T>(byte[] data)
        {
            var binarySerializer = FsPickler.CreateBinarySerializer();
            var obj = binarySerializer.UnPickle<T>(data);
            return obj;
        }
    }
}
