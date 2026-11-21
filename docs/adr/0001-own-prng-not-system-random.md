# Own PRNG instead of System.Random

The engine's only source of randomness is a small, well-known PRNG implemented in this repo (SplitMix64 or xoshiro256**, 64-bit Seed), not `System.Random`. .NET documents that `System.Random`'s algorithm may change between major versions even when seeded, which would break "same Seed → byte-identical Chronicle across machines" (ENGINE_SPEC §1, §3.1). Swapping the generator later changes every Chronicle ever produced, so don't "simplify" this back to `System.Random`.
