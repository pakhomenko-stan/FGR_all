namespace FGR.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed public class ImplementableAttribute(string implementableTypeName) : Attribute
    {
        public string ImplementableTypeName
        {
            get { return implementableTypeName; }
        }
    }
}
