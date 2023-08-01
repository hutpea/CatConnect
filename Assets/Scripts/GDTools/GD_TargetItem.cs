using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CatConnect
{
    public class GD_TargetItem : MonoBehaviour
    {
        public bool isSelected;
        private Button selectBtn;

        private void Awake()
        {
            selectBtn = transform.Find("selectPanel").GetComponent<Button>();    
        }

        private void Start()
        {
            isSelected = false;
            selectBtn.onClick.AddListener(SelectThisItem);
        }

        private void SelectThisItem()
        {
            if (!isSelected)
            {
                isSelected = true;
                selectBtn.transform.GetComponent<Image>().color = Color.green;
            }
            else
            {
                isSelected = false;
                selectBtn.transform.GetComponent<Image>().color = Color.white;
            }
        }
    }
}
