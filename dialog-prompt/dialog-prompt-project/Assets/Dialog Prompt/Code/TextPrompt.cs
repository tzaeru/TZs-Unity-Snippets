using UnityEngine;
using System.Collections;

public class TextPrompt : MonoBehaviour {
    public UnityEngine.UI.InputField text_input;

    static string last_value = "";

    public delegate void PromptCallback(string text);
    protected PromptCallback callback = null;

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
	}

    public static GameObject CreatePrompt(PromptCallback _cb = null)
    {
        var prefab = Resources.Load("UI Prefabs/Name Prompt Panel");
        var p_c = GameObject.FindObjectOfType<Canvas>();
        GameObject prompt_object = (GameObject)Instantiate(prefab);
        prompt_object.transform.SetParent(p_c.transform, false);
        if (_cb != null)
            prompt_object.GetComponent<TextPrompt>().AddCallback(_cb);
        return prompt_object;
    }

    public void AddCallback(PromptCallback _cb)
    {
        callback += _cb;
    }

    public void Close()
    {
        print("Closing");
        last_value = text_input.text;
        if (callback != null)
            callback.Invoke(text_input.text);
        Destroy(gameObject);
    }
}
