﻿using Hikaria.QC.Bootstrap;
using Hikaria.QC.Utilities;
using UnityEngine;

namespace Hikaria.QC.Parsers
{
    public class GameObjectParser : BasicQcParser<GameObject>
    {
        public override GameObject Parse(string value)
        {
            string name = ParseRecursive<string>(value);
            GameObject obj = GameObjectExtensions.Find(name, true);

            if (!obj)
            {
                throw new ParserInputException(QuantumConsoleBootstrap.Localization.Format(55, value));
            }

            return obj;
        }
    }
}
