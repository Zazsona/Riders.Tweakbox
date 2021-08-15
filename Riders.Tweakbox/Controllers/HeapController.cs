﻿using System.Runtime.InteropServices;
using Reloaded.Hooks.Definitions;
using Riders.Tweakbox.Controllers.Interfaces;
using Riders.Tweakbox.Misc;
using Sewer56.SonicRiders.API;
using Sewer56.SonicRiders.Structures.Functions;
using static Riders.Tweakbox.Misc.Log;
namespace Riders.Tweakbox.Controllers;

public class HeapController : IController
{
    /// <summary>
    /// Pointer to the last allocated memory block.
    /// </summary>
    public unsafe MallocResult* FirstAllocResult { get; private set; }

    private static HeapController _instance;
    private IHook<Heap.AllocFnPtr> _mallocHook;
    private IHook<Heap.AllocFnPtr> _callocHook;
    private IHook<Heap.FreeFnPtr> _freeHook;
    private IHook<Heap.FreeFrameFnPtr> _freeFrameHook;

    public HeapController(IReloadedHooks hooks)
    {
        _instance = this;
        _mallocHook = Heap.Malloc.HookAs<Heap.AllocFnPtr>(typeof(HeapController), nameof(MallocImplStatic)).Activate();
        _callocHook = Heap.Calloc.HookAs<Heap.AllocFnPtr>(typeof(HeapController), nameof(CallocImplStatic)).Activate();
        _freeHook = Heap.Free.HookAs<Heap.FreeFnPtr>(typeof(HeapController), nameof(FreeImplStatic)).Activate();
        _freeFrameHook = Heap.FreeFrame.HookAs<Heap.FreeFrameFnPtr>(typeof(HeapController), nameof(FreeFrameImplStatic)).Activate();
    }

    private unsafe int FreeFrameImpl(MallocResult* address)
    {
        if (IsEnabled(LogCategory.Heap)) WriteLine($"FreeFrame: {(long)address:X}", LogCategory.Heap);
        var result = _freeFrameHook.OriginalFunction.Value.Invoke(address);

        return result;
    }

    private unsafe MallocResult* FreeImpl(MallocResult* address)
    {
        if (IsEnabled(LogCategory.Heap)) WriteLine($"Free: {(long)address:X}", LogCategory.Heap);
        var result = _freeHook.OriginalFunction.Value.Invoke(address).Pointer;

        // Erase the contents of the allocation header.
        // This is necessary for our heap walker.
        var header = result->GetHeader(result);
        header->Base = (MallocResult*)0;
        header->AllocationSize = 0;

        return result;
    }

    private unsafe MallocResult* CallocImpl(int alignment, int size)
    {
        var result = _callocHook.OriginalFunction.Value.Invoke(alignment, size).Pointer;
        var header = result->GetHeader(result);
        if (header->Base == *Heap.FirstHeaderFront)
            FirstAllocResult = result;

        if (IsEnabled(LogCategory.Heap)) WriteLine($"Calloc: {(long)result:X} | Alignment {alignment}, Size {size}", LogCategory.Heap);
        if (IsEnabled(LogCategory.Heap)) WriteLine($"Header [{(long)header:X}] | Base: {(long)header->Base:X}, Size: {header->AllocationSize}", LogCategory.Heap);
        return result;
    }

    private unsafe MallocResult* MallocImpl(int alignment, int size)
    {
        var result = _mallocHook.OriginalFunction.Value.Invoke(alignment, size).Pointer;
        var header = result->GetHeader(result);
        if (header->Base == *Heap.FirstHeaderFront)
            FirstAllocResult = result;

        if (IsEnabled(LogCategory.Heap)) WriteLine($"Malloc: {(long)result:X} | Alignment {alignment}, Size {size}", LogCategory.Heap);
        if (IsEnabled(LogCategory.Heap)) WriteLine($"Header [{(long)header:X}] | Base: {(long)header->Base:X}, Size: {header->AllocationSize}", LogCategory.Heap);
        return result;
    }

    #region Static Entry Points
    [UnmanagedCallersOnly]
    private static unsafe MallocResult* MallocImplStatic(int alignment, int size) => _instance.MallocImpl(alignment, size);

    [UnmanagedCallersOnly]
    private static unsafe MallocResult* CallocImplStatic(int alignment, int size) => _instance.CallocImpl(alignment, size);

    [UnmanagedCallersOnly]
    private static unsafe MallocResult* FreeImplStatic(MallocResult* address) => _instance.FreeImpl(address);

    [UnmanagedCallersOnly]
    private static unsafe int FreeFrameImplStatic(MallocResult* address) => _instance.FreeFrameImpl(address);
    #endregion
}
