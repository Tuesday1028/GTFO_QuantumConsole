﻿using Hikaria.QC.Bootstrap;
using System.Runtime.CompilerServices;

namespace Hikaria.QC
{
    /// <summary>
    /// Marks the associated method as a command, allowing it to be loaded by the QuantumConsoleProcessor. This means it will be usable as a command from a Quantum Console.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
    public sealed class CommandAttribute : Attribute
    {
        public readonly string Alias;
        public readonly string Description;
        public readonly Platform SupportedPlatforms;
        public readonly MonoTargetType MonoTarget;
        public readonly bool Valid = true;

        private static readonly char[] _bannedAliasChars = new char[] { ' ', '(', ')', '{', '}', '[', ']', '<', '>' };

        public CommandAttribute([CallerMemberName] string aliasOverride = "", Platform supportedPlatforms = Platform.AllPlatforms, MonoTargetType targetType = MonoTargetType.Single)
        {
            Alias = aliasOverride;
            MonoTarget = targetType;
            SupportedPlatforms = supportedPlatforms;

            for (int i = 0; i < _bannedAliasChars.Length; i++)
            {
                if (Alias.Contains(_bannedAliasChars[i]))
                {
                    string errorMessage = QuantumConsoleBootstrap.Localization.Format(40, Alias, _bannedAliasChars[i]);
                    Logs.LogError(errorMessage);
                    Valid = false;
                    throw new ArgumentException(errorMessage, nameof(aliasOverride));
                }
            }
        }

        public CommandAttribute(string aliasOverride, MonoTargetType targetType, Platform supportedPlatforms = Platform.AllPlatforms) : this(aliasOverride, supportedPlatforms, targetType) { }

        public CommandAttribute(string aliasOverride, string description, Platform supportedPlatforms = Platform.AllPlatforms, MonoTargetType targetType = MonoTargetType.Single) : this(aliasOverride, supportedPlatforms, targetType)
        {
            Description = description;
        }

        public CommandAttribute(string aliasOverride, string description, MonoTargetType targetType, Platform supportedPlatforms = Platform.AllPlatforms) : this(aliasOverride, description, supportedPlatforms, targetType) { }
    }
}
