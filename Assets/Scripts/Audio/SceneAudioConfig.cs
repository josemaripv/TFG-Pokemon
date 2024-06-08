using UnityEngine;

[CreateAssetMenu(fileName = "SceneAudioConfig", menuName = "Audio/SceneAudioConfig")]
public class SceneAudioConfig : ScriptableObject
{
    public string sceneName;
    public AudioClip musicClip;
}
