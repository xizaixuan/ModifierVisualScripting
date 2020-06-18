using System.Reflection;

namespace UnityEditor.Modifier.EditorCommon.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static bool IsStaticConstant(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetMethod == null)
                return false;

            return propertyInfo.CanRead && propertyInfo.GetMethod.IsStatic && !propertyInfo.CanWrite;
        }
    }
}