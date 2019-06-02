using System.Collections;

namespace Aspekt.UI
{
    public interface IUIAnimator
    {
        IEnumerator AnimateIn();
        IEnumerator AnimateOut();
    }
}