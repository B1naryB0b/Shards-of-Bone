using UnityEngine;
using TMPro;

public class SimpleTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    private float elapsedTime;

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        int integerPart = (int)elapsedTime;
        int decimalPart = (int)((elapsedTime - integerPart) * 100);
        timerText.text = $"{integerPart:D2}.{decimalPart:D2}";
    }
}
