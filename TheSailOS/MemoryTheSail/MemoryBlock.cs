namespace TheSailOS.MemoryTheSail;

public class MemoryBlock
{
    public int Address { get; set; }
    public int Size { get; set; }
    public bool IsAllocated { get; set; }

    public MemoryBlock(int address, int size)
    {
        Address = address;
        Size = size;
        IsAllocated = false;
    }
}