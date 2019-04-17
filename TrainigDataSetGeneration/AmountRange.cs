namespace TrainigDataSetGeneration
{
    public struct AmountRange
    {
        public int Start { get; }
        public int End { get; }

        public AmountRange(int start, int end)
        {
            Start = start;
            End = end;
        }
    }
}