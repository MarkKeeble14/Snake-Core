using System.Collections;
using System.Collections.Generic;

public class CardSelectionTriggerEvent : TriggerEvent
{
    public override void Activate()
    {
        UIManager._Instance.OpenSelectionScreen();
    }
}
