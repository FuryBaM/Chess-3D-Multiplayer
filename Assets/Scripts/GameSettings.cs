using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    [SerializeField] private InputField _stockfishPathInputField;
    [SerializeField] private Slider _skillLevelSlider;
    [SerializeField] private TextMeshProUGUI _skillLevelText;
    public void SetStockfishPath()
    {
        if (!File.Exists(_stockfishPathInputField.text))
        {
            return;
        }
        PlayerPrefs.SetString("StockfishPath", _stockfishPathInputField.text);
    }
    public void SetSkillLevel()
    {
        PlayerPrefs.SetInt("SkillLevel", (int)_skillLevelSlider.value);
    }
    public void OnSkillSliderChanged()
    {
        _skillLevelText.text = ((int)_skillLevelSlider.value).ToString();
    }
}
