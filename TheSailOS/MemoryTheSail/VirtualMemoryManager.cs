using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheSailOS.MemoryTheSail;

public class VirtualMemoryManager
{
    private Dictionary<uint, Page> _pageTable;
    private Queue<Page> _freePages;
    private string _swapFilePath;
    private const uint TOTAL_PAGES = 1024;

    public VirtualMemoryManager()
    {
        _pageTable = new Dictionary<uint, Page>();
        _freePages = new Queue<Page>();
        _swapFilePath = "swap.bin";
        InitializePages();
    }

    private void InitializePages()
    {
        for (uint i = 0; i < TOTAL_PAGES; i++)
        {
            _freePages.Enqueue(new Page
            {
                PageNumber = i,
                PhysicalAddress = i * Page.PAGE_SIZE,
                IsPresent = true,
                Permissions = MemoryPermissions.None
            });
        }
    }

    public uint AllocatePages(int count, MemoryPermissions permissions)
    {
        if (_freePages.Count < count)
            SwapOutPages(count);

        uint baseAddress = 0;
        for (int i = 0; i < count; i++)
        {
            var page = _freePages.Dequeue();
            if (i == 0) baseAddress = page.PageNumber * Page.PAGE_SIZE;

            page.Permissions = permissions;
            page.LastAccessed = DateTime.Now;
            _pageTable[page.PageNumber] = page;
        }

        return baseAddress;
    }

    private void SwapOutPages(int count)
    {
        var pagesToSwap = _pageTable.Values
            .Where(p => p.IsPresent)
            .OrderBy(p => p.LastAccessed)
            .Take(count);

        foreach (var page in pagesToSwap)
        {
            SwapOutPage(page);
            _freePages.Enqueue(page);
        }
    }

    private void SwapOutPage(Page page)
    {
        if (page.IsDirty)
        {
            using (var fs = File.OpenWrite(_swapFilePath))
            {
                fs.Seek(page.PageNumber * Page.PAGE_SIZE, SeekOrigin.Begin);
                // Write page content to swap file
            }
        }

        page.IsPresent = false;
    }

    public void SwapInPage(uint pageNumber)
    {
        var page = _pageTable[pageNumber];
        using (var fs = File.OpenRead(_swapFilePath))
        {
            fs.Seek(pageNumber * Page.PAGE_SIZE, SeekOrigin.Begin);
            // Read page content from swap file
        }

        page.IsPresent = true;
        page.LastAccessed = DateTime.Now;
    }
    
    public uint GetTotalPages()
    {
        return TOTAL_PAGES;
    }

    public uint GetUsedPages()
    {
        return (uint)_pageTable.Count;
    }

    public uint GetFreePages()
    {
        return (uint)_freePages.Count;
    }

    public PageStatistics GetPageStatistics()
    {
        return new PageStatistics
        {
            TotalPages = TOTAL_PAGES,
            UsedPages = GetUsedPages(),
            FreePages = GetFreePages(),
            SwappedPages = (uint)_pageTable.Values.Count(p => !p.IsPresent)
        };
    }
}