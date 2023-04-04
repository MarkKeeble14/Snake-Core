using System.Collections;
using System.Collections.Generic;

public class CardSelectionTriggerEvent : TriggerEvent
{
    public override void Activate()
    {
        SnakeBehaviour._Instance.ResetMoveTimer();
        UIManager._Instance.OpenSelectionScreen();
    }
}
