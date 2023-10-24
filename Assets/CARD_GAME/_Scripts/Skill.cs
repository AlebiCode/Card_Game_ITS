using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Skill : MonoBehaviour
{
    [SerializeField] private TMP_Text skillName;
    [SerializeField] private Image[] skillDices;

    public TMP_Text SkillName => skillName;
    public Image[] SkillDices => skillDices;

}
