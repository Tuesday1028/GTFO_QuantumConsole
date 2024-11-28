using Hikaria.QC.Bootstrap;
using Hikaria.QC.Utilities;

namespace Hikaria.QC.Parsers
{
    public class EnumParser : PolymorphicCachedQcParser<Enum>
    {
        public override Enum Parse(string value, Type type)
        {
            try
            {
                return (Enum)Enum.Parse(type, value);
            }
            catch (Exception e)
            {
                throw new ParserInputException(QuantumConsoleBootstrap.Localization.Format(54, value, type.GetDisplayName(), type), e);
            }
        }
    }
}