namespace Game.Core
{
    public sealed class Cell
    {
        public bool IsOpen { get; internal set; }
        public ItemType Item { get; internal set; }

        public Cell(bool isOpen, ItemType item)
        {
            IsOpen = isOpen;
            Item = item;
        }

        public Cell Clone() => new Cell(IsOpen, Item);
    }
}
