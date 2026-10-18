using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PoulpyDialogueController : MonoBehaviour
{
    private class DialogueRequest
    {
        public string[] Messages;
        public Action Callback;

        public DialogueRequest(string[] p_messages, Action p_callback)
        {
            Messages = p_messages;
            Callback = p_callback;
        }
    }

    [Header("UI")]
    [SerializeField] private GameObject m_dialogueRoot;
    [SerializeField] private TMP_Text m_dialogueText;
    [SerializeField] private Button m_continueButton;
    [SerializeField] private Button m_dialogueClickArea;
    [SerializeField] private Button m_recallButton;

    [Header("Text Animation")]
    [Min(0.001f)]
    [SerializeField] private float m_secondsPerCharacter = 0.025f;
    [Min(0f)]
    [SerializeField] private float m_inputProtectionDelay = 0.25f;

    [Header("Interaction")]
    [SerializeField] private InteractionRaycaster m_interactionRaycaster;

    [Header("Debug")]
    [SerializeField] private bool m_debugMode = false;

    private readonly Queue<DialogueRequest> m_dialogueQueue = new Queue<DialogueRequest>();

    private string[] m_currentMessages;
    private string[] m_lastMessages;

    private int m_currentMessageIndex = 0;
    private int m_totalCharacterCount = 0;

    private Action m_onSequenceCompleted;
    private Coroutine m_typingCoroutine;

    private bool m_isTyping = false;
    private float m_canAcceptInputTime = 0f;

    public bool IsOpen { get; private set; }
    public bool IsTyping => m_isTyping;

    private void Awake()
    {
        Debug.Assert(m_dialogueRoot != null);
        Debug.Assert(m_dialogueText != null);
        Debug.Assert(m_continueButton != null);
        Debug.Assert(m_dialogueClickArea != null);
        Debug.Assert(m_recallButton != null);

        m_dialogueRoot.SetActive(false);
        m_recallButton.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        m_continueButton.onClick.AddListener(Continue);
        m_dialogueClickArea.onClick.AddListener(CompleteCurrentText);
        m_recallButton.onClick.AddListener(ShowLastDialogue);
    }

    private void OnDisable()
    {
        m_continueButton.onClick.RemoveListener(Continue);
        m_dialogueClickArea.onClick.RemoveListener(CompleteCurrentText);
        m_recallButton.onClick.RemoveListener(ShowLastDialogue);

        StopTyping();

        if (m_interactionRaycaster != null)
            m_interactionRaycaster.enabled = true;
    }

    public bool ShowMessage(string p_message, Action p_onCompleted = null)
    {
        return ShowSequence(new[] { p_message }, p_onCompleted);
    }

    public bool ShowSequence(string[] p_messages, Action p_onCompleted = null)
    {
        if (p_messages == null || p_messages.Length == 0) return false;

        DialogueRequest request = new DialogueRequest(
            (string[])p_messages.Clone(),
            p_onCompleted
        );

        if (IsOpen)
        {
            m_dialogueQueue.Enqueue(request);

            if (m_debugMode)
                Debug.Log($"[PoulpyDialogueController] Dialogue queued. Queue: {m_dialogueQueue.Count}");

            return true;
        }

        StartDialogue(request, true);
        return true;
    }

    public void Continue()
    {
        if (!CanAcceptInput()) return;

        if (m_isTyping)
        {
            CompleteCurrentText();
            return;
        }

        m_currentMessageIndex++;

        if (m_currentMessageIndex < m_currentMessages.Length)
        {
            RefreshMessage();
            return;
        }

        FinishCurrentDialogue();
    }

    public void CompleteCurrentText()
    {
        if (!CanAcceptInput()) return;
        if (!m_isTyping) return;

        StopTyping();

        m_dialogueText.maxVisibleCharacters = m_totalCharacterCount;
        m_isTyping = false;

        if (m_debugMode)
            Debug.Log("[PoulpyDialogueController] Text reveal completed instantly.");
    }

    public void ShowLastDialogue()
    {
        if (IsOpen) return;
        if (m_lastMessages == null || m_lastMessages.Length == 0) return;

        DialogueRequest request = new DialogueRequest(
            (string[])m_lastMessages.Clone(),
            null
        );

        StartDialogue(request, false);

        if (m_debugMode)
            Debug.Log("[PoulpyDialogueController] Last dialogue recalled.");
    }

    private void StartDialogue(DialogueRequest p_request, bool p_storeAsLast)
    {
        m_currentMessages = p_request.Messages;
        m_currentMessageIndex = 0;
        m_onSequenceCompleted = p_request.Callback;

        if (p_storeAsLast)
            m_lastMessages = (string[])p_request.Messages.Clone();

        IsOpen = true;

        if (m_interactionRaycaster != null)
            m_interactionRaycaster.enabled = false;

        m_recallButton.gameObject.SetActive(false);
        m_dialogueRoot.SetActive(true);
        m_dialogueRoot.transform.SetAsLastSibling();

        RefreshMessage();

        if (m_debugMode)
            Debug.Log("[PoulpyDialogueController] Dialogue opened.");
    }

    private void RefreshMessage()
    {
        StopTyping();

        m_dialogueText.text = m_currentMessages[m_currentMessageIndex];
        m_dialogueText.maxVisibleCharacters = 0;
        m_dialogueText.ForceMeshUpdate();

        m_totalCharacterCount = m_dialogueText.textInfo.characterCount;
        m_canAcceptInputTime = Time.unscaledTime + m_inputProtectionDelay;

        m_typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        m_isTyping = true;

        for (int i = 1; i <= m_totalCharacterCount; i++)
        {
            m_dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(m_secondsPerCharacter);
        }

        m_dialogueText.maxVisibleCharacters = m_totalCharacterCount;
        m_isTyping = false;
        m_typingCoroutine = null;
    }

    private void FinishCurrentDialogue()
    {
        StopTyping();

        IsOpen = false;
        m_dialogueRoot.SetActive(false);

        if (m_interactionRaycaster != null)
            m_interactionRaycaster.enabled = true;

        Action callback = m_onSequenceCompleted;

        m_currentMessages = null;
        m_currentMessageIndex = 0;
        m_onSequenceCompleted = null;

        if (m_debugMode)
            Debug.Log("[PoulpyDialogueController] Dialogue completed.");

        callback?.Invoke();

        // Le callback a pu ouvrir lui-même un nouveau dialogue.
        if (IsOpen) return;

        if (m_dialogueQueue.Count > 0)
        {
            DialogueRequest nextRequest = m_dialogueQueue.Dequeue();
            StartDialogue(nextRequest, true);
            return;
        }

        m_recallButton.gameObject.SetActive(
            m_lastMessages != null &&
            m_lastMessages.Length > 0
        );
    }

    private void StopTyping()
    {
        if (m_typingCoroutine != null)
        {
            StopCoroutine(m_typingCoroutine);
            m_typingCoroutine = null;
        }

        m_isTyping = false;
    }

    private bool CanAcceptInput()
    {
        return IsOpen && Time.unscaledTime >= m_canAcceptInputTime;
    }
}