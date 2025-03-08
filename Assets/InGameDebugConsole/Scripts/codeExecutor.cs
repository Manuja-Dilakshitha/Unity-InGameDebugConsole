using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace IGDC
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DebugVariable : Attribute { }

    public enum textColors
    {
        white,
        red,
        green,
        yellow,
    }

    public enum commands
    {
        list,
        set,
        help,
    }

    public class codeExecutor : MonoBehaviour
    {
        private TextMeshProUGUI _consoleOutput;
        private TMP_InputField _consoleInput;
        private Button _runButton;

        private List<MonoBehaviour> allScripts = new List<MonoBehaviour>();

        private Dictionary<string, FieldInfo> debugFields = new Dictionary<string, FieldInfo>();
        private Dictionary<string, PropertyInfo> debugProperties = new Dictionary<string, PropertyInfo>();

        public void initialize(TextMeshProUGUI consoleOutput, TMP_InputField consoleInput, Button runButton)
        {
            StartCoroutine(initExecutorIE(consoleOutput, consoleInput, runButton));
        }

        IEnumerator initExecutorIE(TextMeshProUGUI _consoleOutput_, TMP_InputField _consoleInput_, Button _runButton_)
        {
            this._consoleOutput = _consoleOutput_;
            this._consoleInput = _consoleInput_;
            this._runButton = _runButton_;

            _consoleOutput.text = "";
            writeOnConsole("Intializing console...");

            _runButton.onClick.AddListener(() => executeCommand(_consoleInput.text));

            yield return new WaitForSeconds(2);

            writeOnConsole("scanning debug variables...");
            scanDebugVariables();
        }

        public string writeOnConsole(string text, textColors color = textColors.white)
        {
            //this function is used to write new items into the console and keep track of the written items (TBI)
            _consoleOutput.text += $"<color={color}>{text}</color>" + "\n\n";
            return text + '\n';
        }

        public void scanDebugVariables()
        {
            allScripts = new List<MonoBehaviour>(FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None));
            foreach (var script in allScripts)
            {
                Type type = script.GetType();
                foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (Attribute.IsDefined(field, typeof(DebugVariable)))
                    {
                        debugFields[$"{script.GetType().Name}.{field.Name}"] = field;
                        writeOnConsole($"Found DebugVariable: {field.Name} in {type.Name}");
                    }
                }
                foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (Attribute.IsDefined(property, typeof(DebugVariable)))
                    {
                        debugProperties[$"{script.GetType().Name}.{property.Name}"] = property;
                        writeOnConsole($"{script.name}.{property.Name}");
                    }
                }
            }

            Debug.Log($"Found {debugFields.Count + debugProperties.Count} debug variables.");
            writeOnConsole($"Found {debugFields.Count + debugProperties.Count} debug variables.", textColors.green);
        }

        public void setVariable(string name, string value)
        {
            StartCoroutine(setVariableIE(name, value));
        }

        IEnumerator setVariableIE(string _name, string _value)
        {
            writeOnConsole("Intialize set variable...");

            bool varFound = false;

            try
            {
                foreach (MonoBehaviour script in allScripts)
                {
                    string scriptVarKey = $"{script.GetType().Name}.{_name}";

                    //handle fields
                    if (debugFields.TryGetValue(scriptVarKey, out FieldInfo field))
                    {
                        object convertedValue = convertValue(_value, field.FieldType);
                        field.SetValue(script, convertedValue);
                        writeOnConsole($"Set {scriptVarKey} to {convertedValue}", textColors.green);
                        varFound = true;
                        yield break;
                    }

                    //handle properties
                    if (debugProperties.TryGetValue(scriptVarKey, out PropertyInfo property))
                    {
                        object convertedValue = convertValue(_value, property.PropertyType);
                        property.SetValue(script, convertedValue);
                        writeOnConsole($"Set {scriptVarKey} to {convertedValue}", textColors.green);
                        varFound = true;
                        yield break;
                    }
                }

                if (!varFound)
                    writeOnConsole($"Variable '{_name}' not found!", textColors.red);
            }
            catch (Exception e)
            {
                writeOnConsole($"<color={textColors.red}>{e.Message}</color>");
            }
            
        }

        private object convertValue(string value, Type targetType)
        {
            if (targetType == typeof(int) && int.TryParse(value, out int intValue))
                return intValue;

            if (targetType == typeof(float) && float.TryParse(value, out float floatValue))
                return floatValue;

            if (targetType == typeof(bool) && bool.TryParse(value, out bool boolValue))
                return boolValue;

            return value; // Assume string for unknown types
        }

        public void listDebugVariables()
        {
            writeOnConsole("Listing debug variables : ");

            foreach (var (key,value) in debugFields)
                writeOnConsole($"{key}  |  <color={textColors.yellow}>[type: {value.FieldType}] [isPublic: {value.IsPublic}]</color>", textColors.green);


            foreach (var (key, value) in debugProperties)
                writeOnConsole($"{key}  |  <color={textColors.yellow}>[type: {value.PropertyType}] [property]</color>", textColors.green);
        }

        public void executeCommand(string command)
        {
            writeOnConsole($"Executing command : {command}");
            string[] command_details = command.ToLower().Split(' ');

            if (command_details[0] == commands.list.ToString() && command_details.Length == 1)
            {
                //only list command is given
                listDebugVariables();
            }
            else if (command_details[0] == commands.list.ToString() && command_details.Length != 1)
            {
                //invalid command
                writeOnConsole("no arguments are allowed with list command!", textColors.red);
            }

            else if (command_details[0] == commands.set.ToString() && command_details.Length == 3)
            {
                //valid command
                setVariable(command_details[1], command_details[2]);
            }
            else if (command_details[0] == commands.set.ToString() && command_details.Length != 3)
            {
                //invalid amount of params
                writeOnConsole("Invalid set command! Usage: set <variable> <value>", textColors.red);
            }

            else if (command_details[0] == commands.help.ToString())
            {
                //list out all commands and their usage
                writeOnConsole("WIP...");
            }
            else if (command_details[0] == commands.help.ToString() && command_details.Length == 2)
            {
                //show the usage of the requested command
                writeOnConsole("WIP");
            }
            else if (command_details[0] == commands.help.ToString() && command_details.Length > 2)
            {
                writeOnConsole("command 'help' takes only one argument! Type help to get details of all available commands or help <command> to get details of a specific command", textColors.red);
            }
            else
            {
                writeOnConsole("Invlid command, type 'help' to get the list of available commands and their usage", textColors.red);
            }
        }
    }
}
