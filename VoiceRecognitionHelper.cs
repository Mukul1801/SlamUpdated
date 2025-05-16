using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

[System.Serializable]
public class VoiceCommand
{
    public string command;
    public string[] alternateCommands;
    public string description;
}

public class VoiceRecognitionHelper : MonoBehaviour
{
    // Listeners
    public StringEvent OnCommandRecognized = new StringEvent();
    
    [Header("Voice Recognition Settings")]
    public VoiceCommand[] predefinedCommands;
    public float recognitionConfidenceThreshold = 0.7f;
    public bool showDebugInfo = true;
    public float listeningTimeout = 5.0f;
    
    [Header("Audio Feedback")]
    public AudioSource audioSource;
    public AudioClip startListeningSound;
    public AudioClip stopListeningSound;
    public AudioClip recognizedCommandSound;
    
    // State tracking
    private bool isInitialized = false;
    private bool isListening = false;
    private string lastRecognizedCommand = "";
    private AndroidJavaObject speechRecognizer;
    private AndroidJavaObject speechRecognizerIntent;
    private bool isSupportedPlatform = false;
    
    // Default commands
    private VoiceCommand[] defaultCommands = new VoiceCommand[]
    {
        new VoiceCommand 
        { 
            command = "Start navigation", 
            alternateCommands = new string[] { "Begin navigation", "Navigate", "Start guiding" },
            description = "Start following the loaded path"
        },
        new VoiceCommand 
        { 
            command = "Stop navigation", 
            alternateCommands = new string[] { "End navigation", "Stop guiding", "Pause navigation" },
            description = "Stop the current navigation session"
        },
        new VoiceCommand 
        { 
            command = "Where am I", 
            alternateCommands = new string[] { "Current location", "My position", "Location status" },
            description = "Announce your current position and status"
        },
        new VoiceCommand 
        { 
            command = "What's ahead", 
            alternateCommands = new string[] { "Describe surroundings", "What's around me", "Scan area" },
            description = "Describe what's in front of you"
        },
        new VoiceCommand 
        { 
            command = "Help", 
            alternateCommands = new string[] { "Instructions", "Commands", "How to use" },
            description = "Get help about how to use the app"
        },
        new VoiceCommand 
        { 
            command = "Open menu", 
            alternateCommands = new string[] { "Accessibility menu", "Settings menu", "Options" },
            description = "Open the accessibility settings menu"
        },
        new VoiceCommand 
        { 
            command = "Close menu", 
            alternateCommands = new string[] { "Exit menu", "Hide menu" },
            description = "Close the currently open menu"
        },
        new VoiceCommand 
        { 
            command = "Load path", 
            alternateCommands = new string[] { "Open path", "Browse paths", "Select path" },
            description = "Open the path loading screen"
        },
        new VoiceCommand 
        { 
            command = "Save path", 
            alternateCommands = new string[] { "Store path", "Save route", "Save current path" },
            description = "Save the current path to storage"
        },
        new VoiceCommand 
        { 
            command = "Emergency help", 
            alternateCommands = new string[] { "Emergency", "Help me", "I'm lost" },
            description = "Get immediate assistance in case of emergency"
        }
    };
    
    void Awake()
    {
        // Check if we're on a supported platform
        isSupportedPlatform = Application.platform == RuntimePlatform.Android;
        
        // Set up audio source if needed
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Set up default commands if none provided
        if (predefinedCommands == null || predefinedCommands.Length == 0)
        {
            predefinedCommands = defaultCommands;
        }
    }
    
    public void Initialize()
    {
        if (!isSupportedPlatform)
        {
            Debug.LogWarning("Voice recognition is only supported on Android platforms.");
            return;
        }
        
        if (isInitialized)
            return;
        
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    // Create the speech recognizer
                    AndroidJavaClass speechRecognizerClass = new AndroidJavaClass("android.speech.SpeechRecognizer");
                    speechRecognizer = speechRecognizerClass.CallStatic<AndroidJavaObject>("createSpeechRecognizer", activity);
                    
                    // Set up the listener
                    AndroidJavaClass listenerClass = new AndroidJavaClass("com.yourcompany.accessibility.SpeechRecognitionListener");
                    if (listenerClass != null)
                    {
                        AndroidJavaObject listener = listenerClass.CallStatic<AndroidJavaObject>("getInstance", activity);
                        speechRecognizer.Call("setRecognitionListener", listener);
                    }
                    else
                    {
                        Debug.LogError("SpeechRecognitionListener class not found. Make sure to include the appropriate plugin.");
                        return;
                    }
                    
                    // Create the intent
                    AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
                    speechRecognizerIntent = new AndroidJavaObject("android.content.Intent", intentClass.GetStatic<string>("ACTION_RECOGNIZE_SPEECH"));
                    
                    // Configure the intent
                    AndroidJavaClass recognizerIntent = new AndroidJavaClass("android.speech.RecognizerIntent");
                    speechRecognizerIntent.Call<AndroidJavaObject>("putExtra",
                        recognizerIntent.GetStatic<string>("EXTRA_LANGUAGE_MODEL"),
                        recognizerIntent.GetStatic<string>("LANGUAGE_MODEL_FREE_FORM"));
                        
                    speechRecognizerIntent.Call<AndroidJavaObject>("putExtra",
                        recognizerIntent.GetStatic<string>("EXTRA_PARTIAL_RESULTS"),
                        true);
                        
                    speechRecognizerIntent.Call<AndroidJavaObject>("putExtra",
                        recognizerIntent.GetStatic<string>("EXTRA_MAX_RESULTS"),
                        5);
                }
            }
            
            isInitialized = true;
            Debug.Log("Voice recognition initialized successfully");
        }
        catch (Exception e)
        {
            Debug.LogError("Error initializing voice recognition: " + e.Message);
        }
#endif

#if UNITY_EDITOR
        // Simulated initialization for editor testing
        isInitialized = true;
        Debug.Log("Voice recognition simulated in Unity Editor");
#endif
    }
    
    public void StartListening()
    {
        if (!isInitialized || isListening)
            return;
            
        // Play feedback sound
        if (audioSource != null && startListeningSound != null)
        {
            audioSource.PlayOneShot(startListeningSound);
        }
        
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            if (speechRecognizer != null && speechRecognizerIntent != null)
            {
                // Start listening
                speechRecognizer.Call("startListening", speechRecognizerIntent);
                isListening = true;
                
                // Start timeout coroutine
                StartCoroutine(ListeningTimeout());
                
                if (showDebugInfo)
                    Debug.Log("Voice recognition started listening");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error starting speech recognition: " + e.Message);
        }
#endif

#if UNITY_EDITOR
        // Simulate listening in editor with a test command after a delay
        isListening = true;
        StartCoroutine(SimulateCommand());
#endif
    }
    
    public void StopListening()
    {
        if (!isListening)
            return;
            
        // Play feedback sound
        if (audioSource != null && stopListeningSound != null)
        {
            audioSource.PlayOneShot(stopListeningSound);
        }
        
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            if (speechRecognizer != null)
            {
                // Stop listening
                speechRecognizer.Call("stopListening");
                isListening = false;
                
                if (showDebugInfo)
                    Debug.Log("Voice recognition stopped listening");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error stopping speech recognition: " + e.Message);
        }
#endif

#if UNITY_EDITOR
        // End simulated listening in editor
        isListening = false;
#endif
    }
    
    private IEnumerator ListeningTimeout()
    {
        yield return new WaitForSeconds(listeningTimeout);
        
        if (isListening)
        {
            StopListening();
        }
    }
    
#if UNITY_EDITOR
    private IEnumerator SimulateCommand()
    {
        // Wait a random amount of time (0.5 to 2 seconds)
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 2.0f));
        
        // Pick a random command
        if (predefinedCommands != null && predefinedCommands.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, predefinedCommands.Length);
            string simulatedCommand = predefinedCommands[randomIndex].command;
            
            // Process the command
            OnRecognizedSpeech(simulatedCommand);
        }
        
        isListening = false;
    }
#endif
    
    // Called by native code through JNI when speech is recognized
    public void OnRecognizedSpeech(string command)
    {
        // Play feedback sound
        if (audioSource != null && recognizedCommandSound != null)
        {
            audioSource.PlayOneShot(recognizedCommandSound);
        }
        
        lastRecognizedCommand = command;
        
        if (showDebugInfo)
            Debug.Log("Voice command recognized: " + command);
        
        // Trigger the event
        OnCommandRecognized.Invoke(command);
    }
    
    void OnDestroy()
    {
        // Clean up the speech recognizer
        if (isInitialized)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (speechRecognizer != null)
            {
                speechRecognizer.Call("destroy");
                speechRecognizer.Dispose();
                speechRecognizer = null;
            }
            
            if (speechRecognizerIntent != null)
            {
                speechRecognizerIntent.Dispose();
                speechRecognizerIntent = null;
            }
#endif
        }
    }
    
    // Utility method to get available voice commands as a formatted string
    public string GetAvailableCommandsText()
    {
        if (predefinedCommands == null || predefinedCommands.Length == 0)
            return "No voice commands available.";
            
        string commandsText = "Available voice commands:\n";
        
        foreach (var command in predefinedCommands)
        {
            commandsText += "• " + command.command;
            if (!string.IsNullOrEmpty(command.description))
            {
                commandsText += " - " + command.description;
            }
            commandsText += "\n";
        }
        
        return commandsText;
    }
}