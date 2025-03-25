using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Linq;
using System;
using Unity.VisualScripting;
using System.Reflection;

namespace IGDC
{
    public class inGameDebugConsole : MonoBehaviour
    {
        [SerializeField]
        private GameObject debugPanel;

        [SerializeField]
        private GameObject logItemPrefab, logItemParent;

        [SerializeField]
        private Sprite logIcon, warningIcon, errorIcon;

        [SerializeField]
        private Button toggleButton;

        private List<(string message, string stackTrace, LogType type)> logEntries = new List<(string message, string stackTrace, LogType type)>();
        //private List<(string message, string stackTrace, LogType type)> enabledLogEntries = new List<(string message, string stackTrace, LogType type)>();

        [SerializeField]
        private TextMeshProUGUI descriptionText;

        private bool logEnabled = true, warningsEnabled = true, errorsEnabled = true, commandEnabled = false;

        [SerializeField]
        private Button logButton, warningsButton, errorsButton, clearButton, commandButton;

        [SerializeField]
        private TextMeshProUGUI consoleOutput;

        [SerializeField]
        private TMP_InputField consoleInput;

        [SerializeField]
        private Button runButton;

        [SerializeField]
        private GameObject commandPanel;

        codeExecutor executor;

        [DebugVariable]
        public int value = 0;

        private void Awake()
        {
            //debugPanel.SetActive(false);

            Application.logMessageReceived += handleLog;

            if (toggleButton != null)
                toggleButton.onClick.AddListener(toggleDebugPanel);

            logButton.onClick.AddListener(delegate { filterButton("log"); });
            warningsButton.onClick.AddListener(delegate { filterButton("warnings"); });
            errorsButton.onClick.AddListener(delegate { filterButton("errors"); });
            commandButton.onClick.AddListener(delegate { filterButton("command"); });
            clearButton.onClick.AddListener(clearAll);
            
            //runButton.onClick.AddListener(delegate { executor.executeCommand(consoleInput.text); });
        }

        private void handleLog(string logString, string stackTrace, LogType type)
        {
            logEntries.Add((logString, stackTrace, type));

            filter(); //filter and spawn log items
        }

        void showErrorDescription(string stackTrace)
        {
            descriptionText.text = stackTrace;
        }

        void toggleDebugPanel()
        {
            debugPanel.SetActive(!debugPanel.activeSelf);
        }

        //TODO : remove this
        private void Start()
        {
            Debug.Log("In Game Debug Console Started");
            StartCoroutine(testErrorIE());

            executor = this.AddComponent<codeExecutor>();
            executor.initialize(consoleOutput, consoleInput, runButton);
            //executor.scanDebugVariables();
        }

        IEnumerator testErrorIE()
        {
            yield return new WaitForSeconds(5);

            Debug.Log(logEntries[10]);

            yield return null;
        }

        //add functionality to the filter buttons
        void filterButton(string type)
        {
            switch (type)
            {
                case "log":
                    logEnabled = !logEnabled;
                    setButtonColor(logButton, logEnabled);
                    break;

                case "warnings":
                    warningsEnabled = !warningsEnabled;
                    setButtonColor(warningsButton, warningsEnabled);
                    break;

                case "errors":
                    errorsEnabled = !errorsEnabled;
                    setButtonColor(errorsButton, errorsEnabled);
                    break;

                case "command":
                    commandEnabled = !commandEnabled;
                    setButtonColor(commandButton, commandEnabled);
                    commandPanel.SetActive(commandEnabled);
                    break;

                default:
                    errorsEnabled = !errorsEnabled;
                    warningsEnabled = !warningsEnabled;
                    logEnabled = !logEnabled;
                    break;
            }

            filter();
        }

        void filter()
        {
            clearAllLogItems();

            var allowedTypes = new HashSet<LogType>();

            if (logEnabled)
                allowedTypes.Add(LogType.Log);

            if (warningsEnabled)
                allowedTypes.Add(LogType.Warning);

            if (errorsEnabled)
            {
                allowedTypes.Add(LogType.Error);
                allowedTypes.Add(LogType.Exception);
            }

            foreach (var (message, stackTrace, type) in logEntries)
            {
                if (allowedTypes.Contains(type))
                {
                    spawnLogItem(message, stackTrace, type);
                }
            }
        }

        //set button colors based on their state
        void setButtonColor(Button button, bool active)
        {
            Image buttonImage = button.gameObject.GetComponent<Image>();

            if (active)
                buttonImage.color = new Color(0.31f, 0.31f, 0.31f);
            else
                buttonImage.color = new Color(0.22f, 0.22f, 0.22f);
        }

        void clearAllLogItems()
        {
            //delete all gameobjects (logitems) from the UI and log list. Keeps the actual logs in logQueue
            foreach (Transform t in logItemParent.transform)
            {
                GameObject.Destroy(t.gameObject);
            }
        }

        void spawnLogItem(string message, string stackTrace, LogType type)
        {
            GameObject logItem = Instantiate(logItemPrefab, logItemParent.transform);
            logItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = $"[{type}]: {message}";
            logItem.GetComponent<Button>().onClick.AddListener(delegate { showErrorDescription(stackTrace); });

            //assign appearance
            switch (type)
            {
                case LogType.Log:
                    logItem.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
                    logItem.transform.GetChild(0).GetComponent<Image>().sprite = logIcon;
                    break;

                case LogType.Warning:
                    logItem.GetComponent<Image>().color = new Color(140f / 255f, 118f / 255f, 30f / 255f, 0.6f);
                    logItem.transform.GetChild(0).GetComponent<Image>().sprite = warningIcon;
                    break;

                case LogType.Error:
                    logItem.GetComponent<Image>().color = new Color(120f / 255f, 0f, 0f, 0.6f);
                    logItem.transform.GetChild(0).GetComponent<Image>().sprite = errorIcon;
                    break;

                case LogType.Exception:
                    logItem.GetComponent<Image>().color = new Color(120f / 255f, 0f, 0f, 0.6f);
                    logItem.transform.GetChild(0).GetComponent<Image>().sprite = errorIcon;
                    break;

                default:
                    logItem.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
                    logItem.transform.GetChild(0).GetComponent<Image>().sprite = logIcon;
                    break;
            }
        }

        void clearAll()
        {
            clearAllLogItems();
            logEntries.Clear();
        }
    }

    
}



