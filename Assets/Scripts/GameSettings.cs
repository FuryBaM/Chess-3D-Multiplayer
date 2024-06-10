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
    [SerializeField] private Slider _depthSlider;
    [SerializeField] private TextMeshProUGUI _depthText;
    private void Awake()
    {
        if (PlayerPrefs.HasKey("Depth"))
        {
            _depthSlider.value = PlayerPrefs.GetInt("Depth");
        }
        if (PlayerPrefs.HasKey("SkillLevel"))
        {
            _skillLevelSlider.value = PlayerPrefs.GetInt("SkillLevel");
        }
        if (PlayerPrefs.HasKey("StockfishPath"))
        {
            _stockfishPathInputField.text = PlayerPrefs.GetString("StockfishPath");
        }
    }
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
    public void SetDepth()
    {
        PlayerPrefs.SetInt("Depth", (int)_depthSlider.value);
    }
    public void OnDepthSliderChanged()
    {
        _depthText.text = ((int)_depthSlider.value).ToString();
    }
}
