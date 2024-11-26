namespace Hikaria.QC.Serializers
{
    public class UnityObjectSerializer : PolymorphicQcSerializer<UnityEngine.Object>
    {
        public override string SerializeFormatted(UnityEngine.Object value, QuantumTheme theme)
        {
            return value.name;
        }
    }
}
