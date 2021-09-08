using System.Dynamic;
using System.Collections.Generic;

namespace Drill4Net.Target.Common
{
    /// <summary>
    /// The dictionary derived from DynamicObject.
    /// </summary>
    public class DynamicDictionary : DynamicObject
    {
        public int Count => _dict.Count;

        private readonly Dictionary<string, object> _dict = new();

        /******************************************************************************/

        // If you try to get a value of a property
        // not defined in the class, this method is called.
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            string name = binder.Name.ToLower();

            // If the property name is found in a dictionary,
            // set the result parameter to the property value and return true.
            // Otherwise, return false.
            return _dict.TryGetValue(name, out result);
        }

        // If you try to set a value of a property that is
        // not defined in the class, this method is called.
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            // Converting the property name to lowercase
            // so that property names become case-insensitive.
            _dict[binder.Name.ToLower()] = value;

            // You can always add a value to a dictionary,
            // so this method always returns true.
            return true;
        }
    }
}
