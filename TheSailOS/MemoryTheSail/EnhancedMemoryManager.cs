using System;
using System.Collections.Generic;
using System.Linq;

namespace TheSailOS.MemoryTheSail;

public class EnhancedMemoryManager
{
    private const int PAGE_SIZE = 4096;
    private const int SMALL_OBJECT_THRESHOLD = PAGE_SIZE / 4;
    private const int MEDIUM_OBJECT_THRESHOLD = PAGE_SIZE;

    private byte[] _rat;
    private Dictionary<uint, MemoryBlock> _allocatedBlocks;
    private uint _heapStart;
    private uint _heapSize;
    private int _freePages;

    public EnhancedMemoryManager()
    {
        _heapSize = 1024 * 1024;
        _heapStart = 0x100000;
        _allocatedBlocks = new Dictionary<uint, MemoryBlock>();
        InitializeRAT();
    }

    private void InitializeRAT()
    {
        int pageCount = (int)(_heapSize / PAGE_SIZE);
        _rat = new byte[pageCount];
        _freePages = pageCount;

        for (int i = 0; i < pageCount; i++)
        {
            _rat[i] = 0;
        }
    }

    public uint AllocateMemory(uint size, MemoryPermissions permissions)
    {
        int pageCount = (int)((size + PAGE_SIZE - 1) / PAGE_SIZE);

        if (pageCount > _freePages)
            throw new OutOfMemoryException("Not enough free pages");

        int startPage = FindContiguousPages(pageCount);
        if (startPage == -1)
            throw new OutOfMemoryException("Memory fragmentation");

        uint address = _heapStart + (uint)(startPage * PAGE_SIZE);
        var block = new MemoryBlock
        {
            Address = address,
            Size = size,
            PageCount = pageCount,
            Permissions = permissions,
            IsGCManaged = true
        };

        MarkPagesAsUsed(startPage, pageCount);
        _allocatedBlocks[address] = block;

        return address;
    }

    private int FindContiguousPages(int count)
    {
        int consecutive = 0;
        int startPage = -1;

        for (int i = 0; i < _rat.Length; i++)
        {
            if (_rat[i] == 0)
            {
                if (consecutive == 0) startPage = i;
                consecutive++;
                if (consecutive == count) return startPage;
            }
            else
            {
                consecutive = 0;
                startPage = -1;
            }
        }

        return -1;
    }

    private void MarkPagesAsUsed(int startPage, int count)
    {
        for (int i = 0; i < count; i++)
        {
            _rat[startPage + i] = 1;
        }

        _freePages -= count;
    }

    public void CleanupUnusedMemory()
    {
        if (_freePages < _rat.Length / 4)
        {
            CollectGarbage();
        }
    }

    private void CollectGarbage()
    {
        var markedBlocks = new HashSet<uint>();

        foreach (var block in _allocatedBlocks.Values)
        {
            if (block.IsGCManaged && block.ReferenceCount > 0)
            {
                markedBlocks.Add(block.Address);
            }
        }

        var addresses = _allocatedBlocks.Keys.ToList();
        foreach (var addr in addresses)
        {
            if (!markedBlocks.Contains(addr))
            {
                FreeBlock(addr);
            }
        }
    }

    private void FreeBlock(uint address)
    {
        if (_allocatedBlocks.TryGetValue(address, out var block))
        {
            int startPage = (int)((address - _heapStart) / PAGE_SIZE);
            for (int i = 0; i < block.PageCount; i++)
            {
                _rat[startPage + i] = 0;
            }

            _freePages += block.PageCount;
            _allocatedBlocks.Remove(address);
        }
    }
}