using System;
using System.Collections.Generic;

/// <summary>
/// Everything the pure evaluator needs, injected as delegates so this file stays free of Unity
/// and can be exercised in edit-mode tests. A null delegate makes that source unsatisfiable.
/// </summary>
public class StoryConditionContext
{
    /// <summary>Returns the current amount for a resource condition (scope already baked into the condition).</summary>
    public Func<StoryCondition, float> ResourceReader;

    /// <summary>Returns the current value of a Yarn variable (float/bool/string), or null if unknown.</summary>
    public Func<string, object> YarnReader;

    /// <summary>Total in-game minutes elapsed since the start of the campaign (GameClock.Now).</summary>
    public long Now;

    /// <summary>Current in-game day (GameClock.Day).</summary>
    public long NowDay;
}

/// <summary>
/// Deterministic gate evaluation and weighted sibling selection. No Unity dependency.
/// </summary>
public static class StoryConditionEvaluator
{
    private const double Epsilon = 1e-4;

    public static bool Evaluate(StoryCondition condition, StoryConditionContext context)
    {
        if (condition == null)
            return true;
        if (context == null)
            return false;

        switch (condition.Source)
        {
            case StoryConditionSource.Resource:
                return EvaluateResource(condition, context);
            case StoryConditionSource.YarnVariable:
                return EvaluateYarn(condition, context);
            case StoryConditionSource.Time:
                return EvaluateTime(condition, context);
            default:
                return true;
        }
    }

    public static bool EvaluateAll(IList<StoryCondition> conditions, StoryConditionContext context)
    {
        if (conditions == null)
            return true;
        for (int i = 0; i < conditions.Count; i++)
            if (!Evaluate(conditions[i], context))
                return false;
        return true;
    }

    /// <summary>
    /// A node is eligible when every condition passes and its date lower bound has been reached.
    /// The date is retroactively satisfied once the clock has passed it; it never fires on its own.
    /// </summary>
    public static bool NodeGatePassed(StoryNode node, StoryConditionContext context)
    {
        if (node == null || context == null)
            return false;
        if (node.HasDate && context.Now < node.DateTick())
            return false;
        return EvaluateAll(node.Conditions, context);
    }

    public static bool CompareNumbers(double value, StoryComparison comparison, double target)
    {
        switch (comparison)
        {
            case StoryComparison.Less: return value < target;
            case StoryComparison.LessOrEqual: return value <= target;
            case StoryComparison.Equal: return Math.Abs(value - target) < Epsilon;
            case StoryComparison.NotEqual: return Math.Abs(value - target) >= Epsilon;
            case StoryComparison.GreaterOrEqual: return value >= target;
            case StoryComparison.Greater: return value > target;
            default: return false;
        }
    }

    private static bool EvaluateResource(StoryCondition condition, StoryConditionContext context)
    {
        if (context.ResourceReader == null)
            return false;
        float value = context.ResourceReader(condition);
        return CompareNumbers(value, condition.Comparison, condition.Amount);
    }

    private static bool EvaluateTime(StoryCondition condition, StoryConditionContext context)
    {
        double now = condition.TimeUseDay ? context.NowDay : context.Now;
        return CompareNumbers(now, condition.Comparison, condition.TimeValue);
    }

    private static bool EvaluateYarn(StoryCondition condition, StoryConditionContext context)
    {
        if (context.YarnReader == null)
            return false;
        object raw = context.YarnReader(condition.Variable);
        if (raw == null)
            return false;

        switch (condition.ValueKind)
        {
            case StoryValueKind.Number:
                if (!TryToDouble(raw, out double number))
                    return false;
                return CompareNumbers(number, condition.Comparison, condition.NumberValue);
            case StoryValueKind.Bool:
                if (!TryToBool(raw, out bool boolean))
                    return false;
                return EqualsComparison(boolean == condition.BoolValue, condition.Comparison);
            case StoryValueKind.String:
                string text = raw as string ?? raw.ToString();
                return EqualsComparison(string.Equals(text, condition.StringValue, StringComparison.Ordinal), condition.Comparison);
            default:
                return false;
        }
    }

    private static bool EqualsComparison(bool equal, StoryComparison comparison)
    {
        switch (comparison)
        {
            case StoryComparison.Equal: return equal;
            case StoryComparison.NotEqual: return !equal;
            default: return false;
        }
    }

    private static bool TryToBool(object raw, out bool result)
    {
        if (raw is bool b)
        {
            result = b;
            return true;
        }
        result = false;
        return false;
    }

    private static bool TryToDouble(object raw, out double result)
    {
        if (raw is float f) { result = f; return true; }
        if (raw is int i) { result = i; return true; }
        if (raw is long l) { result = l; return true; }
        if (raw is double d) { result = d; return true; }
        if (raw is string s && double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double parsed))
        {
            result = parsed;
            return true;
        }
        result = 0d;
        return false;
    }

    /// <summary>
    /// Picks one candidate by weight. Weight 0 candidates are excluded unless every weight is 0,
    /// in which case the pick is uniform. Used when several siblings become eligible at once.
    /// </summary>
    public static StoryNode PickWeighted(IList<StoryNode> candidates, Random rng)
    {
        if (candidates == null || candidates.Count == 0)
            return null;
        if (rng == null)
            rng = new Random();

        double total = 0d;
        for (int i = 0; i < candidates.Count; i++)
        {
            float weight = candidates[i] != null ? candidates[i].Weight : 0f;
            if (weight > 0f)
                total += weight;
        }

        if (total <= 0d)
            return candidates[rng.Next(candidates.Count)];

        double roll = rng.NextDouble() * total;
        for (int i = 0; i < candidates.Count; i++)
        {
            float weight = candidates[i] != null ? candidates[i].Weight : 0f;
            if (weight <= 0f)
                continue;
            roll -= weight;
            if (roll <= 0d)
                return candidates[i];
        }
        return candidates[candidates.Count - 1];
    }
}
