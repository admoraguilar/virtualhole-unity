using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Flexbox4Unity
{
    [Obsolete("Use FlexDirection instead, it's closer to the official CSS name")]
    public enum FlexboxDirection
    {
        ROW,
        COLUMN,
        ROW_REVERSED,
        COLUMN_REVERSED
    }

    [Obsolete("Use FlexJustify instead, it's closer to the official CSS name")]
    public enum FlexboxJustify
    {
        START,
        END,
        CENTER,
        SPACE_BETWEEN,
        SPACE_AROUND,
        SPACE_EVENLY
    }

    [Obsolete("Use AlignItems or AlignSelf instead, depending on context")]
    public enum FlexboxAlign
    {
        START,
        END,
        CENTER,
        STRETCH,
        BASELINE
    }

    public enum BoxSizing
    {
        BORDER_BOX = 0,
        CONTENT_BOX = 1 // not supported yet - this is almost never useful in Unity, since it makes padding very hard to use
    }

    public enum FlexDirection
    {
        ROW,
        COLUMN,
        ROW_REVERSED,
        COLUMN_REVERSED
    }

    public enum FlexJustify
    {
        START,
        END,
        CENTER,
        SPACE_BETWEEN,
        SPACE_AROUND,
        SPACE_EVENLY
    }

    public enum FlexWrap
    {
        NOWRAP,
        WRAP,
        //WRAP_REVERSE // not supported yet
    }

    public enum AlignItems
    {
        START,
        END,
        CENTER,
        BASELINE,
        STRETCH
    }

    public enum AlignSelf
    {
        AUTO, // this is the only difference between the AlignItems and AlignSelf types
        START,
        END,
        CENTER,
        BASELINE,
        STRETCH
    }
}