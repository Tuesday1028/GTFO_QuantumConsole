using Hikaria.QC.Utilities;

namespace Hikaria.QC.Serializers
{
    public class TypeSerialiazer : PolymorphicQcSerializer<Type>
    {
        public override string SerializeFormatted(Type value, QuantumTheme theme)
        {
            return value.GetDisplayName();
        }
    }
}
