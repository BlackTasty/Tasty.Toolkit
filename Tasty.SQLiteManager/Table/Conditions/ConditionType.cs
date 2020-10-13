namespace Tasty.SQLiteManager.Table.Conditions
{
    /// <summary>
    /// The type of a condition
    /// </summary>
    public enum ConditionType
    {
        /// <summary>
        /// Both the left and right condition must be met
        /// </summary>
        AND,
        /// <summary>
        /// Either the left or the right condition must be met
        /// </summary>
        OR,
        /// <summary>
        /// 
        /// </summary>
        NONE
    }
}
