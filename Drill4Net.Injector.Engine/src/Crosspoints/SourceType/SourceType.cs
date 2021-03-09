namespace Drill4Net.Injector.Engine
{
    public struct SourceType
    {
        public bool IsStatic { get; set; }
        public bool IsAbstractType { get; set; }
        public bool IsValueType { get; set; }
        public bool IsAnonymousType { get; set; } //?
        public bool IsAnonymousMethod { get; set; }
        public bool IsOverride { get; set; } //?
        public bool IsGeneric { get; set; } //?
        public bool IsSecurityCritical { get; set; }

        public MethodType MethodType { get; set; }
        public AccessType AccessType { get; set; }
    }
}
