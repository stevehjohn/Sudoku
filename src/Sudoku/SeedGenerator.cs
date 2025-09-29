using System.Runtime.CompilerServices;

namespace Sudoku;

public static class SeedGenerator
{
    public static int From(int baseSeed, int workerId, int attempt)
    {
        ulong x = (uint) baseSeed;
        
        x ^= (ulong) workerId * 0x9E3779B1u;
        
        x ^= (ulong) attempt * 0x85EBCA6Bu;
        
        return (int) SplitMix64(x);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong SplitMix64(ulong x)
    {
        x += 0x9E3779B97F4A7C15UL;
        
        x = (x ^ (x >> 30)) * 0xBF58476D1CE4E5B9UL;
        
        x = (x ^ (x >> 27)) * 0x94D049BB133111EBUL;
        
        return x ^ (x >> 31);
    }
}