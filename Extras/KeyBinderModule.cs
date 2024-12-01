﻿#if !QC_DISABLED && !QC_DISABLE_BUILTIN_ALL && !QC_DISABLE_BUILTIN_EXTRA
using TheArchive.Core.ModulesAPI;
using UnityEngine;

namespace Hikaria.QC.Extras
{
    public class KeyBinderModule : MonoBehaviour
    {
        private readonly struct Binding
        {
            public readonly KeyCode Key;
            public readonly string Command;

            public Binding(KeyCode key, string command)
            {
                Key = key;
                Command = command;
            }
        }

        private readonly CustomSetting<List<Binding>> _bindings = new CustomSetting<List<Binding>>("bindings", new());

        private QuantumConsole _consoleInstance;
        private bool _blocked = false;

        private void BlockInput() { _blocked = true; }
        private void UnblockInput() { _blocked = false; }

        private void BindToConsoleInstance()
        {
            if (!_consoleInstance) { _consoleInstance = FindObjectOfType<QuantumConsole>(); }
            if (_consoleInstance)
            {
                _consoleInstance.OnActivate += BlockInput;
                _consoleInstance.OnDeactivate += UnblockInput;

                _blocked = _consoleInstance.IsActive || FocusStateManager.CurrentState == eFocusState.ComputerTerminal;
            }
            else
            {
                UnblockInput();
            }
        }

        private void Awake()
        {
            BindToConsoleInstance();
        }

        private void Update()
        {
            if (!_blocked)
            {
                foreach (Binding binding in _bindings.Value)
                {
                    if (InputHelper.GetKeyDown(binding.Key))
                    {
                        try
                        {
                            QuantumConsoleProcessor.InvokeCommand(binding.Command);
                        }
                        catch (System.Exception e) { Logs.LogException(e); }
                    }
                }
            }
        }

        [Command("bind", MonoTargetType.Singleton)]
        [CommandDescription("Binds a given command to a given key, so that every time the key is pressed, the command is invoked.")]
        private void AddBinding(KeyCode key, string command)
        {
            _bindings.Value.Add(new Binding(key, command));
        }

        [Command("unbind", MonoTargetType.Singleton)]
        [CommandDescription("Removes every binding for the given key")]
        private void RemoveBindings(KeyCode key)
        {
            _bindings.Value.RemoveAll(x => x.Key == key);
        }

        [Command("unbind-all", MonoTargetType.Singleton)]
        [CommandDescription("Unbinds every existing key binding")]
        private void RemoveAllBindings()
        {
            _bindings.Value.Clear();
        }

        [Command("display-bindings", MonoTargetType.Singleton)]
        [CommandDescription("Displays all existing bindings on the key binder")]
        private IEnumerable<object> DisplayAllBindings()
        {
            foreach (Binding binding in _bindings.Value.OrderBy(x => x.Key))
            {
                yield return new KeyValuePair<KeyCode, string>(binding.Key, binding.Command);
            }
        }
    }
}
#endif
