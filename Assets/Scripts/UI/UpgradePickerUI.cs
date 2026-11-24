using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Upgrade picker shown between waves: three neon cards with icons. Tap a card, or
    /// use the d-pad / arrows and A / Enter. Number keys 1-3 pick directly, R or Y rerolls.
    /// </summary>
    public class UpgradePickerUI : MonoBehaviour
    {
        private const float InputDelay = 0.25f;
        private const int CardCount = 3;

        private class Card
        {
            public Button button;
            public Image frame;
            public Image icon;
            public Text rarity, title, level, description;
        }

        public bool IsOpen { get; private set; }

        private RectTransform root;
        private Text titleText;
        private readonly Card[] cards = new Card[CardCount];
        private Button rerollButton;
        private Text rerollText;
        private List<UpgradeDefinition> offer = new List<UpgradeDefinition>();
        private float openTimer;

        private void Awake()
        {
            Build();
        }

        private void OnEnable()
        {
            WaveDirector.upgradeOfferReady += Open;
            WaveDirector.upgradeChosen += Close;
        }

        private void OnDisable()
        {
            WaveDirector.upgradeOfferReady -= Open;
            WaveDirector.upgradeChosen -= Close;
        }

        private void Update()
        {
            if (!IsOpen) return;

            openTimer += Time.unscaledDeltaTime;
            bool ready = openTimer >= InputDelay;
            foreach (Card card in cards) card.button.interactable = ready && card.button.gameObject.activeSelf;
            rerollButton.interactable = ready && WaveDirector.Instance != null && WaveDirector.Instance.RerollsLeft > 0;
            if (!ready) return;

            int number = GameInput.NumberPressed();
            if (number >= 0) Choose(number);
            else if (GameInput.RerollPressed()) Reroll();
        }

        private void Open(List<UpgradeDefinition> newOffer)
        {
            offer = new List<UpgradeDefinition>(newOffer);
            bool wasOpen = IsOpen;
            IsOpen = true;
            root.gameObject.SetActive(true);
            if (!wasOpen) openTimer = 0f;

            WaveDirector director = WaveDirector.Instance;
            titleText.text = director != null && director.IsBossFight == false && director.CurrentPhase == WaveDirector.Phase.Picking
                ? "WAVE " + director.WaveNumber + " CLEAR" : "CHOOSE AN UPGRADE";

            UpgradeInventory inventory = RunManager.Instance != null ? RunManager.Instance.Inventory : null;
            for (int i = 0; i < CardCount; i++)
            {
                bool used = i < offer.Count;
                cards[i].button.gameObject.SetActive(used);
                if (used) Fill(cards[i], offer[i], inventory);
            }

            rerollText.text = "REROLL (" + (director != null ? director.RerollsLeft : 0) + ")";
            UIFactory.Select(cards[0].button);
        }

        private void Close()
        {
            IsOpen = false;
            root.gameObject.SetActive(false);
        }

        private void Choose(int index)
        {
            if (index < 0 || index >= offer.Count || WaveDirector.Instance == null) return;
            WaveDirector.Instance.ChooseUpgrade(offer[index]);
        }

        private void Reroll()
        {
            if (WaveDirector.Instance != null && WaveDirector.Instance.Reroll())
                GameAudio.Play(GameAudio.UiSelect, 0.7f, 0f);
        }

        private static void Fill(Card card, UpgradeDefinition upgrade, UpgradeInventory inventory)
        {
            Color rarityColor = UITheme.RarityColor(upgrade.rarity);
            card.frame.transform.Find("Frame").GetComponent<Image>().color = rarityColor;
            card.icon.sprite = SpriteLibrary.UpgradeIcon(upgrade.id);
            card.icon.color = rarityColor;
            card.icon.enabled = card.icon.sprite != null;
            card.rarity.text = upgrade.rarity.ToString().ToUpper();
            card.rarity.color = rarityColor;
            card.title.text = upgrade.displayName.ToUpper();

            int owned = inventory != null ? inventory.GetStacks(upgrade.id) : 0;
            card.level.text = owned == 0 ? "NEW" : "LEVEL " + (owned + 1) + " / " + upgrade.maxStacks;
            card.level.color = owned == 0 ? UITheme.Yellow : UITheme.Cyan;
            card.description.text = upgrade.description;
        }

        private void Build()
        {
            Canvas canvas = UIFactory.CreateCanvas("Upgrade Picker Canvas", 20, transform);
            root = UIFactory.CreateRect("Upgrade Picker", canvas.transform);
            UIFactory.Stretch(root);
            UIFactory.CreateBackdrop(root);

            titleText = UIFactory.CreateText(root, "CHOOSE AN UPGRADE", 76, UITheme.White, TextAnchor.MiddleCenter);
            UIFactory.Place(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(1600f, 100f));
            Text subtitle = UIFactory.CreateText(root, "Choose one", 36, UITheme.Dim, TextAnchor.MiddleCenter, false, false);
            UIFactory.Place(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(1600f, 50f));

            const float cardWidth = 420f, cardHeight = 600f, gap = 50f;
            float startX = -(cardWidth + gap);
            for (int i = 0; i < CardCount; i++)
            {
                int index = i;
                Card card = new Card();
                card.button = UIFactory.CreateButton(root, "", UITheme.White, () => Choose(index));
                card.frame = (Image)card.button.targetGraphic;
                UIFactory.Place((RectTransform)card.button.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(startX + i * (cardWidth + gap), 10f), new Vector2(cardWidth, cardHeight));

                Transform t = card.button.transform;
                card.rarity = UIFactory.CreateText(t, "", 28, UITheme.White, TextAnchor.MiddleCenter);
                UIFactory.Place(card.rarity.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(380f, 40f));
                card.icon = UIFactory.CreateImage(t, null, UITheme.White);
                UIFactory.Place(card.icon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(170f, 170f));
                card.title = UIFactory.CreateText(t, "", 44, UITheme.White, TextAnchor.MiddleCenter);
                UIFactory.Place(card.title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -265f), new Vector2(390f, 60f));
                card.level = UIFactory.CreateText(t, "", 28, UITheme.Yellow, TextAnchor.MiddleCenter);
                UIFactory.Place(card.level.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -325f), new Vector2(380f, 40f));
                card.description = UIFactory.CreateText(t, "", 30, UITheme.White, TextAnchor.UpperCenter, false, false);
                UIFactory.Place(card.description.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -385f), new Vector2(360f, 190f));
                cards[i] = card;
            }

            rerollButton = UIFactory.CreateButton(root, "REROLL", UITheme.Yellow, Reroll, 36);
            UIFactory.Place((RectTransform)rerollButton.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(380f, 100f));
            rerollText = rerollButton.GetComponentInChildren<Text>();

            root.gameObject.SetActive(false);
        }
    }

}
