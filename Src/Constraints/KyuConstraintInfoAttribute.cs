using System;

namespace KyudosudokuWebsite
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    sealed class KyuConstraintInfoAttribute : Attribute
    {
        public string Name { get; private set; }
        public KyuConstraintInfoAttribute(string name) { Name = name; }
    }
}
