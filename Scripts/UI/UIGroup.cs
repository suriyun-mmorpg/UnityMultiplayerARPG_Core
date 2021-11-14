public class UIGroup : UIBase
{
    public UIBase[] subSet;

    public override void Show()
    {
        foreach (UIBase entry in subSet)
        {
            entry.Show();
        }
        base.Show();
    }

    public override void Hide()
    {
        foreach (UIBase entry in subSet)
        {
            entry.Hide();
        }
        base.Hide();
    }
}
