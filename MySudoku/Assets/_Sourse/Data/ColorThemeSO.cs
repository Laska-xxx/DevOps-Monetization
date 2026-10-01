using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "ColorTheme", menuName = "Sudoku/ColorTheme")]
    public class ColorThemeSO : ScriptableObject
    {
        [field: Header("Общее")]
        [field: SerializeField] public string ThemeId { get; private set; } = "default";
        [field: SerializeField] public Color Background { get; private set; } = Color.white;

        [field: Header("Кнопки")]
        [field: SerializeField] public Color ConfirmButton { get; private set; } = new Color(0.3f, 0.75f, 0.4f);
        [field: SerializeField] public Color CancelButton { get; private set; } = new Color(0.8f, 0.35f, 0.35f);
        [field: SerializeField] public Color NumpadButton { get; private set; } = Color.white;
        [field: SerializeField] public Color NumpadButtonLocked { get; private set; } = new Color(0.6f, 0.6f, 0.6f);

        [field: Header("Панели")]
        [field: SerializeField] public Color PopupPanel { get; private set; } = Color.white;

        [field: Header("Доска")]
        [field: SerializeField] public Color CellBackground { get; private set; } = Color.white;
        [field: SerializeField] public Color CellHighlight { get; private set; } = new Color(0.85f, 0.90f, 1f);
        [field: SerializeField] public Color CellSameValue { get; private set; } = new Color(0.75f, 0.85f, 1f);
        [field: SerializeField] public Color CellError { get; private set; } = new Color(1f, 0.6f, 0.6f);
        [field: SerializeField] public Color FixedDigitText { get; private set; } = Color.black;
        [field: SerializeField] public Color FilledDigitText { get; private set; } = new Color(0.1f, 0.3f, 0.8f);
        [field: SerializeField] public Color GridBorder { get; private set; } = Color.black;
    }
}
