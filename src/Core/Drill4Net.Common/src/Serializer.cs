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

        //http://mbraceproject.github.io/FsPickler/
        // there is written: "a library designed for cross-platform communication."
        //TODO: need to check transfer data between Win & Linux (Docker) !!!!

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
