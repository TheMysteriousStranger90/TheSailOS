using System;
using System.Collections.Generic;
using System.Linq;

namespace TheSailOS.MemoryTheSail;

public class EnhancedMemoryManager
{
    private readonly Dictionary<uint, byte[]> _memoryBlocks;
    private uint _nextAddress;
    private readonly uint _totalMemory;

    public EnhancedMemoryManager()
    {
        _memoryBlocks = new Dictionary<uint, byte[]>();
        _nextAddress = 0;
        _totalMemory = 1024 * 1024; // 1MB
    }

    public uint AllocateMemory(uint size, MemoryPermissions permissions)
    {
        if (_nextAddress + size > _totalMemory)
            throw new OutOfMemoryException("Not enough memory");

        var block = new byte[size];
        _memoryBlocks[_nextAddress] = block;
        
        uint allocatedAddress = _nextAddress;
        _nextAddress += size;
        
        return allocatedAddress;
    }

    public void CleanupUnusedMemory()
    {
        // Simple cleanup - just compact memory blocks
        var newBlocks = new Dictionary<uint, byte[]>();
        uint newAddress = 0;
        
        foreach (var block in _memoryBlocks)
        {
            newBlocks[newAddress] = block.Value;
            newAddress += (uint)block.Value.Length;
        }

        _memoryBlocks.Clear();
        foreach (var block in newBlocks)
        {
            _memoryBlocks[block.Key] = block.Value;
        }
        _nextAddress = newAddress;
    }
}