using Steropes.Tiles.Sample.Shared.Strategy.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Steropes.Tiles.Unity2D.Demo.Components.Strategy
{
    public class CityBarWidget : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] Image sizeIcon;
        [SerializeField] Text nameLabel;
        [SerializeField] Text productionLabel;
        [SerializeField] Text productionTurns;
        [SerializeField] Text growthTurns;
#pragma warning restore 649
        RectTransform rectTransform;
        ISettlement settlement;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public RectTransform RectTransform
        {
            get { return rectTransform; }
        }

        public ISettlement Settlement
        {
            get { return settlement; }
            set
            {
                settlement = value;
                UpdateSettlementData();
            }
        }

        public int Epoch { get; set; }

        public void UpdateSettlementData()
        {
            if (settlement == null)
            {
                sizeIcon.sprite = null;
                nameLabel.text = "";
                productionLabel.text = "";
                productionTurns.text = "";
                growthTurns.text = "";
            }
            else
            {
                nameLabel.text = settlement.Name;
                productionLabel.text = "Fireworks";
                productionTurns.text = "-";
                growthTurns.text = settlement.Location.X.ToString();
            }
        }
    }
}