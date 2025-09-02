namespace Pro4Soft.BackgroundWorker.Execution;

public class FilterRule
{
    public ConditionType Condition { get; set; }
    public string Field { get; set; }
    public Operator Operator { get; set; }
    public dynamic Value { get; set; }
    public List<FilterRule> Rules { get; set; }

    public string ToOdataQuery()
    {
        var rules = (Rules ?? []).Where(c => c != null).ToList();
        if (rules.Count > 0)
        {
            var condition = Condition switch
            {
                ConditionType.And => "and",
                ConditionType.Or => "or",
                _ => throw new ArgumentOutOfRangeException()
            };

            if (rules.Count == 1)
                return rules[0].ToOdataQuery();

            return string.Join($" {condition} ", rules.Select(c => $"{c.ToOdataQuery()}"));
        }

        var op = Operator switch
        {
            Operator.Eq => "eq",
            Operator.Ne => "ne",
            Operator.Gt => "gt",
            Operator.Ge => "ge",
            Operator.Lt => "lt",
            Operator.Le => "le",
            _ => throw new ArgumentOutOfRangeException()
        };

        return $"{Field} {op} {Value}";
    }
}

public enum ConditionType
{
    And,
    Or
}

public enum Operator
{
    Eq,
    Ne,
    Gt,
    Ge,
    Lt,
    Le
}