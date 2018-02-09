using System;

namespace EntityWorker.Core.Attributes
{
    /// <inheritdoc />
    /// <summary>
    /// https://github.com/AlenToma/EntityWorker.Core/blob/master/Documentation/Attributes.md
    /// This attr will tell LightDataTable abstract to not auto Delete this object when deleting parent, it will however try to create new or update  
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public sealed class IndependentData : Attribute
    {

    }
}
