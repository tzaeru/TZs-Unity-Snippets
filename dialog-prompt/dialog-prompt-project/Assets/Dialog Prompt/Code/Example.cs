using UnityEngine;

public class Example : MonoBehaviour {

    public UnityEngine.UI.Text text;

	public void ShowPrompt()
    {
        TextPrompt.CreatePrompt(DisplayResult);
    }

    void DisplayResult(string result)
    {
        text.text = "The input was: " + result;
    }
}
