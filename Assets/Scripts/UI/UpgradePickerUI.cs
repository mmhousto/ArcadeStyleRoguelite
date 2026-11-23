using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Com.MMsGamingCo.Ricomet
{

    /// <summary>
    /// Gray-box upgrade picker shown between waves: three cards, tap one to take it.
    /// Gamepad: d-pad to move, A to take, X/Y to reroll. Keyboard: 1-3, arrows, Enter, R.
    /// </summary>
    public class UpgradePickerUI : MonoBehaviour
    {
        private const float CardWidth = 380f;
        private const float CardHeight = 460f;
        private const float CardGap = 40f;
        private const float InputDelay = 0.25f;

        private List<UpgradeDefinition> offer = new List<UpgradeDefinition>();
        private bool isOpen = false;
        private int selected = 0;
        private float openTimer = 0f;
        // Only a press that starts after the picker opens can pick a card.
        private bool pressStartedWhileOpen = false;
        private GUIStyle titleStyle, nameStyle, bodyStyle, tagStyle, buttonStyle;

        public bool IsOpen { get { return isOpen; } }

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

        private void Open(List<UpgradeDefinition> newOffer)
        {
            offer = new List<UpgradeDefinition>(newOffer);
            selected = Mathf.Clamp(selected, 0, offer.Count - 1);
            if (!isOpen)
            {
                openTimer = 0f;
                pressStartedWhileOpen = false;
            }
            isOpen = true;
        }

        private void Close()
        {
            isOpen = false;
        }

        private void Update()
        {
            if (!isOpen || WaveDirector.Instance == null) return;

            openTimer += Time.unscaledDeltaTime;
            if (openTimer < InputDelay) return;

            if (GuiInput.PointerPressed) pressStartedWhileOpen = true;

            // Taps
            if (pressStartedWhileOpen && GuiInput.PointerReleased)
            {
                for (int i = 0; i < offer.Count; i++)
                {
                    if (GuiInput.Tapped(CardRect(i)))
                    {
                        Choose(i);
                        return;
                    }
                }
                if (GuiInput.Tapped(RerollRect()))
                    WaveDirector.Instance.Reroll();
            }

            // Gamepad and keyboard
            int move = GuiInput.HorizontalPressed();
            if (move != 0) selected = (selected + move + offer.Count) % offer.Count;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.digit1Key.wasPressedThisFrame) { Choose(0); return; }
                if (Keyboard.current.digit2Key.wasPressedThisFrame) { Choose(1); return; }
                if (Keyboard.current.digit3Key.wasPressedThisFrame) { Choose(2); return; }
            }

            if (GuiInput.ConfirmPressed()) Choose(selected);
            else if (GuiInput.AltPressed()) WaveDirector.Instance.Reroll();
        }

        private void Choose(int index)
        {
            if (index < 0 || index >= offer.Count) return;
            WaveDirector.Instance.ChooseUpgrade(offer[index]);
        }

        private void OnGUI()
        {
            if (!isOpen || WaveDirector.Instance == null) return;

            BuildStyles();
            GUI.matrix = Matrix4x4.Scale(new Vector3(GuiInput.Scale, GuiInput.Scale, 1f));
            float width = GuiInput.Width;

            GUI.color = new Color(0.02f, 0.02f, 0.06f, 0.75f);
            GUI.DrawTexture(new Rect(0f, 0f, width, GuiInput.ReferenceHeight), Texture2D.whiteTexture);

            GUI.color = Color.white;
            GUI.Label(new Rect(0f, 110f, width, 80f), "WAVE " + WaveDirector.Instance.WaveNumber + " CLEAR", titleStyle);
            GUI.color = new Color(1f, 1f, 1f, 0.7f);
            GUI.Label(new Rect(0f, 185f, width, 50f), "Choose an upgrade", bodyCentered());

            UpgradeInventory inventory = RunManager.Instance != null ? RunManager.Instance.Inventory : null;
            for (int i = 0; i < offer.Count; i++)
                DrawCard(CardRect(i), offer[i], inventory, i == selected);

            int rerolls = WaveDirector.Instance.RerollsLeft;
            Color rerollColor = rerolls > 0 ? new Color(1f, 0.9f, 0f) : new Color(0.5f, 0.5f, 0.5f);
            GuiInput.DrawButton(RerollRect(), "Reroll (" + rerolls + ")", rerollColor, false, buttonStyle);

            GUI.color = Color.white;
            GUI.matrix = Matrix4x4.identity;
        }

        private void DrawCard(Rect rect, UpgradeDefinition upgrade, UpgradeInventory inventory, bool highlighted)
        {
            Color rarityColor = RarityColor(upgrade.rarity);

            GUI.color = NeonColors.WithAlpha(rarityColor, highlighted ? 0.22f : 0.1f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GuiInput.DrawOutline(rect, rarityColor, highlighted ? 5f : 3f);

            float x = rect.x + 24f, w = rect.width - 48f;

            GUI.color = rarityColor;
            GUI.Label(new Rect(x, rect.y + 24f, w, 36f), upgrade.rarity.ToString().ToUpper(), tagStyle);

            GUI.color = Color.white;
            GUI.Label(new Rect(x, rect.y + 64f, w, 70f), upgrade.displayName, nameStyle);

            int owned = inventory != null ? inventory.GetStacks(upgrade.id) : 0;
            string level = owned == 0 ? "NEW" : "Level " + (owned + 1) + " / " + upgrade.maxStacks;
            GUI.color = owned == 0 ? new Color(1f, 0.9f, 0f) : new Color(0f, 0.94f, 1f);
            GUI.Label(new Rect(x, rect.y + 134f, w, 36f), level, tagStyle);

            GUI.color = new Color(1f, 1f, 1f, 0.85f);
            GUI.Label(new Rect(x, rect.y + 190f, w, 200f), upgrade.description, bodyStyle);

            GUI.color = new Color(1f, 1f, 1f, 0.4f);
            GUI.Label(new Rect(x, rect.yMax - 56f, w, 36f), string.Join("  ", upgrade.tags).ToUpper(), tagStyle);
        }

        private Rect CardRect(int index)
        {
            float total = offer.Count * CardWidth + (offer.Count - 1) * CardGap;
            float startX = (GuiInput.Width - total) * 0.5f;
            return new Rect(startX + index * (CardWidth + CardGap), 270f, CardWidth, CardHeight);
        }

        private Rect RerollRect()
        {
            return new Rect((GuiInput.Width - 320f) * 0.5f, 780f, 320f, 90f);
        }

        public static Color RarityColor(UpgradeRarity rarity)
        {
            switch (rarity)
            {
                case UpgradeRarity.Rare: return new Color(0.2f, 0.7f, 1f);
                case UpgradeRarity.Epic: return new Color(0.75f, 0.35f, 1f);
                case UpgradeRarity.Legendary: return new Color(1f, 0.8f, 0.1f);
                default: return new Color(0.85f, 0.85f, 0.9f);
            }
        }

        private void BuildStyles()
        {
            if (titleStyle != null) return;

            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 64, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            nameStyle = new GUIStyle(GUI.skin.label) { fontSize = 46, fontStyle = FontStyle.Bold, wordWrap = true };
            bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 32, wordWrap = true };
            tagStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold };
            buttonStyle = new GUIStyle(GUI.skin.label) { fontSize = 36, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            foreach (GUIStyle style in new[] { titleStyle, nameStyle, bodyStyle, tagStyle, buttonStyle })
                style.normal.textColor = Color.white;
        }

        private GUIStyle bodyCentered()
        {
            return new GUIStyle(bodyStyle) { alignment = TextAnchor.MiddleCenter };
        }
    }

}
