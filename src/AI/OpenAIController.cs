using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;

[RequireComponent(typeof(AudioSource))]
public class OpenAIController : MonoBehaviour
{
    public enum CharacterRole
    {
        InsuranceSales,
        HospitalStaff
    }


    [Header("TTS")]
    public VoiceVoxSpeechToText voiceVox;


    [Header("录音设置")]
    public float vadThreshold = 0.3f;
    public float silenceLimit = 1f;
    public int sampleRate = 16000;



    [Header("角色模型")]
    public GameObject insuranceSalesPrefab;
    public GameObject hospitalStaffPrefab;



    private GameObject currentCharacterModel;


    private AudioSource audioSource;

    private AudioClip salesIntroClip;
    private AudioClip hospitalSealIntroClip;
    private AudioClip hospitalDocumentIntroClip;
    private float conversationStartTime;

    private AudioClip recordingClip;

    private string micDevice;



    private bool isRecording = false;

    private float silentTime = 0f;

    private bool isProcessing = false;

    private bool isSpeaking = false;

    private bool isReady = false;



    private OpenAIAPI api;

    private List<ChatMessage> messages;



    private CharacterRole currentRole;



    //==================================================
    // Unity
    //==================================================

    void Awake()
    {
        // Awake runs before the trigger enables and starts this controller.
        salesIntroClip = Resources.Load<AudioClip>("AIIntro/sales");
        hospitalSealIntroClip = Resources.Load<AudioClip>("AIIntro/hospital_seal");
        hospitalDocumentIntroClip = Resources.Load<AudioClip>("AIIntro/hospital_document");
    }

    void OnEnable()
    {
        InitSystem();
    }



    void OnDisable()
    {
        ShutdownSystem();
    }





    //==================================================
    // 初始化
    //==================================================

    private void InitSystem()
    {

        isReady = true;


        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;

        audioSource.loop = false;

        audioSource.mute = true;



        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone");
            return;
        }


        micDevice = Microphone.devices[0];



        try
        {
            api = OpenAIConfig.CreateAPI();
        }
        catch (Exception exception)
        {
            isReady = false;
            Debug.LogError("OpenAI key setup failed: " + exception.Message);
            return;
        }


        messages = new List<ChatMessage>();



        //=====================================
        // 随机角色
        //=====================================

        currentRole =
            UnityEngine.Random.value < 0.5f
            ?
            CharacterRole.InsuranceSales
            :
            CharacterRole.HospitalStaff;




        messages.Add(
            new ChatMessage(
                ChatMessageRole.System,
                GetSystemPrompt(currentRole)
            )
        );





        //=====================================
        // 通知Conversation Manager
        //=====================================

        if (AIConversationManager.Instance != null)
        {

            AIConversationManager.AIType type;


            if (currentRole == CharacterRole.HospitalStaff)
            {
                type = AIConversationManager.AIType.Staff;
            }
            else
            {
                type = AIConversationManager.AIType.Salesman;
            }


            AIConversationManager.Instance.StartConversation(type);

        }





        Debug.Log("Spawn Character");


        SpawnCharacterModel();



        Debug.Log("AI System Ready");

    }





    //==================================================
    // 外部开始
    //==================================================

    public void StartConversation()
    {

        if (!isReady)
            return;



        Debug.Log("Start Conversation");

        conversationStartTime = Time.realtimeSinceStartup;



        // StartCoroutine runs the introduction immediately, up to its first yield.
        StartCoroutine(StartAIIntroduction());

        StartMicrophone();

        Debug.Log($"[AI Timing] microphone_start_ms={(Time.realtimeSinceStartup - conversationStartTime) * 1000f:F0}");

    }





    //==================================================
    // AI开场
    //==================================================

    private IEnumerator StartAIIntroduction()
    {

        string prompt;
        string introText;
        AudioClip introClip;



        //=====================================
        // Salesman
        //=====================================

        if (currentRole == CharacterRole.InsuranceSales)
        {

            introText = "はじめまして。保険会社の佐藤と申します。少しだけお話を聞いていただけますか。";
            introClip = salesIntroClip;

            prompt =
                "あなたは保険会社の営業担当・佐藤です。" +
                "初対面の相手に自然に自己紹介してください。" +
                "100文字以内で話してください。";

        }


        //=====================================
        // Hospital Staff
        //=====================================

        else
        {

            string requestItem;



            // 随机任务
            if (UnityEngine.Random.value < 0.5f)
            {

                requestItem = "印鑑";
                introText = "こんにちは。病院受付の山田です。お手続きには印鑑が必要です。ご用意いただけますか。";
                introClip = hospitalSealIntroClip;


                if (AIConversationManager.Instance != null)
                {
                    AIConversationManager.Instance.SetRequiredItem(
                        AIConversationManager.RequiredItem.Seal
                    );
                }

            }
            else
            {

                requestItem = "確認書";
                introText = "こんにちは。病院受付の山田です。お手続きには確認書が必要です。ご用意いただけますか。";
                introClip = hospitalDocumentIntroClip;


                if (AIConversationManager.Instance != null)
                {
                    AIConversationManager.Instance.SetRequiredItem(
                        AIConversationManager.RequiredItem.Document
                    );
                }

            }



            prompt =
                "あなたは病院の受付スタッフです。山田です。" +
                $"患者に必要な「{requestItem}」について説明してください。" +
                "100文字以内で話してください。";

        }



        if (introClip == null)
        {
            Debug.LogWarning("[AI Timing] intro clip missing; using generated opening line");
            yield return ChatAndSpeak(prompt);
            yield break;
        }

        messages.Add(new ChatMessage(ChatMessageRole.Assistant, introText));
        isSpeaking = true;
        audioSource.clip = introClip;
        audioSource.spatialBlend = 0f;
        audioSource.mute = false;
        audioSource.Play();
        Debug.Log($"[AI Timing] intro_first_audio_ms={(Time.realtimeSinceStartup - conversationStartTime) * 1000f:F0} role={currentRole}");

        while (audioSource.isPlaying)
            yield return null;

        audioSource.mute = true;
        isSpeaking = false;

    }







    //==================================================
    // 麦克风
    //==================================================

    private void StartMicrophone()
    {

        micDevice = Microphone.devices[0];


        recordingClip =
            Microphone.Start(
                micDevice,
                true,
                10,
                sampleRate
            );


        Debug.Log("Microphone Start");

    }



    private void StopMicrophone()
    {

        if (
            micDevice != null &&
            Microphone.IsRecording(micDevice)
        )
        {
            Microphone.End(micDevice);
        }


        recordingClip = null;

    }








    //==================================================
    // Update VAD
    //==================================================

    void Update()
    {

        if (
            !isReady ||
            recordingClip == null ||
            isProcessing
        )
            return;



        int pos = Microphone.GetPosition(micDevice);


        if (pos <= 0)
            return;



        float[] samples = new float[1024];


        int start =
            Mathf.Max(
                0,
                pos - samples.Length
            );


        recordingClip.GetData(
            samples,
            start
        );



        float maxLevel = 0;



        foreach (float s in samples)
        {
            maxLevel =
                Mathf.Max(
                    maxLevel,
                    Mathf.Abs(s)
                );
        }




        // 用户打断AI

        if (
            isSpeaking &&
            maxLevel > vadThreshold
        )
        {

            StopAllCoroutines();

            audioSource.Stop();

            audioSource.mute = true;

            isSpeaking = false;

            isProcessing = false;


            Debug.Log("User Interrupt");

        }






        if (
            !isRecording &&
            maxLevel > vadThreshold
        )
        {

            isRecording = true;

            silentTime = 0;

        }




        if (isRecording)
        {

            if (maxLevel < vadThreshold)
            {

                silentTime += Time.deltaTime;


                if (silentTime >= silenceLimit)
                {

                    isRecording = false;


                    if (!isSpeaking)
                    {

                        isProcessing = true;


                        StartCoroutine(
                            TranscribeAndChat()
                        );

                    }

                }

            }
            else
            {

                silentTime = 0;

            }

        }

    }









    //==================================================
    // GPT + VoiceVox
    //==================================================

    private IEnumerator ChatAndSpeak(string input)
    {

        isSpeaking = true;
        float chatStartTime = Time.realtimeSinceStartup;



        messages.Add(
            new ChatMessage(
                ChatMessageRole.User,
                input
            )
        );



        var request =
            new ChatRequest
            {

                Model = Model.ChatGPTTurbo,

                Messages = messages,

                MaxTokens = 120,

                Temperature = 0.5f

            };



        var task =
            api.Chat.CreateChatCompletionAsync(request);



        while (!task.IsCompleted)
            yield return null;

        Debug.Log($"[AI Timing] chat_response_ms={(Time.realtimeSinceStartup - chatStartTime) * 1000f:F0}");




        var response =
            task.Result
            .Choices[0]
            .Message;



        messages.Add(response);




        audioSource.mute = false;



        float ttsStartTime = Time.realtimeSinceStartup;
        yield return voiceVox.TextToSpeech(
            response.Content
        );
        Debug.Log($"[AI Timing] tts_total_ms={(Time.realtimeSinceStartup - ttsStartTime) * 1000f:F0}");



        audioSource.mute = true;



        isSpeaking = false;

    }









    //==================================================
    // Whisper
    //==================================================

    private IEnumerator TranscribeAndChat()
    {

        float sttStartTime = Time.realtimeSinceStartup;

        byte[] wavData =
            WavUtility.FromAudioClip(
                recordingClip
            );



        WWWForm form =
            new WWWForm();



        form.AddBinaryData(
            "file",
            wavData,
            "recording.wav",
            "audio/wav"
        );


        form.AddField(
            "model",
            "base"
        );


        form.AddField(
            "language",
            "ja"
        );



        UnityWebRequest request =
            UnityWebRequest.Post(
                "http://localhost:9000/v1/audio/transcriptions",
                form
            );



        yield return request.SendWebRequest();

        Debug.Log($"[AI Timing] transcription_ms={(Time.realtimeSinceStartup - sttStartTime) * 1000f:F0}");




        if (request.result != UnityWebRequest.Result.Success)
        {

            isProcessing = false;

            yield break;

        }




        string text =
            request.downloadHandler.text.Trim();





        if (!string.IsNullOrEmpty(text))
        {

            if (AIConversationManager.Instance != null)
            {

                AIConversationManager.Instance.ReceiveUserText(text);

            }



            yield return ChatAndSpeak(text);

        }



        isProcessing = false;

    }










    //==================================================
    // 创建角色
    //==================================================

    private void SpawnCharacterModel()
    {

        if (currentCharacterModel != null)
            Destroy(currentCharacterModel);



        currentCharacterModel =
            Instantiate(
                currentRole ==
                CharacterRole.InsuranceSales
                ?
                insuranceSalesPrefab
                :
                hospitalStaffPrefab
            );



        currentCharacterModel.transform.position =
            new Vector3(
                4.9859f,
                0.555f,
                -0.15971f
            );



        currentCharacterModel.transform.rotation =
            Quaternion.Euler(
                0f,
                -90f,
                0f
            );

    }







    //==================================================
    // Prompt
    //==================================================

    private string GetSystemPrompt(CharacterRole role)
    {

        if (role == CharacterRole.InsuranceSales)
        {
            return
            "あなたは保険会社の営業担当・佐藤です。自然で短く話してください。";
        }



        return
        "あなたは病院受付スタッフ・山田です。丁寧に案内してください。";

    }








    //==================================================
    // Shutdown
    //==================================================

    private void ShutdownSystem()
    {

        isReady = false;


        StopAllCoroutines();


        StopMicrophone();



        if (audioSource != null)
        {

            audioSource.Stop();

            audioSource.mute = true;

        }



        if (currentCharacterModel != null)
            Destroy(currentCharacterModel);




        isRecording = false;

        isProcessing = false;

        isSpeaking = false;





        if (AIConversationManager.Instance != null)
        {

            AIConversationManager.Instance.EndConversation();

        }



        Debug.Log("AI Stop");

    }

}
