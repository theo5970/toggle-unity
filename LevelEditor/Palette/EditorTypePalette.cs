using UnityEngine;

namespace Toggle.LevelEditor.Palette
{
    public class EditorTypePalette : MonoBehaviour
    {
        private PaletteDropdown[] dropdowns;
        private static readonly int[] typeIndices = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20};
    
        public int selectedType { get; private set; }
    
        private void Start()
        {
            int dropdownCount = transform.childCount;
            selectedType = typeIndices[0];

            dropdowns = new PaletteDropdown[dropdownCount];
            for (int i = 0; i < dropdownCount; i++)
            {
                PaletteDropdown dropdown = transform.GetChild(i).GetComponent<PaletteDropdown>();
                dropdown.palette = this;
                dropdown.dropdownIndex = i;
                dropdowns[i] = dropdown;
            }
        }

        public void OnDropdownClicked(int dropdownIndex)
        {
            PaletteDropdown clickedDropdown = dropdowns[dropdownIndex];
            int typeReferenceIndex = clickedDropdown.currentIndex;

            for (int i = 0; i < dropdownIndex; i++)
            {
                PaletteDropdown otherDropdown = dropdowns[i];
                typeReferenceIndex += otherDropdown.itemCount;
            }

            selectedType = typeIndices[typeReferenceIndex];
        }
    }
}