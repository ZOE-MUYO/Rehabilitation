# AI Dialogue for VR Cooking Rehabilitation

这是一个用于认知康复研究的 Unity VR 烹饪任务原型。参与者在准备食材和烹饪时，会遇到门铃打断、炉火安全检查和语音对话。系统记录任务时间、操作错误、门铃反应以及对话相关指标，供研究分析。

## 作品演示

- [观看 VR 演示视频（Google Drive）](https://drive.google.com/file/d/1d_tEYn_Wber8fK6hzD0o1bd1k-9R_X6b/view)
- [查看研究海报（PDF）](https://drive.google.com/file/d/1RC5-_K436RpYnbKnjcjFtELZ6ttTq1nW/view)

视频和海报由项目作者提供。若链接提示需要权限，请联系作者获取访问权限。

## 系统流程

1. 在 Meta Quest 2 中完成读食谱、取食材、切菜和烹饪任务。
2. 门铃在任务过程中响起，参与者需要注意门铃并处理炉火安全问题。
3. 虚拟角色发起对话。语音经 OpenAI Whisper 转写，角色回复由 GPT 生成，再由 VOICEVOX 合成日语语音。
4. 系统将任务与对话指标写入本地 SQLite 数据库。

## 仓库内容

| 路径 | 内容 |
| --- | --- |
| [`src/AI`](src/AI) | 语音输入、对话状态、角色回复与语音合成 |
| [`src/Tasks`](src/Tasks) | 烹饪任务事件与行为指标记录 |
| [`src/Interaction`](src/Interaction) | 门铃、炉火和场景交互逻辑 |

这里展示精选的项目代码，不包含 Unity 场景、素材包、第三方 SDK、实验数据或可执行安装包。因此本仓库不能单独作为完整 Unity 工程运行。公开版本把几处本地绝对路径改成了 `Application.persistentDataPath` 或 `OPENAI_API_KEY_FILE` 环境变量；密钥文件始终应存放在仓库外。

本项目是研究原型。海报中的初步观察不应被解读为临床疗效结论。

---

## English

This repository presents selected C# scripts from a Unity VR cooking task prototype for cognitive rehabilitation research. The experience combines cooking activities, a doorbell interruption, fire safety decisions, and spoken dialogue. Whisper transcribes speech, GPT generates character responses, and VOICEVOX speaks them in Japanese. Task and dialogue events are logged locally for analysis.

[Watch the demo video](https://drive.google.com/file/d/1d_tEYn_Wber8fK6hzD0o1bd1k-9R_X6b/view) · [View the research poster](https://drive.google.com/file/d/1RC5-_K436RpYnbKnjcjFtELZ6ttTq1nW/view)

This is a code showcase, not a standalone Unity project or a clinical product. Licensed assets, scenes, SDKs, participant data, and API credentials are excluded. The public code adapts local file paths for presentation.
