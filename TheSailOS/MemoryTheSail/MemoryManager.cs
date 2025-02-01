using System;
using System.Collections.Generic;
using System.Linq;

namespace TheSailOS.MemoryTheSail;

public class MemoryManager
{
    private List<MemoryBlock> _memoryBlocks;
    private readonly long _totalMemory;
    private long _usedMemory;

    public MemoryManager(long totalMemory)
    {
        _totalMemory = totalMemory;
        _usedMemory = 0;
        _memoryBlocks = new List<MemoryBlock>();
        InitializeMemory();
    }

    private void InitializeMemory()
    {
        _memoryBlocks.Add(new MemoryBlock(0, (int)_totalMemory));
    }

    public int AllocateMemory(int size)
    {
        var block = _memoryBlocks.FirstOrDefault(b => !b.IsAllocated && b.Size >= size);
        if (block == null)
            throw new OutOfMemoryException("No available memory block of requested size");

        if (block.Size > size)
        {
            var newBlock = new MemoryBlock(block.Address + size, block.Size - size);
            _memoryBlocks.Add(newBlock);
            block.Size = size;
        }

        block.IsAllocated = true;
        _usedMemory += size;
        return block.Address;
    }

    public void FreeMemory(int address)
    {
        var block = _memoryBlocks.FirstOrDefault(b => b.Address == address);
        if (block != null && block.IsAllocated)
        {
            block.IsAllocated = false;
            _usedMemory -= block.Size;
            MergeAdjacentFreeBlocks();
        }
    }

    private void MergeAdjacentFreeBlocks()
    {
        for (int i = 0; i < _memoryBlocks.Count - 1; i++)
        {
            var current = _memoryBlocks[i];
            var next = _memoryBlocks[i + 1];

            if (!current.IsAllocated && !next.IsAllocated)
            {
                current.Size += next.Size;
                _memoryBlocks.RemoveAt(i + 1);
                i--;
            }
        }
    }

    public void DisplayMemoryStats()
    {
        Console.WriteLine($"Total Memory: {_totalMemory} bytes");
        Console.WriteLine($"Used Memory: {_usedMemory} bytes");
        Console.WriteLine($"Free Memory: {_totalMemory - _usedMemory} bytes");
    }
}