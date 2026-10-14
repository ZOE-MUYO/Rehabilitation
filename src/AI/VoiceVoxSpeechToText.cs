using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json.Linq;

public class VoiceVoxSpeechToText : MonoBehaviour
{
    private const string VoiceVoxUrl = "http://localhost:50021";
    private string audioPath => Path.Combine(Application.persistentDataPath, "voice", "tts_output.wav");

    private static AudioSource currentAudioSource;

    //==================================================
    // TTS 主流程
    //==================================================
    public IEnumerator TextToSpeech(string text)
    {
        float ttsStartTime = Time.realtimeSinceStartup;
        if (string.IsNullOrEmpty(text))
        {
            Debug.LogWarning("⚠️ TTS输入为空");
            yield break;
        }

        // =========================
        // ① audio_query
        // =========================
        string queryUrl =
            $"{VoiceVoxUrl}/audio_query?text={UnityWebRequest.EscapeURL(text)}&speaker=13";

        UnityWebRequest queryRequest = UnityWebRequest.PostWwwForm(queryUrl, "");
        queryRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return queryRequest.SendWebRequest();
        Debug.Log($"[AI Timing] voicevox_query_ms={(Time.realtimeSinceStartup - ttsStartTime) * 1000f:F0}");

        if (queryRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ audio_query失败: " + queryRequest.error);
            yield break;
        }

        string queryJson = queryRequest.downloadHandler.text;

        // =========================
        // ② 语音参数调整
        // =========================
        queryJson = ApplyVoiceSettings(queryJson);

        // =========================
        // ③ synthesis
        // =========================
        UnityWebRequest synthesisRequest =
            new UnityWebRequest($"{VoiceVoxUrl}/synthesis?speaker=13", "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(queryJson);
        synthesisRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        synthesisRequest.downloadHandler = new DownloadHandlerBuffer();
        synthesisRequest.SetRequestHeader("Content-Type", "application/json");

        float synthesisStartTime = Time.realtimeSinceStartup;
        yield return synthesisRequest.SendWebRequest();
        Debug.Log($"[AI Timing] voicevox_synthesis_ms={(Time.realtimeSinceStartup - synthesisStartTime) * 1000f:F0}");

        if (synthesisRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("❌ synthesis失败: " + synthesisRequest.error);
            yield break;
        }

        // =========================
        // ④ 保存音频
        // =========================
        byte[] audioData = synthesisRequest.downloadHandler.data;

        string folder = Path.GetDirectoryName(audioPath);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        File.WriteAllBytes(audioPath, audioData);
        Debug.Log("💾 TTS保存完成: " + audioPath);

        // =========================
        // ⑤ 播放音频
        // =========================
        StopCurrentAudio();

        GameObject playerObj = new GameObject("TTSPlayer");
        AudioSource audioSource = playerObj.AddComponent<AudioSource>();
        currentAudioSource = audioSource;

        using (UnityWebRequest www =
               UnityWebRequestMultimedia.GetAudioClip("file:///" + audioPath, AudioType.WAV))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ 音频加载失败: " + www.error);
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);

            audioSource.clip = clip;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;

            audioSource.Play();
            Debug.Log($"[AI Timing] voicevox_first_audio_ms={(Time.realtimeSinceStartup - ttsStartTime) * 1000f:F0}");

            Debug.Log("🔊 播放中: " + text);

            yield return new WaitForSeconds(clip.length);
        }

        Destroy(playerObj);

        // ==================================================
        // ⭐新增：通知 AIConversationManager（TTS结束）
        // ==================================================
        if (AIConversationManager.Instance != null)
        {
            // 可用于未来扩展：统计AI发言结束节点
            Debug.Log("🧠 AI speech finished (notified)");
        }
    }

    private string ApplyVoiceSettings(string json)
    {
        JObject obj = JObject.Parse(json);

        obj["speedScale"] = 0.8f;
        obj["pitchScale"] = 0.07f;
        obj["intonationScale"] = 1.62f;

        return obj.ToString();
    }

    private void StopCurrentAudio()
    {
        if (currentAudioSource != null)
        {
            if (currentAudioSource.isPlaying)
            {
                currentAudioSource.Stop();
                Debug.Log("停止旧语音");
            }

            Destroy(currentAudioSource.gameObject);
            currentAudioSource = null;
        }
    }
}
