public static class MathfStuff
{
    public static float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget)
    {
        if (fromSource < toSource)
        {
            if (value <= fromSource)
            {
                return fromTarget;
            }

            if (value >= toSource)
            {
                return toTarget;
            }
        }
        else
        {
            if (value >= fromSource)
            {
                return fromTarget;
            }

            if (value <= toSource)
            {
                return toTarget;
            }
        }
        return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
    }
}