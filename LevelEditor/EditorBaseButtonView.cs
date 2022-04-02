using Toggle.Game.Common;

namespace Toggle.LevelEditor
{
    public class EditorBaseButtonView : BaseButtonView
    {
        public override void OnClicked()
        {
            if (LevelEditor.Instance.editorMode == LevelEditorMode.Click)
            {
                base.OnClicked();
            }

            LevelEditor.Instance.OnMapButtonClicked(this);
        }
    }
}